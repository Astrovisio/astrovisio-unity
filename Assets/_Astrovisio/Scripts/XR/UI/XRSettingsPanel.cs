using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Astrovisio
{
    public class XRSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Button xAxisButton;
        [SerializeField] private Button yAxisButton;
        [SerializeField] private Button zAxisButton;
        [SerializeField] private ScrollRect paramScrollRect;
        [SerializeField] private ParamButton paramButton;
        [SerializeField] private XRSettingPanel xrSettingPanel;

        [ContextMenu("UpdateUI")]
        public void UpdateUI()
        {
            ClearScrollView();

            Project project = RenderManager.Instance.renderedProject;
            File file = RenderManager.Instance.renderedFile;
            if (project == null || file == null || paramScrollRect == null || paramScrollRect.content == null || paramButton == null)
            {
                return;
            }

            if (!SettingsManager.Instance.TryGetSettings(project.Id, file.Id, out var settings) || settings?.Variables == null)
            {
                return;
            }

            foreach (Setting setting in settings.Variables)
            {
                ParamButton paramButton = Instantiate(this.paramButton, paramScrollRect.content);
                paramButton.gameObject.name = $"{setting.Name}";
                paramButton.gameObject.SetActive(true);

                paramButton.InitButtonSetting(
                    setting.Name,
                    setting,
                    () =>
                    {
                        Debug.Log(setting.Name);
                        xrSettingPanel.UpdateUI(setting);
                    }
                );
            }
        }

        private void ClearScrollView()
        {
            RectTransform content = paramScrollRect?.content;
            if (content == null) return;

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

    }

}
