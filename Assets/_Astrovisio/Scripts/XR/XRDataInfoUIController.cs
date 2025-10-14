using System;
using System.Linq;
using System.Text;
using CatalogData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRDataInfoUIController : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI textMeshProUGUI;
        // [SerializeField] private Button closeButton;

        private DataRenderer dataRenderer;
        private AstrovisioDataSetRenderer atrovidioDataSetRenderer;
        private KDTreeComponent kdTreeComponent;

        private void OnEnable()
        {
            if (RenderManager.Instance.isInspectorModeActive)
            {
                UpdateDataInfoPanel();
            }
        }

        private void Update()
        {
            dataRenderer = RenderManager.Instance.DataRenderer;
            if (dataRenderer != null)
            {
                atrovidioDataSetRenderer = dataRenderer.GetAstrovidioDataSetRenderer();
                kdTreeComponent = dataRenderer.GetKDTreeComponent();

                if (kdTreeComponent.GetLastNearest() == null || dataRenderer.GetDataContainer() == null)
                {
                    return;
                }

                UpdateDataInfoPanel();
            }
        }

        private void UpdateDataInfoPanel()
        {
            if (dataRenderer == null || dataRenderer.GetDataContainer() == null)
            {
                return;
            }

            string[] dataHeader = dataRenderer.GetDataContainer().DataPack.Columns;
            float[] dataInfo = atrovidioDataSetRenderer.GetDataInfo();

            int headerLength = dataHeader.Length;
            int infoLength = dataInfo.Length;

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < infoLength; i++)
            {
                if (i < headerLength)
                {
                    builder.AppendLine($"{dataHeader[i]}: {dataInfo[i]}");
                }
                else
                {
                    builder.AppendLine(dataInfo[i].ToString());
                }
            }

            SetText(builder.ToString());
        }

        private void SetText(string text)
        {
            textMeshProUGUI.text = text;
        }

        // private void OnEnable()
        // {
        //     if (closeButton != null)
        //     {
        //         closeButton.onClick.AddListener(OnCloseButtonClicked);
        //     }
        // }

        // private void OnDisable()
        // {
        //     if (closeButton != null)
        //     {
        //         closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        //     }
        // }

        // private void OnCloseButtonClicked()
        // {
        //     SetPanelVisibility(false);
        // }

        // public bool GetPanelVisibility()
        // {
        //     return gameObject.activeSelf;
        // }

        // public void TogglePanelVisibility()
        // {
        //     bool newVisibility = !gameObject.activeSelf;
        //     gameObject.SetActive(newVisibility);
        //     // Debug.Log($"[MenuPanelUI] Panel visibility toggled to: {newVisibility}");
        // }

        // public void SetPanelVisibility(bool isVisible)
        // {
        //     if (gameObject.activeSelf != isVisible)
        //     {
        //         gameObject.SetActive(isVisible);
        //         // Debug.Log($"[MenuPanelUI] Panel visibility set to: {isVisible}");
        //     }
        // }

    }

}
