using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public enum FilesContainerType
    {
        State,
        Info
    }

    public class FilesController<T> where T : IFileEntry
    {
        public VisualElement Root { get; }
        public UIContextSO UIContextSO { get; }

        private readonly FilesContainerType containerType;
        private readonly List<T> fileList = new();
        private readonly ListView listView;


        public FilesController(VisualElement root, UIContextSO ctx, FilesContainerType type = FilesContainerType.Info)
        {
            Root = root;
            UIContextSO = ctx;
            containerType = type;

            listView = Root.Q<ListView>();
            if (listView == null) { Debug.LogError("ListView not found."); return; }

            listView.itemsSource = fileList;
            listView.selectionType = SelectionType.None;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;

            listView.makeItem = () =>
                (containerType == FilesContainerType.State ?
                    UIContextSO.listItemFileStateTemplate :
                    UIContextSO.listItemFileTemplate).CloneTree();

            listView.bindItem = (listItemVisualElement, i) =>
            {
                T entry = fileList[i];

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

                VisualElement badge = listItemVisualElement.Q<VisualElement>("State");
                Button close = listItemVisualElement.Q<Button>("CloseButton");

                if (entry is FileState fs)
                {
                    bool state = fs.state;
                    if (badge != null)
                    {
                        if (state)
                        {
                            badge.AddToClassList("active");
                        }
                        else
                        {
                            badge.RemoveFromClassList("active");
                        }
                    }
                    if (close != null)
                    {
                        close.clicked -= () => { };
                        close.clicked += () => RemoveFile(fileList[i]);
                    }
                }
                else
                {
                    badge?.RemoveFromClassList("active");
                    if (close != null) close.SetEnabled(false);
                }
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

        // private void TestFileStateListView()
        // {
        //     FileState fileStateA = new FileState(new FileInfo("./FileA.fits", "FileA.fits", 1111L));
        //     FileState fileStateB = new FileState(new FileInfo("./FileB.fits", "FileB.fits", 2222L));
        //     FileState fileStateC = new FileState(new FileInfo("./FileC.fits", "FileC.fits", 3333L));
        //     FileState fileStateD = new FileState(new FileInfo("./FileD.fits", "FileD.fits", 4444L));
        //     FileState fileStateE = new FileState(new FileInfo("./FileE.fits", "FileE.fits", 5555L));
        //     FileState fileStateF = new FileState(new FileInfo("./FileF.fits", "FileF.fits", 6666L));

        //     fileList.Add(fileStateA);
        //     fileList.Add(fileStateB);
        //     fileList.Add(fileStateC);
        //     fileList.Add(fileStateD);
        //     fileList.Add(fileStateE);
        //     fileList.Add(fileStateF);

        //     listView = Root.Query<ListView>().First();
        //     listView.itemsSource = fileList;
        //     listView.reorderable = true;
        //     listView.reorderMode = ListViewReorderMode.Animated;
        //     listView.selectionType = SelectionType.None;


        //     listView.makeItem = () =>
        //     {
        //         return UIContextSO.listItemFileStateTemplate.CloneTree();
        //     };

        //     listView.bindItem = (ve, index) =>
        //     {
        //         FileState fileState = fileList[index];

        //         Label nameLabel = ve.Q<Label>("NameLabel");
        //         Label sizeLabel = ve.Q<Label>("SizeLabel");
        //         VisualElement state = ve.Q<VisualElement>("State");
        //         Button closeButton = ve.Q<Button>("CloseButton");

        //         if (nameLabel != null)
        //         {
        //             nameLabel.text = fileState.fileInfo.name;
        //         }

        //         if (sizeLabel != null)
        //         {
        //             sizeLabel.text = fileState.fileInfo.size.ToString();
        //         }

        //         if (state != null)
        //         {
        //             if (fileState.state)

        //             {
        //                 state.AddToClassList("active");
        //             }
        //             else
        //             {
        //                 state.RemoveFromClassList("active");
        //             }
        //         }

        //         if (closeButton != null)
        //         {
        //             closeButton.clickable = new Clickable(() => RemoveListViewItemState(fileState));
        //         }
        //     };

        //     listView.Rebuild();
        // }

    }

}
