using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using MessagePack;
using System.Linq;

namespace Astrovisio
{
    /// <summary>
    /// Manages low-level API calls for project CRUD operations.
    /// Provides coroutine methods with success and error callbacks.
    /// </summary>
    public class APIManagerCoroutine : MonoBehaviour
    {
        public static APIManagerCoroutine Instance;

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

        /// <summary>
        /// Reads a single project by ID.
        /// </summary>
        public IEnumerator ReadProject(
            int id,
            Action<Project> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetProjectById(id);
            // Debug.Log($"[APIManager] GET {url}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

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

        /// <summary>
        /// Reads all projects.
        /// </summary>
        public IEnumerator ReadProjects(
            Action<List<Project>> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetAllProjects();
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

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


        /// <summary>
        /// Creates a new project via POST.
        /// </summary>
        public IEnumerator CreateNewProject(
            CreateProjectRequest req,
            Action<Project> onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.CreateProject();
            string json = JsonConvert.SerializeObject(req);
            // Debug.Log($"[APIManager] POST {url} - Payload: {json}");

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success && request.responseCode != 201)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    try
                    {
                        Project createdProject = JsonConvert.DeserializeObject<Project>(request.downloadHandler.text);
                        // Debug.Log("createdProject " + createdProject.ConfigProcess.Params.Count);
                        onSuccess?.Invoke(createdProject);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Updates an existing project via PUT.
        /// </summary>
        public IEnumerator UpdateProject(
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
                yield return request.SendWebRequest();

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

        /// <summary>
        /// Deletes a project by ID.
        /// </summary>
        public IEnumerator DeleteProject(
            int id,
            Action onSuccess,
            Action<string> onError = null)
        {
            string url = APIEndpoints.GetProjectById(id);
            // Debug.Log($"[APIManager] DELETE {url}");

            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                yield return request.SendWebRequest();

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


        /// <summary>
        /// Process a project by ID, sending configuration as payload.
        /// </summary>
        public IEnumerator ProcessProject(
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

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[APIManager] Error POST: {request.error}");
                    onError?.Invoke(request.downloadHandler.text);
                    yield break;
                }

                try
                {
                    byte[] rawBytes = request.downloadHandler.data;
                    DataPack processedData = MessagePackSerializer.Deserialize<DataPack>(rawBytes);
                    Debug.Log($"[APIManager] Received {processedData.Rows.Length} rows, {processedData.Columns.Length} columns.");

                    // double[] firstRow = processedData.Rows[1];
                    // Debug.Log("Test " + firstRow[2]);

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
