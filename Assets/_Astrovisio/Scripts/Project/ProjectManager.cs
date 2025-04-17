using System.Collections;
using UnityEngine;

namespace Astrovisio
{
    public class ProjectManager : MonoBehaviour
    {
        [SerializeField] private APIManager apiManager;

        public void CreateProject(string name, string description, string[] paths)
        {
            var request = new CreateProjectRequest
            {
                Name = name,
                Description = description,
                Favourite = false,
                Paths = paths
            };

            StartCoroutine(apiManager.CreateNewProject(request));
        }

        public void FetchAllProjects()
        {
            StartCoroutine(apiManager.ReadProjects());
        }

        public void FetchProjectById(int id)
        {
            StartCoroutine(apiManager.ReadProject(id));
        }

        public void UpdateProject(int id, UpdateProjectRequest updatedData)
        {
            // StartCoroutine(apiManager.UpdateProject(id, updatedData));
        }

        public void DeleteProject(int id)
        {
            // StartCoroutine(apiManager.DeleteProject(id));
        }
    }
}
