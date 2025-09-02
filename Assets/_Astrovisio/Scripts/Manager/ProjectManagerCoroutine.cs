using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{

	public class ProjectManagerCoroutine : MonoBehaviour
	{
		[Header("Dependencies")]
		[SerializeField] private APIManager apiManager;
		[SerializeField] private UIManager uiManager;

		// === Events ===
		public event Action<List<Project>> ProjectsFetched;
		public event Action<Project> ProjectOpened;
		public event Action<Project> ProjectCreated;
		public event Action<Project> ProjectUpdated;
		public event Action<Project> ProjectClosed;
		public event Action<Project, DataPack> ProjectProcessed;
		public event Action ProjectUnselected;
		public event Action<Project> ProjectDeleted;
		public event Action<string> ApiError;

		// === Local ===
		private List<Project> projectList = new List<Project>();
		private List<Project> openedProjectList = new List<Project>();
		private Project currentProject;


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

		public Project GetProject(int id)
		{
			return projectList.FirstOrDefault(p => p.Id == id);
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
				// Debug.Log($"[ProjectManager] Progetto con ID {currentProject.Id} deselezionato.");
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


		private Project fakeProject;
		private Project InitFakeProject()
		{
			var configParams = new Dictionary<string, ConfigParam>
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
						YAxis = false,
						ZAxis = true,
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
						YAxis = true,
						ZAxis = false,
						Files = new[] { "data3.csv" }
					}
				}
			};

			var configProcess = new ConfigProcess
			{
				Downsampling = 1.0f,
				Params = configParams
			};

			fakeProject = new Project("Fake Project", "A test project", false, new string[] { "/fake/path" })
			{
				Id = -1,
				Created = DateTime.Now,
				LastOpened = DateTime.Now,
				ConfigProcess = configProcess
			};

			return fakeProject;
		}
		public Project GetFakeProject()
		{
			return fakeProject is null ? InitFakeProject() : fakeProject;
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


		public void FetchAllProjects()
		{
			StartCoroutine(FetchAllProjectsCoroutine());
		}

		private IEnumerator FetchAllProjectsCoroutine()
		{
			// Debug.Log("FetchAllProjectsCoroutine -> START");
			uiManager.SetLoadingView(true);
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
			uiManager.SetLoadingView(false);
			// Debug.Log("FetchAllProjectsCoroutine -> END");
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
					// Cerca se esiste già un progetto con lo stesso ID
					Project existingProject = projectList.FirstOrDefault(p => p.Id == project.Id);

					if (existingProject != null)
					{
						// Aggiorna i valori mantenendo lo stesso riferimento
						existingProject.UpdateFrom(project);
					}
					else
					{
						// Aggiunge il nuovo progetto alla lista
						existingProject = project;
						projectList.Add(existingProject);
					}

					// Aggiunge alla lista degli aperti se non già presente (evita duplicati)
					if (!openedProjectList.Any(p => p.Id == existingProject.Id))
					{
						openedProjectList.Add(existingProject);
					}

					// Aggiorna il progetto corrente
					currentProject = existingProject;

					// Notifica apertura progetto
					ProjectOpened?.Invoke(existingProject);
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

		private IEnumerator CreateProjectCoroutine(CreateProjectRequest createProjectRequest)
		{
			uiManager.SetLoadingView(true);
			bool finished = false;

			yield return apiManager.CreateNewProject(
				createProjectRequest,
				onSuccess: created =>
				{
					projectList.Add(created);
					ProjectCreated?.Invoke(created);
					finished = true;
				},
				onError: err =>
				{
					Debug.Log("Error " + err);
					ApiError?.Invoke(err);
					finished = true;
				}
			);

			// Nel caso la CreateNewProject non sia una coroutine e non garantisca il finish, potresti aggiungere un timeout o controllo
			while (!finished)
				yield return null;

			uiManager.SetLoadingView(false);
		}


		public void DuplicateProject(string name, string description, Project projectToDuplicate)
		{
			DuplicateProjectRequest duplicateProjectRequest = new DuplicateProjectRequest
			{
				Name = name,
				Description = description,
				Paths = projectToDuplicate.Paths,
				ConfigProcess = projectToDuplicate.ConfigProcess
			};

			// Debug.Log($"Duplicating project {projectToDuplicate.Id} - {projectToDuplicate.Name}");
			StartCoroutine(DuplicateProjectCoroutine(duplicateProjectRequest));
		}

		private IEnumerator DuplicateProjectCoroutine(DuplicateProjectRequest duplicateProjectRequest)
		{
			uiManager.SetLoadingView(true);
			// Debug.Log($"[Duplicate] Step 1: Creating '{duplicateProjectRequest.Name}'");

			// Step 1: Crea un nuovo progetto base
			var createProjectRequest = new CreateProjectRequest
			{
				Name = duplicateProjectRequest.Name,
				Description = duplicateProjectRequest.Description,
				Favourite = false,
				Paths = duplicateProjectRequest.Paths
			};

			Project createdProject = null;

			yield return apiManager.CreateNewProject(
				createProjectRequest,
				onSuccess: created =>
				{
					createdProject = created;
					projectList.Add(createdProject);
				},
				onError: err =>
				{
					ApiError?.Invoke(err);
				}
			);

			// Se la creazione è fallita, termina
			if (createdProject == null)
			{
				uiManager.SetLoadingView(false);
				yield break;
			}

			// Debug.Log($"[Duplicate] Step 2: Created base project with ID {createdProject.Id}");

			// Step 2: Applica il ConfigProcess e altri dettagli duplicati
			UpdateProjectRequest updateProjectRequest = new UpdateProjectRequest
			{
				Name = createdProject.Name,
				Description = createdProject.Description,
				Favourite = false,
				Paths = createdProject.Paths,
				ConfigProcess = duplicateProjectRequest.ConfigProcess.DeepCopy()
			};

			Debug.Log("SERIAL: " + JsonConvert.SerializeObject(updateProjectRequest.ConfigProcess));

			// UpdateProjectRequest updateProjectRequest = new UpdateProjectRequest
			// {
			// 	Name = createdProject.Name,
			// 	Description = createdProject.Description,
			// 	Favourite = createdProject.Favourite,
			// 	Paths = createdProject.Paths,
			// 	ConfigProcess = createdProject.ConfigProcess
			// };

			// Debug.Log(updateProjectRequest.ConfigProcess);

			yield return apiManager.UpdateProject(
				createdProject.Id,
				updateProjectRequest,
				onSuccess: updated =>
				{
					// Usa UpdateFrom per copiare solo i valori, senza sostituire riferimenti
					createdProject.UpdateFrom(updated);

					// Debug.Log(createdProject.Print());
					// currentProject = createdProject;
					ProjectCreated?.Invoke(createdProject);
					// Debug.Log($"[Duplicate] Step 3: Duplicated project '{createdProject.Name}' (ID: {createdProject.Id})");
				},
				onError: err =>
				{
					// Debug.Log("ERROR: " + err);
					ApiError?.Invoke(err);
				}
			);

			uiManager.SetLoadingView(false);
		}



		public void UpdateProject(int id, Project project)
		{
			var request = new UpdateProjectRequest
			{
				Name = project.Name,
				Favourite = project.Favourite,
				Description = project.Description,
				Paths = project.Paths,
				ConfigProcess = project.ConfigProcess
			};
			StartCoroutine(UpdateProjectCoroutine(id, request));
		}

		private IEnumerator UpdateProjectCoroutine(int id, UpdateProjectRequest updateProjectRequest)
		{
			yield return apiManager.UpdateProject(
				id,
				updateProjectRequest,
				onSuccess: updated =>
				{
					// Cerca se esiste già il progetto nella lista
					Project existingProject = projectList.FirstOrDefault(p => p.Id == updated.Id);

					if (existingProject != null)
					{
						// Aggiorna i valori senza creare un nuovo riferimento
						existingProject.UpdateFrom(updated);
					}
					else
					{
						// Aggiunge il nuovo progetto alla lista
						existingProject = updated;
						projectList.Add(existingProject);
					}

					// Eventuale aggiornamento del currentProject, se ti serve
					// currentProject = existingProject;

					// Notifica aggiornamento
					ProjectUpdated?.Invoke(existingProject);
					// Debug.Log($"Updated project: {existingProject.Name} (ID: {existingProject.Id})");
				},
				onError: err => ApiError?.Invoke(err)
			);
		}




		public void DeleteProject(int id, Project project)
		{
			StartCoroutine(DeleteProjectCoroutine(id, project));
		}

		private IEnumerator DeleteProjectCoroutine(int id, Project project)
		{
			projectList.Remove(project);
			openedProjectList.Remove(project); // ?

			yield return apiManager.DeleteProject(
			  id,
			  onSuccess: () => ProjectDeleted?.Invoke(project),
			  onError: err => ApiError?.Invoke(err)
			);
		}


		public void ProcessProject(int id, ConfigProcess configProcess)
		{
			var request = new ProcessProjectRequest
			{
				Downsampling = configProcess.Downsampling,
				ConfigParam = configProcess.Params
			};
			StartCoroutine(ProcessProjectCoroutine(id, request));
		}

		private IEnumerator ProcessProjectCoroutine(int id, ProcessProjectRequest processProjectRequest)
		{
			uiManager.SetLoadingView(true);
			yield return apiManager.ProcessProject(
				id,
				processProjectRequest,
				// onSuccess: processed =>
				// {
				// 	Project project = GetProject(id);
				// 	// Debug.Log(project == projectList.FirstOrDefault(p => p.Id == id));
				// 	if (project is not null)
				// 	{
				// 		ProjectProcessed?.Invoke(project, processed);
				// 	}
				// 	else
				// 	{
				// 		Debug.LogWarning($"Project with ID {id} not found.");
				// 	}
				// },
				onError: err => ApiError?.Invoke(err)
			);
			uiManager.SetLoadingView(false);
		}


		private void OnDisable()
		{
			ProjectsFetched = null;
			ProjectOpened = null;
			ProjectCreated = null;
			ProjectUpdated = null;
			ProjectDeleted = null;
			ProjectProcessed = null;
			ApiError = null;
		}


	}

}
