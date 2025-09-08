using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Astrovisio
{
	public class ProjectManager : MonoBehaviour
	{
		[Header("Dependencies")]
		[SerializeField] private APIManager apiManager;
		[SerializeField] private UIManager uiManager;

		[Header("Debug")]
		[SerializeField] private bool saveProjectCSV = false;

		public event Action<List<Project>> ProjectsFetched;
		public event Action<Project> ProjectOpened;
		public event Action<Project> ProjectCreated;
		public event Action<Project> ProjectUpdated;
		public event Action<Project> ProjectClosed;
		public event Action<Project, DataPack> ProjectProcessed;
		public event Action ProjectUnselected;
		public event Action<Project> ProjectDeleted;
		public event Action<string> ApiError;

		private List<Project> projectList = new();
		private List<Project> openedProjectList = new();
		private Project currentProject;

		private void Awake()
		{
			if (apiManager == null)
			{
				Debug.LogError("[ProjectManager] APIManager reference is missing.");
				enabled = false;
			}
		}

		public bool IsProjectOpened(int id) => openedProjectList.Any(p => p.Id == id);
		public Project GetProject(int id) => projectList.FirstOrDefault(p => p.Id == id);
		public Project GetCurrentProject() => currentProject;
		public Project GetOpenedProject(int id) => openedProjectList.Find(p => p.Id == id);
		public List<Project> GetProjectList() => projectList;

		public void CloseProject(int id)
		{
			Project projectToRemove = openedProjectList.Find(p => p.Id == id);
			if (projectToRemove != null)
			{
				openedProjectList.Remove(projectToRemove);
				if (currentProject?.Id == id)
				{
					currentProject = null;
				}
				ProjectClosed?.Invoke(projectToRemove);
			}
		}

		public void UnselectProject()
		{
			if (currentProject != null)
			{
				currentProject = null;
				ProjectUnselected?.Invoke();
			}
		}

		public async void FetchAllProjects()
		{
			uiManager.SetLoadingView(true);

			await apiManager.ReadProjects(
				projects =>
				{
					projectList = projects;
					ProjectsFetched?.Invoke(projectList);
					uiManager.SetLoadingView(false);
				},
				error =>
				{
					ApiError?.Invoke(error);
					uiManager.SetLoadingView(false);
				});
		}

		public async void OpenProject(int id)
		{
			Project alreadyOpened = openedProjectList.Find(p => p.Id == id);
			if (alreadyOpened != null)
			{
				currentProject = alreadyOpened;
				ProjectOpened?.Invoke(alreadyOpened);
				return;
			}

			await apiManager.ReadProject(id,
				project =>
				{
					Project existing = projectList.FirstOrDefault(p => p.Id == project.Id);
					if (existing != null)
					{
						existing.UpdateFrom(project);
					}
					else
					{
						projectList.Add(project);
					}

					if (!openedProjectList.Any(p => p.Id == project.Id))
					{
						openedProjectList.Add(project);
					}

					currentProject = project;
					ProjectOpened?.Invoke(project);
				},
				error => ApiError?.Invoke(error));
		}

		public async void CreateProject(string name, string description, string[] paths)
		{
			uiManager.SetLoadingView(true);

			CreateProjectRequest req = new CreateProjectRequest
			{
				Name = name,
				Description = description,
				Favourite = false,
				Paths = paths
			};

			await apiManager.CreateNewProject(req,
				created =>
				{
					projectList.Add(created);
					ProjectCreated?.Invoke(created);
					uiManager.SetLoadingView(false);
				},
				error =>
				{
					ApiError?.Invoke(error);
					uiManager.SetLoadingView(false);
				});
		}

		public async void DuplicateProject(string name, string description, Project projectToDuplicate)
		{
			uiManager.SetLoadingView(true);

			CreateProjectRequest createReq = new CreateProjectRequest
			{
				Name = name,
				Description = description,
				Favourite = false,
				Paths = projectToDuplicate.Paths
			};

			Project createdProject = null;
			bool createSuccess = false;

			await apiManager.CreateNewProject(createReq,
				created =>
				{
					createdProject = created;
					projectList.Add(created);
					createSuccess = true;
				},
				error =>
				{
					ApiError?.Invoke(error);
					createSuccess = false;
				});

			if (!createSuccess || createdProject == null)
			{
				uiManager.SetLoadingView(false);
				return;
			}

			UpdateProjectRequest updateReq = new UpdateProjectRequest
			{
				Name = createdProject.Name,
				Description = createdProject.Description,
				Favourite = false,
				Paths = createdProject.Paths,
				ConfigProcess = projectToDuplicate.ConfigProcess.DeepCopy()
			};

			await apiManager.UpdateProject(createdProject.Id, updateReq,
				updated =>
				{
					createdProject.UpdateFrom(updated);
					ProjectCreated?.Invoke(createdProject);
					uiManager.SetLoadingView(false);
				},
				error =>
				{
					ApiError?.Invoke(error);
					uiManager.SetLoadingView(false);
				});
		}

		public async void UpdateProject(int id, Project project)
		{
			UpdateProjectRequest req = new UpdateProjectRequest
			{
				Name = project.Name,
				Favourite = project.Favourite,
				Description = project.Description,
				Paths = project.Paths,
				ConfigProcess = project.ConfigProcess
			};

			await apiManager.UpdateProject(id, req,
				updated =>
				{
					Project projectToUpdate = projectList.FirstOrDefault(p => p.Id == updated.Id);
					if (projectToUpdate != null)
					{
						projectToUpdate.UpdateFrom(updated);
					}
					else
					{
						projectList.Add(updated);
					}
					ProjectUpdated?.Invoke(updated);
				},
				error => ApiError?.Invoke(error));
		}

		public async void DeleteProject(int id, Project project)
		{
			await apiManager.DeleteProject(id,
				() =>
				{
					projectList.Remove(project);
					openedProjectList.Remove(project);
					ProjectDeleted?.Invoke(project);
				},
				error => ApiError?.Invoke(error));
		}

		public async void ProcessProject(int id, ConfigProcess configProcess)
		{
			uiManager.SetLoadingBarProgress(0.0f, ProcessingStatusMessages.GetClientMessage("sending"));
			uiManager.SetLoadingView(true, LoaderType.Bar);

			ProcessProjectRequest req = new ProcessProjectRequest
			{
				Downsampling = configProcess.Downsampling,
				ConfigParam = configProcess.Params
			};

			try
			{
				// Get Job ID
				int? jobID = await apiManager.ProcessProject(id, req, error =>
				{
					ApiError?.Invoke(error);
					uiManager.SetLoadingView(false);
				});

				if (jobID == null)
				{
					Debug.LogError("Job ID is null. Aborting process.");
					return;
				}
				Debug.Log($"[ProjectManager] Job ID: {jobID}");


				// Polling
				try
				{
					float progress = 0f;
					while (progress < 1.0f)
					{
						await Task.Delay(250);

						JobStatusResponse statusResponse = await apiManager.GetProjectJobStatus(id, jobID.Value, error =>
						{
							ApiError?.Invoke(error);
						});

						if (statusResponse == null)
						{
							Debug.LogWarning("Null statusResponse");
							continue;
						}

						progress = statusResponse.Progress;
						// Debug.Log($"Polling job {jobID}: status={status.Status}, progress={progress}");

						uiManager.SetLoadingBarProgress(progress, ProcessingStatusMessages.GetClientMessage(statusResponse.Status));
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"[ProjectManager] Process failed: {ex.Message}");
					ApiError?.Invoke("Errore nel processamento: " + ex.Message);
					uiManager.SetLoadingView(false);
				}


				// Get DataPack
				uiManager.SetLoadingBarProgress(1.0f, ProcessingStatusMessages.GetClientMessage("loading"));
				DataPack dataPack = await apiManager.FetchProjectProcessedData(id, jobID.Value, error =>
				{
					ApiError?.Invoke(error);
				});

				if (dataPack == null)
				{
					Debug.LogError("DataPack is null. Aborting process.");
					return;
				}

				if (saveProjectCSV)
				{
					SaveProjectCSV(dataPack);
				}

				Project project = GetProject(id);
				if (project != null)
				{
					ProjectProcessed?.Invoke(project, dataPack);
					Debug.Log($"[ProjectManager] Process completed, rows: {dataPack.Rows.Length}");
				}

				uiManager.SetLoadingView(false);
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ProjectManager] Process failed: {ex.Message}");
				ApiError?.Invoke("Errore nel processamento: " + ex.Message);
			}
			finally
			{
				uiManager.SetLoadingView(false);
			}
		}

		private void SaveProjectCSV(DataPack dataPack)
		{
			if (dataPack == null)
			{
				Debug.LogError("DataPack nullo, impossibile salvare il CSV");
				return;
			}

			StringBuilder sb = new StringBuilder();

			// 1. Intestazioni (columns)
			if (dataPack.Columns != null && dataPack.Columns.Length > 0)
			{
				sb.AppendLine(string.Join(",", dataPack.Columns));
			}

			// 2. Righe (rows)
			if (dataPack.Rows != null)
			{
				foreach (var row in dataPack.Rows)
				{
					// row è double[], convertiamolo in stringhe
					string[] values = new string[row.Length];
					for (int i = 0; i < row.Length; i++)
					{
						// Usa ToString con InvariantCulture per evitare problemi con la virgola decimale
						values[i] = row[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
					}
					sb.AppendLine(string.Join(",", values));
				}
			}

			// 3. Percorso del file (Unity: cartella scrivibile sicura)
			string filePath = Path.Combine(Application.persistentDataPath, "project.csv");

			File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

			Debug.Log("CSV salvato in: " + filePath);
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