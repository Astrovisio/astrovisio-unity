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
        [SerializeField] private Button closeButton;

        private void Update()
        {
            DataRenderer dataRenderer = RenderManager.Instance.GetCurrentDataRenderer();
            AstrovisioDataSetRenderer atrovidioDataSetRenderer = dataRenderer.GetAstrovidioDataSetRenderer();

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


        private void OnEnable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }

        private void OnCloseButtonClicked()
        {
            SetPanelVisibility(false);
        }

        private void SetText(string text)
        {
            textMeshProUGUI.text = text;
        }

        public bool GetPanelVisibility()
        {
            return gameObject.activeSelf;
        }

        public void TogglePanelVisibility()
        {
            bool newVisibility = !gameObject.activeSelf;
            gameObject.SetActive(newVisibility);
            // Debug.Log($"[MenuPanelUI] Panel visibility toggled to: {newVisibility}");
        }

        public void SetPanelVisibility(bool isVisible)
        {
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
                // Debug.Log($"[MenuPanelUI] Panel visibility set to: {isVisible}");
            }
        }

    }

}
