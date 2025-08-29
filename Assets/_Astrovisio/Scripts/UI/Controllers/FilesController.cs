using System.Collections.Generic;
using Astrovisio;
using UnityEngine;
using UnityEngine.UIElements;


public class FilesController
{
    public VisualElement Root { get; }
    public UIContextSO UIContextSO { get; }


    private List<FileState> stateFiles = new List<FileState>();
    private ListView stateListView;


    public FilesController(VisualElement root, UIContextSO uiContextSO)
    {
        Root = root;
        UIContextSO = uiContextSO;

        TestFileStateListView();
    }

    private void TestFileStateListView()
    {
        FileState fileStateA = new FileState(new FileInfo("./FileA.fits", "FileA.fits", 1111L));
        FileState fileStateB = new FileState(new FileInfo("./FileB.fits", "FileB.fits", 2222L));
        FileState fileStateC = new FileState(new FileInfo("./FileC.fits", "FileC.fits", 3333L));
        FileState fileStateD = new FileState(new FileInfo("./FileD.fits", "FileD.fits", 4444L));
        FileState fileStateE = new FileState(new FileInfo("./FileE.fits", "FileE.fits", 5555L));
        FileState fileStateF = new FileState(new FileInfo("./FileF.fits", "FileF.fits", 6666L));

        stateFiles.Add(fileStateA);
        stateFiles.Add(fileStateB);
        stateFiles.Add(fileStateC);
        stateFiles.Add(fileStateD);
        stateFiles.Add(fileStateE);
        stateFiles.Add(fileStateF);

        stateListView = Root.Query<ListView>("FileStateListView").First();
        stateListView.itemsSource = stateFiles;
        stateListView.reorderable = true;
        stateListView.reorderMode = ListViewReorderMode.Animated;
        stateListView.selectionType = SelectionType.None;


        stateListView.makeItem = () =>
        {
            return UIContextSO.listItemFileStateTemplate.CloneTree();
        };

        stateListView.bindItem = (ve, index) =>
        {
            FileState fileState = stateFiles[index];

            Label nameLabel = ve.Q<Label>("NameLabel");
            Label sizeLabel = ve.Q<Label>("SizeLabel");
            VisualElement state = ve.Q<VisualElement>("State");
            Button closeButton = ve.Q<Button>("CloseButton");

            if (nameLabel != null)
            {
                nameLabel.text = fileState.fileInfo.name;
            }

            if (sizeLabel != null)
            {
                sizeLabel.text = fileState.fileInfo.size.ToString();
            }

            if (state != null)
            {
                // Aggiunge o rimuove la classe "active" a seconda del valore di data.State
                if (fileState.state)
                    state.AddToClassList("active");
                else
                    state.RemoveFromClassList("active");
            }

            if (closeButton != null)
            {
                closeButton.clickable = new Clickable(() => RemoveListViewItemState(fileState));
            }
        };

        stateListView.Rebuild();
    }

    private void RemoveListViewItemState(FileState data)
    {
        if (stateFiles.Contains(data))
        {
            stateFiles.Remove(data);
            stateListView.Rebuild(); // Oppure .Refresh() se vuoi mantenere l’ordine
            Debug.Log($"[ListView] Rimosso: {data.fileInfo.name}");
        }
    }

}
