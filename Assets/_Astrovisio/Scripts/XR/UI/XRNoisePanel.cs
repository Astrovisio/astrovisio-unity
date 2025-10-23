using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRNoisePanel : MonoBehaviour
    {

        [SerializeField] private Button closeButton;
        [SerializeField] private Slider noiseSlider;
        [SerializeField] private TextMeshProUGUI noiseTMP;

        private void Start()
        {
            closeButton.onClick.AddListener(HandleCloseButton);

            noiseSlider.minValue = 0f;
            noiseSlider.maxValue = 0.1f;

            noiseSlider.onValueChanged.AddListener(HandleNoiseSliderChange);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(HandleCloseButton);
            noiseSlider.onValueChanged.RemoveListener(HandleNoiseSliderChange);
        }

        private void HandleCloseButton()
        {
            Destroy(transform.parent.parent.gameObject, 0.1f);
        }

        [ContextMenu("Update")]
        public void UpdateUI()
        {
            // noiseToggle.isOn = RenderManager.Instance.GetNoiseState();
            noiseSlider.value = RenderManager.Instance.GetNoise();
            noiseTMP.text = $"{noiseSlider.value:F3}%";
        }

        private void HandleNoiseSliderChange(float newValue)
        {
            RenderManager.Instance.SetNoise(newValue);
        }

    }

}
