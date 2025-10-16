using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRNoisePanel : MonoBehaviour
    {

        [SerializeField] private Toggle noiseToggle;
        [SerializeField] private Slider noiseSlider;
        [SerializeField] private TextMeshProUGUI noiseTMP;

        private void Start()
        {
            noiseSlider.minValue = 0f;
            noiseSlider.maxValue = 0.1f;
        }

        [ContextMenu("Update")]
        public void UpdateUI()
        {
            noiseToggle.isOn = RenderManager.Instance.GetNoiseState();
            noiseSlider.value = RenderManager.Instance.GetNoiseValue();
            noiseTMP.text = $"{noiseSlider.value:F3}%";
        }

    }

}
