using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using MessagePack;

namespace Astrovisio
{

    /// <summary>
    /// Manages low-level API calls for project CRUD operations.
    /// Provides async methods with success and error callbacks.
    /// </summary>
    public class APIManager : MonoBehaviour
    {
        public static APIManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async Task<UnityWebRequest> SendWebRequestAsync(UnityWebRequest request)
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            return request;
        }

        public async Task ReadProject(
            int id,
            Action<Project> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetProjectById(id);
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                await SendWebRequestAsync(request);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    try
                    {
                        Project project = JsonConvert.DeserializeObject<Project>(request.downloadHandler.text);
                        onSuccess?.Invoke(project);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex.Message);
                    }
                }
            }
        }

        public async Task ReadProjects(
            Action<List<Project>> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetAllProjects();
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                await SendWebRequestAsync(request);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"[ReadProjects] JSON Response: {jsonResponse}");

                    try
                    {
                        List<Project> projects = JsonConvert.DeserializeObject<List<Project>>(jsonResponse);
                        onSuccess?.Invoke(projects);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ReadProjects] JSON Deserialization failed: {ex.Message}");
                        onError?.Invoke("Deserialization error: " + ex.Message);
                    }
                }
            }
        }

        public async Task CreateNewProject(
            CreateProjectRequest req,
            Action<Project> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.CreateProject();
            string json = JsonConvert.SerializeObject(req);

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await SendWebRequestAsync(request);

                if (request.result != UnityWebRequest.Result.Success && request.responseCode != 201)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    try
                    {
                        Project createdProject = JsonConvert.DeserializeObject<Project>(request.downloadHandler.text);
                        onSuccess?.Invoke(createdProject);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex.Message);
                    }
                }
            }
        }

        public async Task UpdateProject(
            int id,
            UpdateProjectRequest req,
            Action<Project> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetProjectById(id);
            string json = JsonConvert.SerializeObject(req);
            Debug.Log($"[APIManager] PUT {url} - Payload: {json}");

            using (UnityWebRequest request = UnityWebRequest.Put(url, json))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                await SendWebRequestAsync(request);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    try
                    {
                        Project updated = JsonConvert.DeserializeObject<Project>(request.downloadHandler.text);
                        onSuccess?.Invoke(updated);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex.Message);
                    }
                }
            }
        }

        public async Task DeleteProject(
            int id,
            Action onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetProjectById(id);

            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                await SendWebRequestAsync(request);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    onSuccess?.Invoke();
                }
            }
        }

        public async Task ProcessProject(
            int id,
            ProcessProjectRequest req,
            Action<DataPack> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.ProcessProject(id);
            Debug.Log($"[APIManager] POST {url}");

            string jsonPayload = JsonConvert.SerializeObject(req, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            Debug.Log($"[APIManager] jsonPayload {jsonPayload}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await SendWebRequestAsync(request);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[APIManager] Error POST: {request.error}");
                    onError?.Invoke(request.downloadHandler.text);
                    return;
                }

                try
                {
                    byte[] rawBytes = request.downloadHandler.data;
                    DataPack processedData = MessagePackSerializer.Deserialize<DataPack>(rawBytes);
                    Debug.Log($"[APIManager] Received {processedData.Rows.Length} rows, {processedData.Columns.Length} columns.");

                    onSuccess?.Invoke(processedData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[APIManager] Deserialization failed: {ex.Message}");
                    onError?.Invoke("Deserialization failed: " + ex.Message);
                }
            }
        }
        
    }

}
