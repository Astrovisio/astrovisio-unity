using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Astrovisio
{
    public class ProjectManager : MonoBehaviour
    {

        // [SerializeField] private APIManager api


        // Singleton
        private static ProjectManager _instance;
        private static readonly object _lock = new object();

        public static ProjectManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = FindFirstObjectByType<ProjectManager>();
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject("ProjectManager");
                            _instance = singletonObject.AddComponent<ProjectManager>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                }
                return _instance;
            }
        }


        public event Action<Project[]> OnProjectsLoaded;
        private Project[] projects;

        private void Start()
        {
            StartCoroutine(GetProjectsFromAPI());
        }

        public IEnumerator GetProjectsFromAPI()
        {
            string url = "http://localhost:8000/api/projects/";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Errore nella richiesta API: {request.error}");
                }
                else
                {
                    string json = request.downloadHandler.text;

                    try
                    {
                        projects = JsonConvert.DeserializeObject<Project[]>(json);

                        foreach (var project in projects)
                        {
                            Debug.Log($"[API] ID: {project.Id}, Nome: {project.Name}, Descrizione: {project.Description}");
                            if (project.ConfigProcess != null)
                            {
                                // Debug.Log($"Downsampling: {project.ConfigProcess.Downsampling}");
                                // Puoi iterare le variabili se necessario
                                foreach (var variable in project.ConfigProcess.Variables)
                                {
                                    Debug.Log($"Variabile {variable.Key}: Unit {variable.Value.Unit}, Selected {variable.Value.Selected}");
                                }
                            }
                        }

                        OnProjectsLoaded?.Invoke(projects);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Errore nella deserializzazione JSON: {e.Message}");
                    }
                }
            }
        }

        public IEnumerator GetProjectsFromFakeAPI()
        {
            yield return new WaitForSeconds(1f);

            string fakeJson = @"
            [
                {
                    ""name"": ""Progetto Alfa"",
                    ""favourite"": true,
                    ""description"": ""Descrizione del progetto Alfa"",
                    ""id"": 1,
                    ""created"": ""2025-04-16T08:37:43.746Z"",
                    ""last_opened"": ""2025-04-16T08:40:00.123Z"",
                    ""paths"": [""/path/to/data1"", ""/path/to/data2""]
                },
                {
                    ""name"": ""Progetto Beta"",
                    ""favourite"": false,
                    ""description"": ""Descrizione del progetto Beta"",
                    ""id"": 2,
                    ""created"": ""2025-04-15T12:20:10.000Z"",
                    ""last_opened"": ""2025-04-16T08:50:10.000Z"",
                    ""paths"": [""/path/to/beta1""]
                }
            ]";

            projects = JsonConvert.DeserializeObject<Project[]>(fakeJson);

            foreach (var project in projects)
            {
                Debug.Log($"[Newtonsoft] ID: {project.Id} - Nome: {project.Name}");
            }

            OnProjectsLoaded?.Invoke(projects);
        }

    }
}
