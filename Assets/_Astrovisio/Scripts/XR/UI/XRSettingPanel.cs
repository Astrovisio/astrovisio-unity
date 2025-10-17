using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Astrovisio
{
    public class XRSettingPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleTMP;
        [SerializeField] private Button yAxisButton;
        [SerializeField] private Button zAxisButton;

        public void UpdateUI(Setting setting)
        {
            titleTMP.text = setting.Name;
        }

    }
    
}
