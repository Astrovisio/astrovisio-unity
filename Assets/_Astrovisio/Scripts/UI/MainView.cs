using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class MainView : MonoBehaviour
    {

        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private UIDocument mainViewUIDocument;

        [SerializeField] private GameObject newProjectView;

        private Button newProjectButton;


        private void Start()
        {
            Debug.Log("NewProjectPanel");
        }

        private void OnEnable()
        {
            VisualElement root = mainViewUIDocument.rootVisualElement;


            VisualElement newProjectButtonInstance = root.Q<VisualElement>("NewProjectButton");
            newProjectButton = newProjectButtonInstance?.Q<Button>();

            if (newProjectButton != null)
            {
                Debug.Log("Button trovato");
                newProjectButton.RegisterCallback<ClickEvent>(OnNewProjectClicked);
            }
            else
            {
                Debug.LogWarning("Button NON trovato all'interno di NewProjectButton");
            }

        }

        private void OnDisable()
        {
            if (newProjectButton != null)
            {
                newProjectButton.UnregisterCallback<ClickEvent>(OnNewProjectClicked);
            }
        }

        private void OnNewProjectClicked(ClickEvent evt)
        {
            Debug.Log("OnNewProjectClicked");
            newProjectView.SetActive(true);
        }


    }

}
