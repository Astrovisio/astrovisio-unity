using System;
using System.Collections;
using System.Collections.Generic;
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

        private List<Project> projects = new List<Project>();

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
                    projects = fetched ?? new List<Project>();
                    ProjectsFetched?.Invoke(projects);
                    Debug.Log($"[ProjectManager] Fetched {projects.Count} projects");
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
