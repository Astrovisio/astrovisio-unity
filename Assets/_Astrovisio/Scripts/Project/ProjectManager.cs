using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    /// <summary>
    /// Central manager for project CRUD operations.
    /// Exposes events for created, fetched, updated, and deleted projects.
    /// </summary>
    public class ProjectManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private APIManager apiManager;

        private List<Project> projectList = new List<Project>();

        // --- Events ---
        public event Action<List<Project>> ProjectsFetched;
        public event Action<Project> ProjectFetched;
        public event Action<Project> ProjectCreated;
        public event Action<Project> ProjectUpdated;
        public event Action<int> ProjectDeleted;
        public event Action<string> ApiError;

        private void Awake()
        {
            if (apiManager == null)
            {
                Debug.LogError("[ProjectManager] APIManager reference is missing.");
                enabled = false;
            }
        }

        public List<Project> GetProjectList()
        {
            return projectList;
        }

        public List<Project> GetFakeProjectList()
        {
            
            string jsonResponse = @"
                [
                  {
                    ""name"": ""string"",
                    ""favourite"": false,
                    ""description"": ""string"",
                    ""id"": 19,
                    ""created"": ""2025-04-18T10:32:02.518009"",
                    ""last_opened"": ""2025-04-18T13:18:59.765766"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""1"",
                    ""favourite"": false,
                    ""description"": ""2"",
                    ""id"": 20,
                    ""created"": ""2025-04-18T12:21:49.983021"",
                    ""last_opened"": ""2025-04-18T13:57:47.675657"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""a"",
                    ""favourite"": false,
                    ""description"": ""v"",
                    ""id"": 21,
                    ""created"": ""2025-04-18T12:39:33.380259"",
                    ""last_opened"": ""2025-04-18T13:11:45.849116"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Last month 3"",
                    ""favourite"": false,
                    ""description"": ""Last month 3 description"",
                    ""id"": 22,
                    ""created"": ""2025-04-01T13:45:43.461823"",
                    ""last_opened"": ""2025-04-01T13:45:43.461823"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Last month 2"",
                    ""favourite"": false,
                    ""description"": ""Last month 2 description"",
                    ""id"": 23,
                    ""created"": ""2025-04-01T13:45:53.243955"",
                    ""last_opened"": ""2025-04-01T13:45:53.243955"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Last month 1"",
                    ""favourite"": false,
                    ""description"": ""Last month 1 description"",
                    ""id"": 24,
                    ""created"": ""2025-04-01T13:46:05.204847"",
                    ""last_opened"": ""2025-04-01T13:46:05.204847"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Older 1"",
                    ""favourite"": false,
                    ""description"": ""Older 1 description"",
                    ""id"": 25,
                    ""created"": ""2024-04-18T13:46:33.107021"",
                    ""last_opened"": ""2024-04-18T13:46:33.107021"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Older 2"",
                    ""favourite"": false,
                    ""description"": ""Older 2 description"",
                    ""id"": 26,
                    ""created"": ""2024-04-18T13:46:43.284789"",
                    ""last_opened"": ""2024-04-18T13:46:43.284789"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Older 3"",
                    ""favourite"": false,
                    ""description"": ""Older 3 description"",
                    ""id"": 27,
                    ""created"": ""2024-04-18T13:46:52.282072"",
                    ""last_opened"": ""2024-04-18T13:46:52.282072"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Older 4"",
                    ""favourite"": false,
                    ""description"": ""Older 4 description"",
                    ""id"": 27,
                    ""created"": ""2024-04-18T13:46:52.282072"",
                    ""last_opened"": ""2024-04-18T13:46:52.282072"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Older 5"",
                    ""favourite"": false,
                    ""description"": ""Older 5 description"",
                    ""id"": 27,
                    ""created"": ""2024-04-18T13:46:52.282072"",
                    ""last_opened"": ""2024-04-18T13:46:52.282072"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Older 6"",
                    ""favourite"": false,
                    ""description"": ""Older 6 description"",
                    ""id"": 27,
                    ""created"": ""2024-04-18T13:46:52.282072"",
                    ""last_opened"": ""2024-04-18T13:46:52.282072"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  },
                  {
                    ""name"": ""Older 7"",
                    ""favourite"": false,
                    ""description"": ""Older 7 description"",
                    ""id"": 27,
                    ""created"": ""2024-04-18T13:46:52.282072"",
                    ""last_opened"": ""2024-04-18T13:46:52.282072"",
                    ""paths"": [],
                    ""config_process"": {
                      ""downsampling"": 1,
                      ""variables"": {}
                    }
                  }
                ]";

            try
            {
                return JsonConvert.DeserializeObject<List<Project>>(jsonResponse);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GetFakeProjects] Deserialization error: {ex.Message}");
                return new List<Project>();
            }
        }


        /// <summary>
        /// Fetches all projects from the server.
        /// </summary>
        public void FetchAllProjects()
        {
            StartCoroutine(FetchAllProjectsCoroutine());
        }

        private IEnumerator FetchAllProjectsCoroutine()
        {
            yield return apiManager.ReadProjects(
                onSuccess: fetched =>
                {
                    // Check for null in case of bad responses
                    projectList = fetched ?? new List<Project>();
                    ProjectsFetched?.Invoke(projectList);
                    // Debug.Log($"[ProjectManager] Fetched {projectList.Count} projects");
                },
                onError: err => ApiError?.Invoke(err)
            );
        }


        /// <summary>
        /// Fetches a single project by its ID.
        /// </summary>
        public void FetchProjectById(int id)
        {
            StartCoroutine(FetchProjectByIdCoroutine(id));
        }

        private IEnumerator FetchProjectByIdCoroutine(int id)
        {
            yield return apiManager.ReadProject(
                id,
                onSuccess: project => ProjectFetched?.Invoke(project),
                onError: err => ApiError?.Invoke(err)
            );
        }


        /// <summary>
        /// Creates a new project with the given parameters.
        /// </summary>
        public void CreateProject(string name, string description, string[] paths)
        {
            var request = new CreateProjectRequest
            {
                Name = name,
                Description = description,
                Favourite = false,
                Paths = paths
            };
            StartCoroutine(CreateProjectCoroutine(request));
        }

        private IEnumerator CreateProjectCoroutine(CreateProjectRequest request)
        {
            yield return apiManager.CreateNewProject(
                request,
                onSuccess: created => ProjectCreated?.Invoke(created),
                onError: err => ApiError?.Invoke(err)
            );
        }


        /// <summary>
        /// Updates an existing project.
        /// </summary>
        public void UpdateProject(int id, UpdateProjectRequest updatedData)
        {
            StartCoroutine(UpdateProjectCoroutine(id, updatedData));
        }

        private IEnumerator UpdateProjectCoroutine(int id, UpdateProjectRequest updatedData)
        {
            yield return apiManager.UpdateProject(
                id,
                updatedData,
                onSuccess: updated => ProjectUpdated?.Invoke(updated),
                onError: err => ApiError?.Invoke(err)
            );
        }


        /// <summary>
        /// Deletes a project by its ID.
        /// </summary>
        public void DeleteProject(int id)
        {
            StartCoroutine(DeleteProjectCoroutine(id));
        }

        private IEnumerator DeleteProjectCoroutine(int id)
        {
            yield return apiManager.DeleteProject(
                id,
                onSuccess: () => ProjectDeleted?.Invoke(id),
                onError: err => ApiError?.Invoke(err)
            );
        }


        // Clean up subscriptions when the object is disabled
        private void OnDisable()
        {
            ProjectsFetched = null;
            ProjectFetched = null;
            ProjectCreated = null;
            ProjectUpdated = null;
            ProjectDeleted = null;
            ApiError = null;
        }

    }
}
