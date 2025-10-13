using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFB;
using UnityEngine;

namespace Astrovisio
{
	public class ProjectManager : MonoBehaviour
	{
		[Header("Dependencies")]
		[SerializeField] private APIManager apiManager;
		[SerializeField] private UIManager uiManager;
		[SerializeField] private SettingsManager settingsManager;

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
		public event Action<Project, File> FileAdded;
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
			Project opened = GetOpenedProject(id);
			if (opened != null)
			{
				currentProject = opened;
				ProjectOpened?.Invoke(opened);
				return opened;
			}

			Project projectResult = null;

			await apiManager.ReadProject(
				id,
				projectFromApi =>
				{
					Project existing = projectList.FirstOrDefault(p => p.Id == projectFromApi.Id);
					if (existing != null)
					{
						existing.UpdateFrom(projectFromApi);
						projectResult = existing;
					}
					else
					{
						projectList.Add(projectFromApi);
						projectResult = projectFromApi;
					}

					if (!openedProjectList.Any(p => p.Id == projectResult.Id))
					{
						openedProjectList.Add(projectResult);
					}

					currentProject = projectResult;
					ProjectOpened?.Invoke(projectResult);
				},
				error =>
				{
					ApiError?.Invoke(error);
				}
			);

			return projectResult;
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

		public async void ProcessFile(int projectId, int fileId)
		{
			uiManager.SetLoadingBarProgress(0.0f, ProcessingStatusMessages.GetClientMessage("sending"));
			uiManager.SetLoadingView(true, LoaderType.Bar);

			try
			{
				// Get Job ID
				int? jobID = await apiManager.ProcessFile(projectId, fileId, error =>
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
				Project project = GetProject(projectId);
				File file = GetFile(projectId, fileId);

				// Get updated project and update file
				await apiManager.ReadProject(
				projectId,
				onSuccess: (p) =>
				{
					File updatedFile = p.Files?.FirstOrDefault(f => f.Id == fileId);
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
						Debug.LogWarning($"No file found with id={fileId} in project={projectId}");
					}
				}
				else
				{
					Debug.LogError($"Project with id={projectId} not found or has no files");
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

			// Debug.Log($"Request: {file.Id} {file.Name} {file.Order}");

			await apiManager.UpdateFile(projectId, file.Id, req,
				updatedFile =>
				{
					// Debug.Log("Sended: " + req.Order);
					// Debug.Log("Received: " + updatedFile.Order);

					Project project = projectList.FirstOrDefault(p => p.Id == projectId);
					if (project != null && project.Files != null)
					{
						File fileToUpdate = project.Files.FirstOrDefault(f => f.Id == updatedFile.Id);
						if (fileToUpdate != null)
						{
							fileToUpdate.UpdateFrom(updatedFile);
							FileUpdated?.Invoke(project, fileToUpdate);
						}
						else
						{
							project.Files.Add(updatedFile);
						}
					}
					// Debug.Log($"[ProjectManager] File {updatedFile.Name} updated successfully.");
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
						// Debug.Log($"[GetProcessedFile] RECEIVED DataPack -> rows={rows}, cols={cols}");

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
						// Debug.Log($"[GetProcessedFile] FileProcessed event invoked for file {fileId} in project {projectId}.");

						bool hasDC = RenderManager.Instance.TryGetDataContainer(projectId, fileId, out var _);
						// Debug.Log($"[GetProcessedFile] DataContainer exists after event? {hasDC}");
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

		public async Task<File> AddFile(int projectId, string path)
		{
			// Pre-checks
			Project project = GetProject(projectId);
			if (project == null)
			{
				Debug.LogWarning($"[ProjectManager] AddFile: project {projectId} not found.");
				return null;
			}
			if (string.IsNullOrWhiteSpace(path))
			{
				Debug.LogWarning($"[ProjectManager] AddFile: invalid path.");
				return null;
			}
			if (project.Files != null && project.Files.Any(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase)))
			{
				// Already present locally
				return project.Files.FirstOrDefault(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase));
			}

			// Build payload with the new path appended
			string[] newPaths = (project.Files ?? new List<File>())
				.Select(f => f.Path)
				.Where(p => !string.IsNullOrEmpty(p))
				.Concat(new[] { path })
				.ToArray();

			ReplaceProjectFilesRequest req = new ReplaceProjectFilesRequest { Paths = newPaths };

			uiManager.SetLoadingView(true);
			try
			{
				// Server update
				try
				{
					await apiManager.ReplaceProjectFiles(projectId, req);
				}
				catch (Exception ex)
				{
					Debug.LogError($"[ProjectManager] AddFile: API error replacing files. {ex.Message}");
					ApiError?.Invoke($"ReplaceProjectFiles failed: {ex.Message}");
					return null;
				}

				// Read back authoritative state
				Project updated;
				try
				{
					TaskCompletionSource<Project> tcs = new TaskCompletionSource<Project>();
					await apiManager.ReadProject(
						projectId,
						onSuccess: p => tcs.TrySetResult(p),
						onError: err => tcs.TrySetException(new Exception(err))
					);
					updated = await tcs.Task;
				}
				catch (Exception ex)
				{
					Debug.LogError($"[ProjectManager] AddFile: ReadProject exception. {ex.Message}");
					return null;
				}
				if (updated == null)
				{
					Debug.LogWarning($"[ProjectManager] AddFile: ReadProject returned null for {projectId}.");
					return null;
				}

				Project updatedProject = GetProject(projectId) ?? project;
				updatedProject.UpdateFrom(updated);
				ProjectUpdated?.Invoke(updatedProject);
				return updatedProject.Files.FirstOrDefault(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase));
			}
			finally
			{
				uiManager.SetLoadingView(false);
			}
		}

		public async Task<bool> RemoveFile(int projectId, int fileId)
		{
			// Pre-checks
			Project project = GetProject(projectId);
			if (project == null || project.Files == null || project.Files.Count == 0)
			{
				Debug.LogWarning($"[ProjectManager] RemoveFile: project {projectId} not found or has no files.");
				return false;
			}
			if (!project.Files.Any(f => f.Id == fileId))
			{
				Debug.LogWarning($"[ProjectManager] RemoveFile: file {fileId} not found in project {projectId}.");
				return false;
			}

			uiManager.SetLoadingView(true);
			try
			{
				// Build server payload without the removed file
				string[] remainingPaths = project.Files
					.Where(f => f.Id != fileId)
					.Select(f => f.Path)
					.Where(p => !string.IsNullOrEmpty(p))
					.ToArray();

				ReplaceProjectFilesRequest req = new ReplaceProjectFilesRequest { Paths = remainingPaths };

				// Server update
				try
				{
					await apiManager.ReplaceProjectFiles(projectId, req);
				}
				catch (Exception ex)
				{
					Debug.LogError($"[ProjectManager] RemoveFile: API error replacing files. {ex.Message}");
					ApiError?.Invoke($"ReplaceProjectFiles failed: {ex.Message}");
					return false;
				}

				// Read back authoritative state
				Project updated;
				try
				{
					var tcs = new TaskCompletionSource<Project>();
					await apiManager.ReadProject(
						projectId,
						onSuccess: p => tcs.TrySetResult(p),
						onError: err => tcs.TrySetException(new Exception(err))
					);
					updated = await tcs.Task;
				}
				catch (Exception ex)
				{
					Debug.LogError($"[ProjectManager] RemoveFile: ReadProject exception. {ex.Message}");
					return false;
				}
				if (updated == null)
				{
					Debug.LogWarning($"[ProjectManager] RemoveFile: ReadProject returned null for {projectId}.");
					return false;
				}

				Project updatedProject = GetProject(projectId) ?? project;

				Debug.Log("updatedProject.Files.Count " + updatedProject.Files.Count);
				Debug.Log("updated.Files.Count " + updated.Files.Count);

				updatedProject.UpdateFrom(updated);
				ProjectUpdated?.Invoke(updatedProject);
				return true;
			}
			finally
			{
				uiManager.SetLoadingView(false);
			}
		}

		public async Task UpdateFileOrder(int projectId, int[] orderIds)
		{
			Project project = GetProject(projectId);
			if (project == null || project.Files == null || project.Files.Count == 0)
			{
				Debug.LogWarning($"[UpdateFileOrder] Project {projectId} not found or has no files.");
				return;
			}
			if (orderIds == null || orderIds.Length != project.Files.Count)
			{
				Debug.LogWarning($"[UpdateFileOrder] Invalid order length. Expected {project.Files.Count}, got {orderIds?.Length ?? 0}.");
				return;
			}

			UpdateProjectRequest req = new UpdateProjectRequest
			{
				Name = project.Name,
				Description = project.Description,
				Favourite = project.Favourite,
				Order = orderIds
			};

			uiManager.SetLoadingView(true);
			Project updatedFromApi = null;

			await apiManager.UpdateProject(
				projectId,
				req,
				onSuccess: p =>
				{
					updatedFromApi = p;
				},
				onError: err =>
				{
					ApiError?.Invoke(err);
					Debug.LogError($"[UpdateFileOrder] API error: {err}");
				});


			if (updatedFromApi == null)
			{
				return;
			}

			project.UpdateFrom(updatedFromApi);
			ProjectUpdated?.Invoke(project);
			uiManager.SetLoadingView(false);

			foreach (File file in project.Files)
			{
				Debug.Log($"{file.Name} order: {file.Order}");
			}
		}

		// TODO: settings


		// --------------------
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

		public async Task<Project> CreateProjectFromSavedProject(SavedProject savedProject)
		{
			Project createdProject = await CreateProject(savedProject.Project.Name, savedProject.Project.Description, savedProject.GetFilePaths());

			if (createdProject == null)
			{
				return null;
			}

			savedProject.Project.Id = createdProject.Id;

			List<File> SortedList = savedProject.Project.Files.OrderBy(o => o.Order).ToList();
			List<int> OrderedFileIDs = new List<int>();

			foreach (File f in SortedList)
			{
				UpdateFile(savedProject.Project.Id, f);
				OrderedFileIDs.Add(f.Id);
			}

			await UpdateFileOrder(createdProject.Id, OrderedFileIDs.ToArray());

			return createdProject;
		}

		[ContextMenu("SaveProjectToJSON")]
		public void SaveProjectToJSON()
		{
			SavedProject savedProject = new SavedProject();
			savedProject.Project = GetCurrentProject();
			savedProject.FilesSettings = new List<Settings>(settingsManager.GetCurrentProjectFilesSettings());

			string file = StandaloneFileBrowser.SaveFilePanel("Save Project", "", savedProject.Project.Name + ".json", "json");
			System.IO.File.WriteAllText(file, DebugUtility.TryPrettifyJson(JsonConvert.SerializeObject(savedProject)), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		}

		[ContextMenu("LoadProjectFromJSON")]
		public void LoadProjectFromJSON()
		{
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select file", "", "json", false);
			if (paths.Length > 0)
			{
				StreamReader sr = new StreamReader(paths[0]);
				string fileContents = sr.ReadToEnd();
				sr.Close();

				SavedProject savedProject = JsonConvert.DeserializeObject<SavedProject>(fileContents);

				_ = CreateProjectFromSavedProject(savedProject);
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
