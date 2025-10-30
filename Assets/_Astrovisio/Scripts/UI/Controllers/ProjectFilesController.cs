/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class ProjectFilesController
    {

        // === Dependencies ===
        public ProjectManager ProjectManager { get; }
        public Project Project { get; }
        public VisualElement Root { get; }
        public UIContextSO UIContextSO { get; }

        // === Actions ===
        private Action onUpdateAction;
        private Action<FileState> onClickAction;

        // === Local ===
        private readonly List<FileState> fileList = new();
        private ListView listView;
        private Label fileCounterLabel;
        private Button addFileButton;

        public ProjectFilesController(
            ProjectManager projectManager,
            Project project,
            VisualElement root,
            UIContextSO ctx,
            Action onUpdateAction = null,
            Action<FileState> onClickAction = null)
        {
            ProjectManager = projectManager;
            Project = project;
            Root = root;
            UIContextSO = ctx;

            this.onUpdateAction = onUpdateAction;
            // this.onClickAction += OnClickActionInternal;
            this.onClickAction += onClickAction;


            ProjectManager.ProjectUpdated += OnProjectUpdated;

            InitFileCounterLabel();
            InitAddFileButton();
            InitListView();
        }

        private void InitFileCounterLabel()
        {
            fileCounterLabel = Root.Q<Label>("FileCounterLabel");
            UpdateFileCounter();
        }

        private void InitAddFileButton()
        {
            addFileButton = Root.Q<VisualElement>("ButtonAddFile")?.Q<Button>();
            addFileButton.clicked += async () =>
            {
                await OnUploadFileClicked();
                onUpdateAction?.Invoke();
            };
        }

        private void InitListView()
        {
            listView = Root.Q<ListView>();
            if (listView == null)
            {
                Debug.LogError("ListView not found.");
                return;
            }

            listView.itemsSource = fileList;
            listView.selectionType = SelectionType.Single;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;

            listView.schedule.Execute(() =>
            {
                listView.selectedIndex = 0; // oppure listView.SetSelection(0);
            }).StartingIn(0);

            listView.makeItem = () =>
            {
                // Debug.Log("[FilesController] makeItem: creating row VisualElement");

                VisualElement ve = UIContextSO.listItemFileStateTemplate.CloneTree();

                ve.RegisterCallback<ClickEvent>(evt =>
                {
                    // Debug.Log("[FilesController] Row clicked");

                    if (onClickAction == null)
                    {
                        Debug.LogWarning("[FilesController] onClickAction is null; skipping invocation");
                        return;
                    }

                    // Debug.Log($"[FilesController] userData is {(ve.userData == null ? "null" : ve.userData.GetType().Name)}");

                    if (ve.userData is FileState item)
                    {
                        onClickAction(item);
                        evt.StopPropagation();
                    }
                    else
                    {
                        Debug.LogWarning("[FilesController] userData is not of type FileState; click ignored");
                    }
                });

                return ve;
            };


            listView.bindItem = (listItemVisualElement, i) =>
            {

                FileState entry = fileList[i];

                listItemVisualElement.userData = entry;

                Label name = listItemVisualElement.Q<Label>("NameLabel");
                if (name != null)
                {
                    name.text = entry.Name;
                }

                Label size = listItemVisualElement.Q<Label>("SizeLabel");
                if (size != null)
                {
                    size.text = FormatFileSize(entry.Size);
                }

                VisualElement listItemState = listItemVisualElement.Q<VisualElement>("State");
                if (listItemState != null)
                {
                    if (entry.processed)
                    {
                        listItemState.AddToClassList("active");
                    }
                    else
                    {
                        listItemState.RemoveFromClassList("active");
                    }
                }

                Button deleteButton = listItemVisualElement.Q<Button>("CloseButton");
                if (deleteButton != null)
                {
                    deleteButton.clickable = null;
                    FileState current = entry;
                    deleteButton.clickable = new Clickable(async () =>
                    {
                        // Debug.LogWarning(Project.Files.Count);
                        if (Project.Files.Count <= 1)
                        {
                            Debug.LogWarning("Can't remove the last file.");
                            return;
                        }

                        // Remove file
                        await ProjectManager.RemoveFile(Project.Id, current.file.Id);
                        RemoveFile(current);

                        // Update order
                        onUpdateAction?.Invoke();
                        listView.RefreshItems();

                        // Final updates
                        onClickAction(fileList[0]);
                        listView.selectedIndex = 0;
                        UpdateFileCounter();
                    });
                }
            };


            listView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                Debug.Log("itemIndexChanged");
                onUpdateAction?.Invoke();
                listView.RefreshItems();
                // PrintListView();
            };
        }

        public void AddFile(FileState entry)
        {
            fileList.Add(entry);
            Refresh();
        }

        public void RemoveFile(FileState entry)
        {
            if (fileList.Remove(entry))
            {
                Refresh();
            }
        }

        public List<FileState> GetFileList()
        {
            return fileList;
        }

        public void ClearAll()
        {
            if (fileList.Count == 0)
            {
                return;
            }
            fileList.Clear();
            Refresh();
        }

        private void Refresh()
        {
            listView.RefreshItems();
            // onUpdateAction?.Invoke(); // Not needed ?
        }

        private string FormatFileSize(long sizeInBytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (sizeInBytes >= GB)
            {
                return $"{sizeInBytes / (float)GB:F2}GB";
            }
            else if (sizeInBytes >= MB)
            {
                return $"{sizeInBytes / (float)MB:F2}MB";
            }
            else
            {
                return $"{sizeInBytes / (float)KB:F2}KB";
            }
        }

        public long GetTotalSizeBytes()
        {
            return fileList.Sum(file => file.Size);
        }

        public string GetFormattedTotalSize()
        {
            return FormatFileSize(GetTotalSizeBytes());
        }

        public void SetFileState(File file, bool value)
        {
            if (file == null)
            {
                Debug.LogWarning("SetFileState: file is null.");
                return;
            }

            for (int i = 0; i < fileList.Count; i++)
            {
                var fs = fileList[i];

                bool isMatch =
                    ReferenceEquals(fs.file, file) ||
                    (fs.file != null && fs.file.Id == file.Id);

                if (!isMatch)
                {
                    continue;
                }

                if (fs.processed != value)
                {
                    fs.processed = value;
                    fileList[i] = fs;
                }

                Refresh();
                return;
            }

            Debug.LogWarning($"SetFileState: file with Id={file.Id} not found in list.");
        }

        private void UpdateFileCounter()
        {
            // Debug.Log("File counter: " + Project.Files.Count);
            fileCounterLabel.text = $"Files ({Project.Files.Count})";
        }

        private async Task OnUploadFileClicked()
        {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select file", "", "", false);

            if (paths.Length > 0)
            {
                foreach (string path in paths)
                {
                    if (!System.IO.File.Exists(path))
                    {
                        Debug.LogWarning($"File not found: {path}");
                        continue;
                    }

                    // Check if the file extension is supported (.hdf5 or .fits)
                    string extension = Path.GetExtension(path).ToLowerInvariant();
                    if (extension != ".hdf5" && extension != ".fits")
                    {
                        Debug.LogWarning($"Unsupported file format: {path}");
                        continue;
                    }

                    // Skip if the file is already in the list
                    if (fileList.Any(file => file.Path == path))
                    {
                        Debug.Log($"File already added: {path}");
                        continue;
                    }

                    string relativePath = "data/" + Path.GetFileName(path);

                    File fileAdded = await ProjectManager.AddFile(Project.Id, relativePath);
                    if (fileAdded == null)
                    {
                        return;
                    }
                    // fileAdded.Order = fileList.Count - 1;
                    // ProjectManager.UpdateFile(Project.Id, fileAdded);


                    FileInfo fileInfo = new FileInfo(fileAdded.Path, fileAdded.Name, fileAdded.Size);
                    FileState fileState = new FileState(fileInfo, fileAdded, false);
                    AddFile(fileState);
                    Debug.Log($"File added: {fileState.Name} ({fileState.Size} bytes) - {fileState.Path} @ Order {fileAdded.Order}");
                }
            }
        }

        private void OnProjectUpdated(Project project)
        {
            if (project == null || project.Id != Project.Id)
            {
                return;
            }

            UpdateFileCounter();
        }

        public void Dispose()
        {
            ProjectManager.ProjectUpdated -= OnProjectUpdated;
        }

        public void PrintListView()
        {
            if (listView?.itemsSource == null)
            {
                Debug.LogWarning("ListView or itemsSource is null.");
                return;
            }

            Debug.Log($"[ListView] Count = {listView.itemsSource.Count}");

            for (int i = 0; i < listView.itemsSource.Count; i++)
            {
                if (listView.itemsSource[i] is FileState fileState)
                {
                    Debug.Log($"[{i}] FileState -> Name={fileState.fileInfo.Name}, Size={fileState.fileInfo.Size}, Path={fileState.fileInfo.Path}, State={fileState.processed}");
                }
                else
                {
                    Debug.Log($"[{i}] {listView.itemsSource[i]}");
                }
            }
        }

    }

}
