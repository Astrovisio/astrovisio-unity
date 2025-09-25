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

		// === Events ===
		public event Action<List<Project>> ProjectsFetched;
		public event Action<Project> ProjectOpened;
		public event Action<Project> ProjectCreated;
		public event Action<Project> ProjectUpdated;
		public event Action<Project> ProjectClosed;
		public event Action<Project> ProjectDeleted;
		public event Action ProjectUnselected;
		public event Action<Project, File> FileSelected;
		public event Action<Project, File> FileUpdated;
		public event Action<Project, File, DataPack> FileProcessed;
		public event Action<string> ApiError;

		// === Local ===
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

		public bool IsProjectOpened(int projectId)
		{
			return openedProjectList.Any(p => p.Id == projectId);
		}

		public Project GetProject(int projectId)
		{
			return projectList.FirstOrDefault(p => p.Id == projectId);
		}

		public File GetFile(int projectId, int fileId)
		{
			Project project = GetProject(projectId);
			return project?.Files?.FirstOrDefault(f => f.Id == fileId);
		}

		public Project GetCurrentProject()
		{
			return currentProject;
		}

		public Project GetOpenedProject(int id)
		{
			return openedProjectList.Find(p => p.Id == id);
		}

		public List<Project> GetProjectList()
		{
			return projectList;
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

		public async Task<Project> OpenProject(int id)
		{
			Project alreadyOpened = GetOpenedProject(id);
			if (alreadyOpened != null)
			{
				currentProject = alreadyOpened;
				ProjectOpened?.Invoke(alreadyOpened);
				return alreadyOpened;
			}

			Project result = null;
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

					if (!openedProjectList.Any(p => p.Id == project.Id) && existing != null)
					{
						openedProjectList.Add(existing);
					}

					currentProject = project;
					result = project;
					ProjectOpened?.Invoke(project);
				},
				error => ApiError?.Invoke(error));
			return result ?? currentProject;
		}

		public async Task<Project> CreateProject(string name, string description, string[] paths)
		{
			uiManager.SetLoadingView(true);

			CreateProjectRequest req = new CreateProjectRequest
			{
				Name = name,
				Description = description,
				Favourite = false,
				Paths = paths
			};

			Project created = null;
			await apiManager.CreateNewProject(req,
				createdProj =>
				{
					created = createdProj;
					projectList.Add(createdProj);
					ProjectCreated?.Invoke(createdProj);
					uiManager.SetLoadingView(false);
				},
				error =>
				{
					ApiError?.Invoke(error);
					uiManager.SetLoadingView(false);
				});
			return created;
		}

		public async Task<Project> DuplicateProject(string name, string description, Project projectToDuplicate)
		{
			uiManager.SetLoadingView(true);

			DuplicateProjectRequest req = new DuplicateProjectRequest
			{
				Name = name,
				Description = description
			};

			Project duplicated = null;
			await apiManager.DuplicateProject(
				projectToDuplicate.Id,
				req,
				duplicatedProj =>
				{
					duplicated = duplicatedProj;
					projectList.Add(duplicatedProj);
					ProjectCreated?.Invoke(duplicatedProj);
					uiManager.SetLoadingView(false);
				},
				error =>
				{
					ApiError?.Invoke(error);
					uiManager.SetLoadingView(false);
				}
			);
			return duplicated;
		}

		public async Task<Project> UpdateProject(int id, Project project)
		{
			UpdateProjectRequest req = new UpdateProjectRequest
			{
				Name = project.Name,
				Favourite = project.Favourite,
				Description = project.Description
			};

			Project updated = null;
			await apiManager.UpdateProject(id, req,
				updatedProj =>
				{
					updated = updatedProj;
					Project projectToUpdate = projectList.FirstOrDefault(p => p.Id == updatedProj.Id);
					if (projectToUpdate != null)
					{
						projectToUpdate.UpdateFrom(updatedProj);
					}
					else
					{
						projectList.Add(updatedProj);
					}
					ProjectUpdated?.Invoke(updatedProj);
				},
				error => ApiError?.Invoke(error));
			return updated;
		}

		public async void DeleteProject(int id, Project project)
		{
			await apiManager.DeleteProject(id,
				() =>
				{
					projectList.RemoveAll(p => p.Id == id);
					openedProjectList?.RemoveAll(p => p.Id == id);
					// Debug.Log($"Removed={removed}. Left: {string.Join(", ", projectList.Select(p => p.Name))}");

					ProjectDeleted?.Invoke(project);
				},
				error => ApiError?.Invoke(error));
		}

		public async void ProcessFile(int projectID, int fileID)
		{
			uiManager.SetLoadingBarProgress(0.0f, ProcessingStatusMessages.GetClientMessage("sending"));
			uiManager.SetLoadingView(true, LoaderType.Bar);

			try
			{
				// Get Job ID
				int? jobID = await apiManager.ProcessFile(projectID, fileID, error =>
				{
					ApiError?.Invoke(error);
					uiManager.SetLoadingView(false);
				});

				if (jobID == null)
				{
					Debug.LogError("Job ID is null. Aborting process.");
					return;
				}
				// Debug.Log($"[ProjectManager] Job ID: {jobID}");


				// Polling
				try
				{
					float progress = 0f;
					while (progress < 1.0f)
					{
						await Task.Delay(250);

						JobStatusResponse statusResponse = await apiManager.GetJobProgress(jobID.Value, error =>
						{
							ApiError?.Invoke(error);
						});

						if (statusResponse == null)
						{
							Debug.LogWarning("Null statusResponse"); // Err
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
				DataPack dataPack = await apiManager.GetJobResult(jobID.Value, error =>
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


				// Get project and file
				Project project = GetProject(projectID);
				File file = GetFile(projectID, fileID);

				// Get updated project and update file
				await apiManager.ReadProject(
				projectID,
				onSuccess: (p) =>
				{
					File updatedFile = p.Files?.FirstOrDefault(f => f.Id == fileID);
					// Debug.Log($"A {updatedFile.Id} {updatedFile.Name} {updatedFile.Processed}");
					file.UpdateFrom(updatedFile);
					// Debug.Log($"B {file.Id} {file.Name} {file.Processed}");
				},
				onError: (err) =>
				{
					ApiError?.Invoke(err);
					return;
				});


				if (project != null && project.Files != null)
				{
					if (file != null)
					{
						// Debug.Log($"Found file: {file.Name}, id={file.Id}, proc={file.Processed}");
						FileProcessed.Invoke(project, file, dataPack);
					}
					else
					{
						Debug.LogWarning($"No file found with id={fileID} in project={projectID}");
					}
				}
				else
				{
					Debug.LogError($"Project with id={projectID} not found or has no files");
				}

				uiManager.SetLoadingView(false);
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ProjectManager] Process failed: {ex.Message}");
				ApiError?.Invoke("Error during processing: " + ex.Message);
			}
			finally
			{
				uiManager.SetLoadingView(false);
			}
		}

		public async void UpdateFile(int projectId, File file)
		{
			UpdateFileRequest req = new UpdateFileRequest
			{
				Type = file.Type,
				Name = file.Name,
				Path = file.Path,
				Size = file.Size,
				Processed = file.Processed,
				Downsampling = file.Downsampling,
				Order = file.Order,
				ProcessedPath = file.ProcessedPath,
				Variables = file.Variables
			};

			// Debug.Log("[UpdateFile] Request payload:\n" + Newtonsoft.Json.JsonConvert.SerializeObject(req, Newtonsoft.Json.Formatting.Indented));

			// Debug.Log($"Z {file.Id} {file.Name} {file.Processed}");

			await apiManager.UpdateFile(projectId, file.Id, req,
				updatedFile =>
				{
					Project project = projectList.FirstOrDefault(p => p.Id == projectId);
					if (project != null && project.Files != null)
					{
						File fileToUpdate = project.Files.FirstOrDefault(f => f.Id == updatedFile.Id);
						if (fileToUpdate != null)
						{
							fileToUpdate.UpdateFrom(updatedFile);
						}
						else
						{
							project.Files.Add(updatedFile);
						}
					}
					Debug.Log($"[ProjectManager] File {updatedFile.Name} updated successfully.");
				},
				error =>
				{
					ApiError?.Invoke(error);
				});
		}

		public async void GetProcessedFile(int projectId, int fileId)
		{
			uiManager.SetLoadingView(true);

			await apiManager.GetProcessedFile(
				projectId,
				fileId,
				dataPack =>
				{
					try
					{
						int rows = dataPack?.Rows?.Length ?? 0;
						int cols = dataPack?.Columns?.Length ?? 0;
						Debug.Log($"[GetProcessedFile] RECEIVED DataPack -> rows={rows}, cols={cols}");

						Project project = GetProject(projectId);
						if (project == null)
						{
							Debug.LogWarning($"[GetProcessedFile] Project {projectId} not found.");
							return;
						}

						if (project.Files == null)
						{
							Debug.LogWarning($"[GetProcessedFile] Project {projectId} has no file list.");
							return;
						}

						File file = project.Files.FirstOrDefault(f => f.Id == fileId);
						if (file == null)
						{
							Debug.LogWarning($"[GetProcessedFile] File {fileId} not found in project {projectId}.");
							return;
						}

						if (!file.Processed)
						{
							file.Processed = true;
							ProjectUpdated?.Invoke(project);
							Debug.Log($"[GetProcessedFile] File {fileId} marked as processed.");
						}

						FileProcessed?.Invoke(project, file, dataPack);
						Debug.Log($"[GetProcessedFile] FileProcessed event invoked for file {fileId} in project {projectId}.");

						bool hasDC = RenderManager.Instance.TryGetDataContainer(projectId, fileId, out var _);
						Debug.Log($"[GetProcessedFile] DataContainer exists after event? {hasDC}");
					}
					finally
					{
						uiManager.SetLoadingView(false);
					}
				},
				error =>
				{
					ApiError?.Invoke(error);
					Debug.LogError($"[GetProcessedFile] API error: {error}");
					uiManager.SetLoadingView(false);
				}
			);
		}


		public void CloseProject(int id)
		{
			Project projectToRemove = openedProjectList.Find(p => p.Id == id);
			if (projectToRemove != null)
			{
				// Debug.Log("Closed project " + GetProject(id).Name);
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

		public void NotifyFileSelected(Project project, File file)
		{
			if (file == null)
			{
				Debug.LogWarning("[ProjectManager] NotifyFileSelected: file is null");
				return;
			}

			if (project == null)
			{
				Debug.LogWarning($"[ProjectManager] NotifyFileSelected: project {project.Name} not found");
				return;
			}

			FileSelected?.Invoke(project, file);
		}

		// TODO: evaluate utility of this function
		public void NotifyFileUpdated(Project project, File file)
		{
			if (file == null)
			{
				Debug.LogWarning("[ProjectManager] NotifyFileSelected: file is null");
				return;
			}

			if (project == null)
			{
				Debug.LogWarning($"[ProjectManager] NotifyFileSelected: project {project.Name} not found");
				return;
			}

			FileUpdated?.Invoke(project, file);
		}

		private void SaveProjectCSV(DataPack dataPack)
		{
			if (dataPack == null)
			{
				Debug.LogError("DataPack nullo, impossibile salvare il CSV");
				return;
			}

			StringBuilder sb = new StringBuilder();

			if (dataPack.Columns != null && dataPack.Columns.Length > 0)
			{
				sb.AppendLine(string.Join(",", dataPack.Columns));
			}

			if (dataPack.Rows != null)
			{
				foreach (var row in dataPack.Rows)
				{
					string[] values = new string[row.Length];
					for (int i = 0; i < row.Length; i++)
					{
						values[i] = row[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
					}
					sb.AppendLine(string.Join(",", values));
				}
			}

			string filePath = Path.Combine(Application.persistentDataPath, "project.csv");
			System.IO.File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
			Debug.Log("CSV saved in: " + filePath);
		}

		private void OnDisable()
		{
			ProjectsFetched = null;
			ProjectOpened = null;
			ProjectCreated = null;
			ProjectUpdated = null;
			ProjectClosed = null;
			FileProcessed = null;
			ProjectUnselected = null;
			ProjectDeleted = null;
			ApiError = null;
		}

	}

}
