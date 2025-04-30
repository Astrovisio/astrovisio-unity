using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{

	public class ProjectManager : MonoBehaviour
	{
		[Header("Dependencies")]
		[SerializeField] private APIManager apiManager;

		private List<Project> projectList = new List<Project>();
		private List<Project> openedProjectList = new List<Project>();
		private Project currentProject;

		// --- Events ---
		public event Action<List<Project>> ProjectsFetched;
		public event Action<Project> ProjectOpened;
		public event Action<Project> ProjectCreated;
		public event Action<Project> ProjectUpdated;
		public event Action<Project> ProjectClosed;
		public event Action ProjectUnselected;
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

		public bool IsProjectOpened(int id)
		{
			return openedProjectList.Any(p => p.Id == id);
		}

		public Project GetCurrentProject()
		{
			return currentProject;
		}

		public Project GetOpenedProject(int id)
		{
			return openedProjectList.Find(p => p.Id == id);
		}

		public void CloseProject(int id)
		{
			var projectToRemove = openedProjectList.Find(p => p.Id == id);
			if (projectToRemove != null)
			{
				openedProjectList.Remove(projectToRemove);
				Debug.Log($"[ProjectManager] Progetto con ID {id} chiuso e rimosso dalla lista aperti.");

				if (currentProject != null && currentProject.Id == id)
				{
					currentProject = null;
					Debug.Log("[ProjectManager] Progetto corrente azzerato.");
				}

				ProjectClosed?.Invoke(projectToRemove);
			}
			else
			{
				Debug.LogWarning($"[ProjectManager] Nessun progetto aperto trovato con ID {id} da chiudere.");
			}
		}

		public void UnselectProject()
		{
			if (currentProject != null)
			{
				Debug.Log($"[ProjectManager] Progetto con ID {currentProject.Id} deselezionato.");
				currentProject = null;
				ProjectUnselected?.Invoke();
			}
			else
			{
				Debug.LogWarning("[ProjectManager] Nessun progetto selezionato da deselezionare.");
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

		public Project GetFakeProject()
		{
			var variables = new Dictionary<string, ConfigParam>
			{
				{
					"Temperature",
					new ConfigParam
					{
						Unit = "°C",
						Selected = true,
						ThrMin = 0,
						ThrMinSel = 10,
						ThrMax = 100,
						ThrMaxSel = 90,
						XAxis = true,
						YAxis = false,
						ZAxis = false,
						Files = new[] { "data1.csv" }
					}
				},
				{
					"Pressure",
					new ConfigParam
					{
						Unit = "Pa",
						Selected = false,
						ThrMin = 100,
						ThrMinSel = 200,
						ThrMax = 1000,
						ThrMaxSel = 800,
						XAxis = false,
						YAxis = true,
						ZAxis = false,
						Files = new[] { "data2.csv" }
					}
				},
				{
					"Velocity",
					new ConfigParam
					{
						Unit = "m/s",
						Selected = true,
						ThrMin = 0,
						ThrMinSel = 5,
						ThrMax = 50,
						ThrMaxSel = 40,
						XAxis = false,
						YAxis = false,
						ZAxis = true,
						Files = new[] { "data3.csv" }
					}
				}
			};

			var configProcess = new ConfigProcess
			{
				Downsampling = 1.0f,
				Params = variables
			};

			return new Project("Fake Project", "A test project", false, new string[] { "/fake/path" })
			{
				Id = -1,
				Created = System.DateTime.Now,
				LastOpened = System.DateTime.Now,
				ConfigProcess = configProcess
			};
		}


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


		public void OpenProject(int id)
		{
			// Debug.Log("Opening project: " + id);

			Project alreadyOpened = openedProjectList.Find(p => p.Id == id);
			if (alreadyOpened != null)
			{
				Debug.Log("[ProjectManager] Project already opened, skipping API call.");
				currentProject = alreadyOpened;
				ProjectOpened?.Invoke(alreadyOpened);
				return;
			}

			StartCoroutine(OpenProjectCoroutine(id));
		}

		private IEnumerator OpenProjectCoroutine(int id)
		{
			yield return apiManager.ReadProject(
				id,
				onSuccess: project =>
				{
					currentProject = project;
					// Debug.Log("Current project updated to: " + currentProject.Id);

					if (!openedProjectList.Any(p => p.Id == project.Id))
					{
						openedProjectList.Add(project);
					}

					ProjectOpened?.Invoke(project);
				},
				onError: err => ApiError?.Invoke(err)
			);
		}


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


		private void OnDisable()
		{
			ProjectsFetched = null;
			ProjectOpened = null;
			ProjectCreated = null;
			ProjectUpdated = null;
			ProjectDeleted = null;
			ApiError = null;
		}


	}

}
