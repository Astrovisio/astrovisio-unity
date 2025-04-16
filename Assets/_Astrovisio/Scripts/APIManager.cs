using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Astrovisio
{

    public class APIManager : MonoBehaviour
    {
        public static APIManager Instance;
        private readonly string baseUrl = "http://localhost:8080";

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
            StartCoroutine(GetProjects());
        }

        private IEnumerator GetProjects()
        {
            string url = APIEndpoints.GetProjects();

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

        public IEnumerator PostProject(Project project)
        {
            string json = JsonUtility.ToJson(project);
            using var request = new UnityWebRequest(baseUrl + "/api/projects", "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            // request.SetRequestHeader("Content-Type", "application/json");

            // yield return request.SendWebRequest();
            yield return new WaitForSeconds(1f);

            // if (request.result == UnityWebRequest.Result.Success)
            //     onComplete?.Invoke(true);
            // else
            // {
            //     Debug.LogError("POST error: " + request.error);
            //     onComplete?.Invoke(false);
            // }
        }

    }

}
