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
            int projectID,
            Action<Project> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetProject(projectID);
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
            string url = APIEndpoints.GetProjects();
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
            int projectID,
            UpdateProjectRequest req,
            Action<Project> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetProject(projectID);
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
            int projectID,
            Action onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.DeleteProject(projectID);

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

        public async Task DuplicateProject(
            int projectID,
            DuplicateProjectRequest req,
            Action<Project> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.DuplicateProject(projectID);

            string jsonPayload = JsonConvert.SerializeObject(req);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
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
                        Project duplicated = JsonConvert.DeserializeObject<Project>(request.downloadHandler.text);
                        onSuccess?.Invoke(duplicated);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke("Deserialization failed: " + ex.Message);
                    }
                }
            }
        }

        public async Task<int?> ProcessFile(
            int projectID,
            int fileID,
            Action<string> onError = null)
        {
            string url = APIEndpoints.ProcessFile(projectID, fileID);

            Debug.Log($"[APIManager] POST {url}");

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>());
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await SendWebRequestAsync(request);
                string raw = request.downloadHandler.text;
                // Debug.Log($"[APIManager] Raw response: {raw}");

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[APIManager] Error POST: {request.error}");
                    onError?.Invoke(string.IsNullOrEmpty(raw) ? request.error : raw);
                    return null;
                }

                try
                {
                    JobResponse jobResponse = JsonConvert.DeserializeObject<JobResponse>(raw);
                    // Debug.Log($"[APIManager] Received job_id: {jobResponse?.JobID}");
                    return jobResponse?.JobID;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[APIManager] Deserialization failed: {ex.Message}");
                    onError?.Invoke("Deserialization failed: " + ex.Message);
                    return null;
                }
            }
        }

        public async Task GetProcessedFile(
            int projectID,
            int fileID,
            Action<DataPack> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetProcessedFile(projectID, fileID);
            Debug.Log($"[APIManager] GET {url}");

            const int maxRetries = 3;
            const int retryDelayMs = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {

                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.timeout = 10;
                    request.SetRequestHeader("Accept", "application/x-msgpack");

                    await SendWebRequestAsync(request);

                    Debug.Log($"Attempt {attempt}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            byte[] rawBytes = request.downloadHandler.data;
                            DataPack dataPack = MessagePackSerializer.Deserialize<DataPack>(rawBytes);
                            onSuccess?.Invoke(dataPack);
                            return;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[APIManager] Deserialization failed (MessagePack): {ex.Message}");
                            onError?.Invoke("Deserialization failed: " + ex.Message);
                            return;
                        }
                    }
                    else
                    {
                        Debug.LogError($"[APIManager] Error GET ProcessedFile (attempt {attempt}): {request.error}");
                        Debug.LogError($"[APIManager] Error Text: {request.downloadHandler.text}");

                        if (attempt == maxRetries)
                        {
                            onError?.Invoke(string.IsNullOrEmpty(request.downloadHandler.text)
                                ? request.error
                                : request.downloadHandler.text);
                            return;
                        }

                        await Task.Delay(retryDelayMs);
                    }
                }
            }

            onError?.Invoke("Unknown error");
        }

        public async Task UpdateFile(
            int projectID,
            int fileID,
            UpdateFileRequest req,
            Action<File> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.UpdateFile(projectID, fileID);
            string json = JsonConvert.SerializeObject(req);

            // Debug.Log($"[APIManager] PUT {url} - Payload: {json}");
            // string savedPath = DebugUtility.SaveJson("PUT", json, pretty: true);


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
                        // string savedPath = DebugUtility.SaveJson("PUT_UpdateFILE", request.downloadHandler.text, pretty: true);
                        File updatedFile = JsonConvert.DeserializeObject<File>(request.downloadHandler.text);
                        onSuccess?.Invoke(updatedFile);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex.Message);
                    }
                }
            }
        }

        public async Task ReplaceProjectFiles(
            int projectID,
            ReplaceProjectFilesRequest req)
        {
            if (req == null || req.Paths == null || req.Paths.Length == 0)
                Debug.LogWarning("[APIManager] ReplaceProjectFiles called with empty request/paths.");

            string url = APIEndpoints.UpdateProjectFiles(projectID);
            string json = JsonConvert.SerializeObject(req);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            Debug.Log($"[APIManager] PUT {url} - Payload: {json}");

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await SendWebRequestAsync(request);

                long status = request.responseCode;
                string body = request.downloadHandler?.text;
                bool success = request.result == UnityWebRequest.Result.Success;

                if (!success)
                {
                    Debug.LogError($"[APIManager] Error PUT ReplaceProjectFiles: {request.error} | Status: {status}\nBody: {body}");
                    throw new Exception($"PUT failed ({status}): {request.error}");
                }

                Debug.Log($"[APIManager] ReplaceProjectFiles OK. Status: {status}\nBody: {body}");
            }
        }

        public async Task<JobStatusResponse> GetJobProgress(
            int jobID,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetJobProgress(jobID);
            // Debug.Log($"[APIManager] GET {url}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Content-Type", "application/json");

                await SendWebRequestAsync(request);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[APIManager] Error GET JobStatus: {request.error}"); // Err
                    onError?.Invoke(request.downloadHandler.text);
                    return null;
                }

                // Debug.Log($"[APIManager] Job Status Raw response: {request.downloadHandler.text}");

                try
                {
                    string json = request.downloadHandler.text;
                    // Debug.Log(json);
                    JobStatusResponse jobStatusResponse = JsonConvert.DeserializeObject<JobStatusResponse>(json);
                    return jobStatusResponse;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[APIManager] Failed to parse job result: {ex.Message}");
                    onError?.Invoke("Deserialization or parsing failed: " + ex.Message);
                    return null;
                }
            }
        }

        public async Task<DataPack> GetJobResult(
            int jobID,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetJobResult(jobID);
            Debug.Log($"[APIManager] GET {url}");

            const int maxRetries = 3;
            const int retryDelayMs = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.timeout = 10;

                    await SendWebRequestAsync(request);

                    Debug.Log($"Attempt {attempt}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            byte[] rawBytes = request.downloadHandler.data;
                            DataPack processedData = MessagePackSerializer.Deserialize<DataPack>(rawBytes);
                            Debug.Log($"[APIManager] Received {processedData.Rows.Length} rows, {processedData.Columns.Length} columns.");
                            return processedData;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[APIManager] Deserialization failed: {ex.Message}");
                            onError?.Invoke("Deserialization failed: " + ex.Message);
                            return null;
                        }
                    }
                    else
                    {
                        Debug.LogError($"[APIManager] Error GET (attempt {attempt}): {request.error}");
                        Debug.LogError($"[APIManager] Error Text: {request.downloadHandler.text}");

                        if (attempt == maxRetries)
                        {
                            onError?.Invoke(request.downloadHandler.text);
                            return null;
                        }

                        await Task.Delay(retryDelayMs);
                    }
                }
            }

            onError?.Invoke("Unknown error");
            return null;
        }

    }

}
