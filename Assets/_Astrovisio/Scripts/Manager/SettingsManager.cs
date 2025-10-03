using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private APIManager apiManager;
        [SerializeField] private ProjectManager projectManager;

        // === Data ===
        private readonly Dictionary<ProjectFile, Settings> settingsDictionary = new();


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of SettingsManager found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (projectManager == null)
            {
                Debug.LogError("[SettingsManager] ProjectManager reference is missing.");
                return;
            }

            projectManager.FileProcessed += OnFileProcessed;
            projectManager.ProjectClosed += OnProjectClosed;
        }

        private void OnDestroy()
        {
            if (projectManager != null)
            {
                projectManager.FileProcessed -= OnFileProcessed;
                projectManager.ProjectClosed -= OnProjectClosed;
            }
        }


        private async void OnFileProcessed(Project project, File file, DataPack pack)
        {
            if (project == null || file == null)
                return;

            try
            {
                Settings settings = await GetSettings(project.Id, file.Id);
                Debug.LogWarning(JsonConvert.SerializeObject(settings));

                settings.SetDefaults();
                Debug.LogError(JsonConvert.SerializeObject(settings));

                ProjectFile key = new ProjectFile(project.Id, file.Id);
                AddSettings(project.Id, file.Id, settings);
                // settingsDictionary[key] = settings;

                Settings updatedSettings = await UpdateSettings(project.Id, file.Id);
                Debug.LogWarning(JsonConvert.SerializeObject(updatedSettings));

                Debug.Log($"[SettingsManager] Settings cached for {key} " + $"(var count: {settingsDictionary[key]?.Variables?.Count ?? 0}).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsManager] OnFileProcessed -> GetSettings failed for P{project.Id}-F{file.Id}: {ex.Message}");
            }
        }

        private void OnProjectClosed(Project project)
        {
            if (project == null)
            {
                return;
            }

            List<ProjectFile> toRemove = new List<ProjectFile>();
            foreach (KeyValuePair<ProjectFile, Settings> kv in settingsDictionary)
            {
                if (kv.Key.ProjectId == project.Id)
                {
                    toRemove.Add(kv.Key);
                }
            }
            foreach (ProjectFile k in toRemove)
            {
                settingsDictionary.Remove(k);
            }

            Debug.Log($"[SettingsManager] Project {project.Id} closed. Removed {toRemove.Count} settings entries.");
        }

        public bool TryGetSettings(int projectId, int fileId, out Settings settings)
        {
            return settingsDictionary.TryGetValue(new ProjectFile(projectId, fileId), out settings);
        }

        public void AddSettings(int projectId, int fileId, Settings settings)
        {
            if (settings == null)
            {
                return;
            }

            ProjectFile key = new ProjectFile(projectId, fileId);
            settingsDictionary.Remove(key);
            settingsDictionary.Add(key, settings);

            int count = settingsDictionary[key]?.Variables?.Count ?? 0;
            Debug.Log($"[SettingsManager] Inserted settings entry for {key} (var count: {count}).");
        }

        public bool RemoveSettings(int projectId, int fileId)
        {
            ProjectFile key = new ProjectFile(projectId, fileId);
            bool removed = settingsDictionary.Remove(key);

            if (removed)
            {
                Debug.Log($"[SettingsManager] Removed settings entry for {key}.");
            }
            else
            {
                Debug.LogWarning($"[SettingsManager] No settings entry found for {key} to remove.");
            }

            return removed;
        }

        public async Task<Settings> GetSettings(int projectId, int fileId)
        {
            if (apiManager == null)
            {
                Debug.LogError("[SettingsManager] APIManager reference is missing.");
                return null;
            }

            TaskCompletionSource<Settings> tcs = new TaskCompletionSource<Settings>();

            _ = apiManager.GetSettings(
                projectId, fileId,
                onSuccess: s => tcs.TrySetResult(s),
                onError: err =>
                {
                    Debug.LogError("[SettingsManager] GetSettings error: " + err);
                    tcs.TrySetException(new Exception(err));
                });

            return await tcs.Task;
        }

        public async Task<Settings> UpdateSettings(int projectId, int fileId)
        {
            if (apiManager == null)
            {
                Debug.LogError("[SettingsManager] APIManager reference is missing.");
                return null;
            }

            ProjectFile key = new ProjectFile(projectId, fileId);
            if (!settingsDictionary.TryGetValue(key, out Settings current) || current?.Variables == null)
            {
                Debug.LogWarning($"[SettingsManager] No cached settings for {key}. " + "Run GetSettings (or wait for FileProcessed) before UpdateSettings.");
                return null;
            }

            UpdateSettingsRequest req = new UpdateSettingsRequest
            {
                Variables = current.Variables ?? new List<Setting>()
            };

            TaskCompletionSource<Settings> tcs = new TaskCompletionSource<Settings>();

            _ = apiManager.UpdateSettings(
                projectId, fileId, req,
                onSuccess: updated =>
                {
                    settingsDictionary[key] = updated ?? new Settings { Variables = new List<Setting>() };
                    tcs.TrySetResult(updated);
                },
                onError: err =>
                {
                    Debug.LogError("[SettingsManager] UpdateSettings error: " + err);
                    tcs.TrySetException(new Exception(err));
                });

            return await tcs.Task;
        }

    }

}
