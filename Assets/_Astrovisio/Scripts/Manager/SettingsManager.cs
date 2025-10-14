using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogData;
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

            projectManager.ProjectOpened += OnProjectOpened;
            projectManager.ProjectClosed += OnProjectClosed;
            // projectManager.FileProcessed += OnFileProcessed;
            projectManager.FileUpdated += OnFileUpdated;
        }

        private void OnDestroy()
        {
            if (projectManager != null)
            {
                // projectManager.FileProcessed -= OnFileProcessed;
                projectManager.ProjectClosed -= OnProjectClosed;
                projectManager.FileUpdated -= OnFileUpdated;
            }
        }

        // === Events ===
        private async void OnFileProcessed(Project project, File file, DataPack pack)
        {
            if (project == null || file == null)
            {
                return;
            }

            try
            {
                RemoveSettings(project.Id, file.Id);

                Settings settings = await GetSettings(project.Id, file.Id);
                // Debug.LogWarning(JsonConvert.SerializeObject(settings));

                // Debug.LogWarning((settings != null) + " " + settings.Variables.Count);
                if (settings != null && settings.Variables.Count == 0)
                {
                    settings.SetDefaults(file);
                    // Debug.LogError(JsonConvert.SerializeObject(settings));

                    Settings updatedSettings = await UpdateSettings(project.Id, file.Id);
                    Debug.LogError(JsonConvert.SerializeObject(updatedSettings));

                    Debug.Log($"[SettingsManager] Settings cached for {new ProjectFile(project.Id, file.Id)} " + $"(var count: {settingsDictionary[new ProjectFile(project.Id, file.Id)]?.Variables?.Count ?? 0}).");
                }

                ProjectFile key = new ProjectFile(project.Id, file.Id);
                AddSettings(project.Id, file.Id, settings);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsManager] OnFileProcessed -> GetSettings failed for P{project.Id}-F{file.Id}: {ex.Message}");
            }
        }

        private async void OnFileUpdated(Project project, File file)
        {

            if (project == null || file == null)
            {
                return;
            }

            try
            {
                // Debug.Log(">>>>>>>>> OnFileUpdated " + file.Path);
                RemoveSettings(project.Id, file.Id);

                Settings settings = await GetSettings(project.Id, file.Id);
                // Debug.LogWarning(JsonConvert.SerializeObject(settings));

                // Debug.LogWarning((settings != null) + " " + settings.Variables.Count);
                if (settings != null && settings.Variables.Count == 0)
                {
                    settings.SetDefaults(file);
                    // Debug.LogError(JsonConvert.SerializeObject(settings));

                    // Settings updatedSettings = await UpdateSettings(project.Id, file.Id);
                    // Debug.LogError(JsonConvert.SerializeObject(updatedSettings));

                    Debug.Log($"[SettingsManager] Settings cached for {new ProjectFile(project.Id, file.Id)} " + $"(var count: {settingsDictionary[new ProjectFile(project.Id, file.Id)]?.Variables?.Count ?? 0}).");
                }

                ProjectFile key = new ProjectFile(project.Id, file.Id);
                AddSettings(project.Id, file.Id, settings);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsManager] OnFileProcessed -> GetSettings failed for P{project.Id}-F{file.Id}: {ex.Message}");
            }
        }

        private void OnProjectOpened(Project project)
        {
            Debug.Log("OnProjectOpened");
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

        // === Data ===
        public bool TryGetSettings(int projectId, int fileId, out Settings settings)
        {
            return settingsDictionary.TryGetValue(new ProjectFile(projectId, fileId), out settings);
        }

        public Setting GetSetting(int projectId, int fileId, string varName)
        {
            if (string.IsNullOrWhiteSpace(varName))
            {
                return null;
            }

            if (!settingsDictionary.TryGetValue(new ProjectFile(projectId, fileId), out Settings settings) ||
                settings?.Variables == null || settings.Variables.Count == 0)
            {
                Debug.LogWarning($"[SettingsManager] No settings cached for P{projectId}-F{fileId} or no variables present.");
                return null;
            }

            return settings.Variables.Find(s => string.Equals(s.Name, varName, StringComparison.OrdinalIgnoreCase));
        }

        public bool AddSetting(int projectId, int fileId, Setting setting, bool overwrite = true)
        {
            if (setting == null || string.IsNullOrWhiteSpace(setting.Name))
            {
                Debug.LogError("[SettingsManager] AddSetting failed: 'setting' is null or has no Name.");
                return false;
            }

            var key = new ProjectFile(projectId, fileId);

            // Ensure a Settings container exists for this key
            if (!settingsDictionary.TryGetValue(key, out var settings) || settings == null)
            {
                settings = new Settings { Variables = new List<Setting>() };
                settingsDictionary[key] = settings;
                Debug.Log($"[SettingsManager] Created new Settings container for {key}.");
            }

            settings.Variables ??= new List<Setting>();

            // Find existing by name (case-insensitive)
            int idx = settings.Variables.FindIndex(s =>
                s != null && string.Equals(s.Name, setting.Name, StringComparison.OrdinalIgnoreCase));

            // If exists and overwrite is false -> skip
            if (idx >= 0 && !overwrite)
            {
                Debug.LogWarning($"[SettingsManager] AddSetting skipped: '{setting.Name}' already exists for {key} and overwrite=false.");
                return false;
            }

            // Determine mapping to enforce uniqueness on
            string selectedMapping = setting.Mapping;
            bool enforceUniqueness =
                !string.IsNullOrWhiteSpace(selectedMapping) &&
                (selectedMapping.Equals("Opacity", StringComparison.OrdinalIgnoreCase) ||
                 selectedMapping.Equals("Colormap", StringComparison.OrdinalIgnoreCase));

            // If we will apply this setting (insert or update), and mapping is Opacity/Colormap,
            // clear the same mapping from all other settings in this file.
            if (enforceUniqueness)
            {
                foreach (var s in settings.Variables)
                {
                    if (s == null) continue;
                    if (string.Equals(s.Name, setting.Name, StringComparison.OrdinalIgnoreCase)) continue;

                    if (string.Equals(s.Mapping, selectedMapping, StringComparison.OrdinalIgnoreCase))
                    {
                        s.Mapping = null; // or "None" if preferisci un valore esplicito
                                          // (opzionale) reset di altri campi legati al mapping
                                          // es: s.InvertMapping = false; s.Colormap = null; ecc. se serve
                    }
                }
            }

            // Apply insert or update
            if (idx >= 0)
            {
                settings.Variables[idx] = setting;
                Debug.Log($"[SettingsManager] Updated setting '{setting.Name}' for {key} (mapping: {setting.Mapping ?? "None"}).");
            }
            else
            {
                settings.Variables.Add(setting);
                Debug.Log($"[SettingsManager] Added setting '{setting.Name}' for {key} (mapping: {setting.Mapping ?? "None"}).");
            }

            return true;
        }


        public bool RemoveSetting(int projectId, int fileId, string varName)
        {
            if (string.IsNullOrWhiteSpace(varName))
            {
                Debug.LogError("[SettingsManager] RemoveSetting failed: 'varName' is null or empty.");
                return false;
            }

            var key = new ProjectFile(projectId, fileId);

            if (!settingsDictionary.TryGetValue(key, out var settings) || settings?.Variables == null)
            {
                Debug.LogWarning($"[SettingsManager] RemoveSetting: no cached Settings or Variables for {key}.");
                return false;
            }

            int idx = settings.Variables.FindIndex(s =>
                s != null && string.Equals(s.Name, varName, StringComparison.OrdinalIgnoreCase));

            if (idx < 0)
            {
                Debug.LogWarning($"[SettingsManager] RemoveSetting: '{varName}' not found for {key}.");
                return false;
            }

            settings.Variables.RemoveAt(idx);
            Debug.Log($"[SettingsManager] Removed setting '{varName}' for {key}.");
            return true;
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

        public List<Settings> GetCurrentProjectFilesSettings()
        {
            List<Settings> filesSettings = new List<Settings>();

            foreach (File file in projectManager.GetCurrentProject().Files)
            {
                Settings settings;
                if (TryGetSettings(projectManager.GetCurrentProject().Id, file.Id, out settings))
                {
                    settings.Path = file.Path;
                    filesSettings.Add(settings);
                }
            }

            return filesSettings;
        }

        // === Render ===
        public void SetAxisSetting(Axis axis, Setting setting)
        {
            if (setting == null)
            {
                Debug.LogError("[SettingsManager] Setting è null.");
                return;
            }

            if (!Enum.TryParse(setting.Scaling, ignoreCase: true, out ScalingType scalingType))
            {
                Debug.LogWarning($"[SettingsManager] Scaling '{setting.Scaling}' not valid. Using default: {ScalingType.Linear}");
                scalingType = ScalingType.Linear;
            }

            float thrMin = (float)(setting.ThrMinSel ?? setting.ThrMin);
            float thrMax = (float)(setting.ThrMaxSel ?? setting.ThrMax);

            RenderManager.Instance.DataRenderer.SetAxisAstrovisio(
                axis,
                setting.Name,
                thrMin,
                thrMax,
                scalingType
            );
        }

        public void SetParamSetting(Setting setting, Settings settings = null)
        {
            switch (setting.Mapping)
            {
                case null:
                    Debug.Log("None: " + setting.Mapping);
                    break;
                case "Opacity":
                    SetOpacity(setting);
                    break;
                case "Colormap":
                    Debug.Log("1");
                    SetColormap(setting);
                    break;
                default:
                    break;
            }
        }

        public void SetSettings(int projectId, int fileId)
        {
            if (TryGetSettings(projectId, fileId, out var settings) == false)
            {
                return;
            }

            File file = projectManager.GetFile(projectId, fileId);
            string xAxisName = file.Variables.FirstOrDefault(v => v.XAxis)?.Name;
            string yAxisName = file.Variables.FirstOrDefault(v => v.YAxis)?.Name;
            string zAxisName = file.Variables.FirstOrDefault(v => v.ZAxis)?.Name;


            RemoveOpacity();
            RemoveColormap();
            foreach (Setting setting in settings.Variables)
            {
                // Debug.LogWarning($"{setting.Name} - {setting.Mapping}");

                if (setting.Name == xAxisName)
                {
                    ScalingType scalingType = Enum.TryParse<ScalingType>(setting.Scaling, true, out var parsed) ? parsed : ScalingType.Linear;
                    SetAxis(
                        Axis.X,
                        xAxisName,
                        (float)(setting.ThrMinSel ?? setting.ThrMin),
                        (float)(setting.ThrMaxSel ?? setting.ThrMax),
                        scalingType);
                }
                else if (setting.Name == yAxisName)
                {
                    ScalingType scalingType = Enum.TryParse<ScalingType>(setting.Scaling, true, out var parsed) ? parsed : ScalingType.Linear;
                    SetAxis(
                        Axis.Y,
                        yAxisName,
                        (float)(setting.ThrMinSel ?? setting.ThrMin),
                        (float)(setting.ThrMaxSel ?? setting.ThrMax),
                        scalingType);
                }
                else if (setting.Name == zAxisName)
                {
                    ScalingType scalingType = Enum.TryParse<ScalingType>(setting.Scaling, true, out var parsed) ? parsed : ScalingType.Linear;
                    SetAxis(
                        Axis.Z,
                        zAxisName,
                        (float)(setting.ThrMinSel ?? setting.ThrMin),
                        (float)(setting.ThrMaxSel ?? setting.ThrMax),
                        scalingType);
                }
                else
                {

                }

                switch (setting.Mapping)
                {
                    case null:
                        break;
                    case "Opacity":
                        SetOpacity(setting);
                        break;
                    case "Colormap":
                        SetColormap(setting);
                        break;
                    default:
                        break;
                }
            }
        }

        private void SetOpacity(Setting setting)
        {
            if (setting == null)
            {
                Debug.LogError("[SetOpacity] 'setting' is null.");
                return;
            }

            if (!string.Equals(setting.Mapping, "Opacity", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[SetOpacity] Invalid mapping (expected 'Opacity').");
                return;
            }

            // Parse scaling string into enum with fallback
            if (!Enum.TryParse(setting.Scaling, ignoreCase: true, out ScalingType scaling))
            {
                Debug.LogWarning($"[SetOpacity] Invalid scaling value '{setting.Scaling}'. Using default: {ScalingType.Linear}");
                scaling = ScalingType.Linear;
            }

            float min = (float)(setting.ThrMinSel ?? setting.ThrMin);
            float max = (float)(setting.ThrMaxSel ?? setting.ThrMax);

            RenderManager.Instance.DataRenderer.SetOpacity(
                setting.Name,
                min,
                max,
                scaling,
                setting.InvertMapping
            );
        }

        private void SetColormap(Setting setting)
        {
            if (setting == null)
            {
                Debug.LogError("[SetColormap] Setting is null.");
                return;
            }

            if (!string.Equals(setting.Mapping, "Colormap", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[SetColormap] Invalid mapping (expected 'Opacity').");
                return;
            }

            // Parse colormap string into enum with fallback
            if (!Enum.TryParse(setting.Colormap, ignoreCase: true, out ColorMapEnum colormap))
            {
                Debug.LogWarning($"[SetColormap] Invalid colormap value '{setting.Colormap}'. Using default: {ColorMapEnum.Autumn}");
                colormap = ColorMapEnum.Autumn;
            }

            // Parse scaling string into enum with fallback
            if (!Enum.TryParse(setting.Scaling, ignoreCase: true, out ScalingType scaling))
            {
                Debug.LogWarning($"[SetColormap] Invalid scaling value '{setting.Scaling}'. Using default: {ScalingType.Linear}");
                scaling = ScalingType.Linear;
            }

            float min = (float)(setting.ThrMinSel ?? setting.ThrMin);
            float max = (float)(setting.ThrMaxSel ?? setting.ThrMax);

            RenderManager.Instance.DataRenderer.SetColormap(
                setting.Name,
                colormap,
                min,
                max,
                scaling,
                setting.InvertMapping
            );
        }

        public void RemoveOpacity()
        {
            RenderManager.Instance.DataRenderer.RemoveOpacity();
        }

        public void RemoveColormap()
        {
            RenderManager.Instance.DataRenderer.RemoveColormap();
        }

        public void SetAxis(Axis axis, string paramName, float thresholdMin, float thresholdMax, ScalingType scalingType)
        {
            // Debug.Log($"{paramName}, {thresholdMin}, {thresholdMax}, {scalingType}");
            RenderManager.Instance.DataRenderer?.SetAxisAstrovisio(axis, paramName, thresholdMin, thresholdMax, scalingType);
        }

        // === API ===
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

            Debug.Log(JsonConvert.SerializeObject(req));

            TaskCompletionSource<Settings> tcs = new TaskCompletionSource<Settings>();

            _ = apiManager.UpdateSettings(
                projectId,
                fileId,
                req,
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
