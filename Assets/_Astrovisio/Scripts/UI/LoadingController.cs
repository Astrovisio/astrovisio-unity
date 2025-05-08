using UnityEngine;
using UnityEngine.UIElements;

public class LoadingController : MonoBehaviour
{
    [Header("UI Templates")]
    [SerializeField] private VisualTreeAsset loadingSpinnerTemplate;
    [SerializeField] private float spinnerSpeed = 180f; // Gradi per secondo

    private VisualElement dotSpinner;
    private VisualElement spinnerContainer;
    private UIDocument uiDocument;

    // private void Awake()
    // {
    //     uiDocument = GetComponent<UIDocument>();
    //     if (uiDocument == null)
    //     {
    //         Debug.LogError("UIDocument non trovato!");
    //     }
    // }

    // private void OnEnable()
    // {
    //     if (loadingSpinnerTemplate == null || uiDocument == null)
    //     {
    //         return;
    //     }

    //     // Clona lo spinner e aggiungilo alla root UI
    //     spinnerContainer = loadingSpinnerTemplate.CloneTree();
    //     // uiDocument.rootVisualElement.Add(spinnerContainer);

    //     // Recupera il riferimento al dot
    //     dotSpinner = spinnerContainer.Q<VisualElement>("Dot");
    //     if (dotSpinner == null)
    //     {
    //         Debug.LogError("Elemento 'Dot' non trovato nel template");
    //     }
    // }

    // private void Update()
    // {
    //     if (dotSpinner != null)
    //     {
    //         dotSpinner.transform.rotation *= Quaternion.Euler(0, 0, -spinnerSpeed * Time.deltaTime);
    //     }
    // }

    // /// <summary>
    // /// Attiva o disattiva lo spinner
    // /// </summary>
    // public void SetSpinner(bool active)
    // {
    //     if (spinnerContainer != null)
    //     {
    //         spinnerContainer.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
    //     }
    // }
}
