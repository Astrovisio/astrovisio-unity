using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class FilesController<T> where T : IFileEntry
    {
        public VisualElement Root { get; }
        public UIContextSO UIContextSO { get; }

        public IReadOnlyList<T> Items => fileList;

        private readonly List<T> fileList = new();
        private readonly ListView listView;

        private readonly Action onUpdateAction;
        private readonly Action<T> onClickAction;


        public FilesController(VisualElement root, UIContextSO ctx, Action onUpdateAction = null, Action<T> onClickAction = null)
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
                // Debug.Log("[FilesController] makeItem: creating row VisualElement");

                VisualElement ve = (typeof(T) == typeof(FileState)
                    ? UIContextSO.listItemFileStateTemplate
                    : (UIContextSO.listItemFileTemplate ?? UIContextSO.listItemFileStateTemplate)
                ).CloneTree();

                ve.RegisterCallback<ClickEvent>(evt =>
                {
                    // Debug.Log("[FilesController] Row clicked");

                    if (onClickAction == null)
                    {
                        Debug.LogWarning("[FilesController] onClickAction is null; skipping invocation");
                        return;
                    }

                    // Debug.Log($"[FilesController] userData is {(ve.userData == null ? "null" : ve.userData.GetType().Name)}");

                    if (ve.userData is T item)
                    {
                        var fe = (IFileEntry)item;
                        // Debug.Log($"[FilesController] Invoking onClickAction for item: Name='{fe.Name}', Size={fe.Size}, Path='{fe.Path}'");
                        onClickAction(item);
                        evt.StopPropagation();
                    }
                    else
                    {
                        Debug.LogWarning("[FilesController] userData is not of type T; click ignored");
                    }
                });

                return ve;
            };


            listView.bindItem = (listItemVisualElement, i) =>
            {
                T entry = fileList[i];

                // Keep current item on the row for the click handler (and log binding)
                listItemVisualElement.userData = entry;
                // var feBound = (IFileEntry)entry;
                // Debug.Log($"[FilesController] bindItem: index={i}, Name='{feBound.Name}', Size={feBound.Size}, Path='{feBound.Path}'");

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
                VisualElement listItemState = listItemVisualElement.Q<VisualElement>("State");
                if (entry is FileState fileState)
                {
                    bool state = fileState.state;
                    if (listItemState != null)
                    {
                        if (state)
                        {
                            listItemState.AddToClassList("active");
                        }
                        else
                        {
                            listItemState.RemoveFromClassList("active");
                        }
                    }
                }
                else
                {
                    listItemState?.RemoveFromClassList("active");
                }

                // Delete
                Button deleteButton = listItemVisualElement.Q<Button>("CloseButton");
                if (deleteButton != null)
                {
                    deleteButton.clickable = null;
                    T current = entry;
                    deleteButton.clickable = new Clickable(() =>
                    {
                        RemoveFile(current);
                        // PrintListView();
                    });
                }

            };

            listView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                onUpdateAction?.Invoke();
                listView.RefreshItems();
                // PrintListView();
            };
        }

        public void AddFile(T entry)
        {
            if (entry == null)
            {
                Debug.LogWarning("AddFile: null");
                return;
            }
            fileList.Add(entry);
            Refresh();
        }

        public void RemoveFile(T entry)
        {
            if (fileList.Remove(entry))
            {
                Refresh();
            }
        }

        public List<T> GetFileList()
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
                    Debug.Log($"[{i}] FileInfo -> Name={fileInfo.name}, Size={fileInfo.size}, Path={fileInfo.path}");
                }
                else if (listView.itemsSource[i] is FileState fs)
                {
                    Debug.Log($"[{i}] FileState -> Name={fs.fileInfo.name}, Size={fs.fileInfo.size}, Path={fs.fileInfo.path}, State={fs.state}");
                }
                else
                {
                    Debug.Log($"[{i}] {listView.itemsSource[i]}");
                }
            }
        }

    }

}
