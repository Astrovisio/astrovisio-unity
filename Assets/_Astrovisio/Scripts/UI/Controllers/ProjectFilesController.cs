using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class ProjectFilesController
    {
        public ProjectManager ProjectManager { get; }
        public Project Project { get; }
        public VisualElement Root { get; }
        public UIContextSO UIContextSO { get; }

        public IReadOnlyList<FileState> Items => fileList;

        private readonly List<FileState> fileList = new();
        private readonly ListView listView;


        public ProjectFilesController(ProjectManager projectManager, Project project, VisualElement root, UIContextSO ctx, Action onUpdateAction = null, Action<FileState> onClickAction = null)
        {
            ProjectManager = projectManager;
            Project = project;
            Root = root;
            UIContextSO = ctx;

            listView = Root.Q<ListView>();
            if (listView == null)
            {
                Debug.LogError("ListView not found.");
                return;
            }

            listView.itemsSource = fileList;
            listView.selectionType = SelectionType.None;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;

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
                    if (entry.state)
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
                    deleteButton.clickable = new Clickable(() =>
                    {
                        Debug.Log("Remove file here... API CALL");
                        ProjectManager.RemoveFile(Project.Id, current.file.Id);
                        RemoveFile(current);
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

                if (fs.state != value)
                {
                    fs.state = value;
                    fileList[i] = fs;
                }

                Refresh();
                return;
            }

            Debug.LogWarning($"SetFileState: file with Id={file.Id} not found in list.");
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
                    Debug.Log($"[{i}] FileState -> Name={fileState.fileInfo.Name}, Size={fileState.fileInfo.Size}, Path={fileState.fileInfo.Path}, State={fileState.state}");
                }
                else
                {
                    Debug.Log($"[{i}] {listView.itemsSource[i]}");
                }
            }
        }

    }

}
