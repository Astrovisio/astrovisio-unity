using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        private void Start()
        {
            FetchProjects();
        }

        public void FetchProjects()
        {
            StartCoroutine(ReadProjects());
            // StartCoroutine(CreateNewProject(req));
        }

        public IEnumerator ReadProject(int id)
        {
            string url = APIEndpoints.GetProjectById(id);
            Debug.Log($"[ReadProject] Fetching project with ID {id} from: {url}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[ReadProject] Error ({request.responseCode}): {request.error}");
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"[ReadProject] Response: {jsonResponse}");
                }
            }
        }

        public IEnumerator ReadProjects()
        {
            string url = APIEndpoints.GetAllProjects();
            Debug.Log($"[ReadProjects] Fetching all projects from: {url}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[ReadProjects] Error: {request.error}");
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"[ReadProjects] Response: {jsonResponse}");

                    // Se il server restituisce una lista
                    // List<Project> projects = JsonConvert.DeserializeObject<List<Project>>(jsonResponse);
                    // onSuccess?.Invoke(projects);
                }
            }
        }

        public IEnumerator CreateNewProject(CreateProjectRequest req)
        {
            string json = JsonConvert.SerializeObject(req);

            using (UnityWebRequest request = new UnityWebRequest(APIEndpoints.CreateProject(), "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success || request.responseCode == 201)
                {
                    Debug.Log("Project created: " + request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"POST error ({request.responseCode}): {request.error}");
                }
            }
        }

        public IEnumerator UpdateProject(int id)
        {
            string url = APIEndpoints.GetProjectById(id);

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Error");
                }
                else
                {
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log(jsonResponse);
                    // ProjectsWrapper wrapper = JsonUtility.FromJson<ProjectsWrapper>(jsonResponse);
                    // onSuccess?.Invoke(wrapper.projects);
                }
            }
        }

    }

}
