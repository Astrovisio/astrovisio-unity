using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class NewProjectFilesController
    {
        public VisualElement Root { get; }
        public UIContextSO UIContextSO { get; }

        public IReadOnlyList<FileInfo> Items => fileList;

        private readonly List<FileInfo> fileList = new();
        private readonly ListView listView;

        private readonly Action onUpdateAction;
        private readonly Action<FileInfo> onClickAction;


        public NewProjectFilesController(VisualElement root, UIContextSO ctx, Action onUpdateAction = null, Action<FileInfo> onClickAction = null)
        {
            Root = root;
            UIContextSO = ctx;
            this.onUpdateAction = onUpdateAction;
            this.onClickAction = onClickAction;

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
                VisualElement ve = UIContextSO.listItemFileTemplate.CloneTree();

                ve.RegisterCallback<ClickEvent>(evt =>
                {
                    if (onClickAction == null)
                    {
                        Debug.LogWarning("[FilesController] onClickAction is null; skipping invocation");
                        return;
                    }

                    if (ve.userData is FileInfo item)
                    {
                        onClickAction(item);
                        evt.StopPropagation();
                    }
                    else
                    {
                        Debug.LogWarning("[FilesController] userData is not of type FileInfo; click ignored");
                    }
                });

                return ve;
            };


            listView.bindItem = (listItemVisualElement, i) =>
            {
                FileInfo entry = fileList[i];

                listItemVisualElement.userData = entry;

                // Name
                Label name = listItemVisualElement.Q<Label>("NameLabel");
                if (name != null)
                {
                    name.text = entry.Name;
                }

                // Size
                Label size = listItemVisualElement.Q<Label>("SizeLabel");
                if (size != null)
                {
                    size.text = FormatFileSize(entry.Size);
                }

                // State
                listItemVisualElement.Q<VisualElement>("State");
                listItemVisualElement.RemoveFromClassList("active");


                // Delete
                Button deleteButton = listItemVisualElement.Q<Button>("CloseButton");
                if (deleteButton != null)
                {
                    deleteButton.clickable = null;
                    FileInfo current = entry;
                    deleteButton.clickable = new Clickable(() =>
                    {
                        RemoveFile(current);
                        // PrintListView();
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

        public void AddFile(FileInfo entry)
        {
            fileList.Add(entry);
            Refresh();
        }


        public void RemoveFile(FileInfo entry)
        {
            if (fileList.Remove(entry))
            {
                Refresh();
            }
        }

        public List<FileInfo> GetFileList()
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
            onUpdateAction?.Invoke();
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
                if (listView.itemsSource[i] is FileInfo fileInfo)
                {
                    Debug.Log($"[{i}] FileInfo -> Name={fileInfo.Name}, Size={fileInfo.Size}, Path={fileInfo.Path}");
                }
                else
                {
                    Debug.Log($"[{i}] {listView.itemsSource[i]}");
                }
            }
        }

    }

}
