using System;
using Astrovisio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReelPanelHandler : MonoBehaviour
{
    [SerializeField] private ProjectManager projectManager;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private void Awake()
    {
        if (prevButton == null)
            Debug.LogError("[ReelPanelHandler] Prev button is not assigned.");
        if (nextButton == null)
            Debug.LogError("[ReelPanelHandler] Next button is not assigned.");
        if (label == null)
            Debug.LogError("[ReelPanelHandler] Label is not assigned.");
    }

    private void Start()
    {
        if (prevButton != null)
            prevButton.onClick.AddListener(OnPrevClicked);
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
    }

    private void OnDestroy()
    {
        if (prevButton != null)
            prevButton.onClick.RemoveListener(OnPrevClicked);
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextClicked);
    }

    private void OnPrevClicked()
    {
        RenderManager.Instance.RenderReelPrev(3);
    }

    private void OnNextClicked()
    {
        RenderManager.Instance.RenderReelNext(3);
        int? currentFileId = RenderManager.Instance.GetReelCurrentFileId(3);

        if (currentFileId != null)
        {
            File file = projectManager.GetFile(3, currentFileId.Value);
            SetLabel(file.Name);
        }
    }

    public void SetLabel(string text)
    {
        if (label != null)
            label.text = text;
    }
}
