using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class NoiseSettingController
    {
        public VisualElement Root { get; }

        private Toggle noiseToggle;
        private MinMaxSlider noiseSlider;
        private FloatField noiseFloatField;

        private bool noiseState = false;
        private float noiseMinValue = 0.0f;
        private float noiseMaxValue = 0.1f;
        private float noiseValue = 0.0f;

        public NoiseSettingController(VisualElement root)
        {
            Root = root;
            Init();
        }

        private void Init()
        {
            noiseToggle = Root.Q<VisualElement>("NoiseToggle").Q<Toggle>();
            noiseSlider = Root.Q<VisualElement>("NoiseSlider").Q<MinMaxSlider>();
            noiseFloatField = Root.Q<FloatField>("NoiseInputField");

            noiseSlider.SetEnabled(noiseState);
            noiseFloatField.SetEnabled(noiseState);
            noiseFloatField.formatString = "F3";

            // Debug.Log(noiseToggle);
            // Debug.Log(noiseSlider);
            // Debug.Log(noiseFloatField);

            // Toggle
            noiseToggle.value = noiseState;

            noiseToggle.RegisterValueChangedCallback(evt =>
            {
                noiseState = evt.newValue;

                noiseSlider.SetEnabled(noiseState);
                noiseFloatField.SetEnabled(noiseState);

                SetNoise(noiseState, noiseValue);
            });


            bool isUpdating = false;

            // Slider
            noiseSlider[0].pickingMode = PickingMode.Ignore;
            noiseSlider[0][0].pickingMode = PickingMode.Ignore;
            noiseSlider[0][1].pickingMode = PickingMode.Ignore;
            noiseSlider.lowLimit = noiseMinValue;
            noiseSlider.highLimit = noiseMaxValue;
            noiseSlider.minValue = noiseMinValue;
            noiseSlider.maxValue = noiseValue;

            noiseSlider.RegisterValueChangedCallback(evt =>
            {
                if (isUpdating)
                {
                    return;
                }
                isUpdating = true;

                float newValue = Mathf.Clamp(noiseSlider.maxValue, noiseMinValue, noiseMaxValue);
                noiseValue = newValue;
                noiseFloatField.value = newValue;

                SetNoise(noiseToggle.value, newValue);

                isUpdating = false;
            });

            // FloatField
            noiseFloatField.value = noiseValue;

            noiseFloatField.RegisterValueChangedCallback(evt =>
            {
                if (isUpdating)
                {
                    return;
                }
                isUpdating = true;

                float newValue = Mathf.Clamp(evt.newValue, noiseMinValue, noiseMaxValue);
                noiseValue = newValue;
                noiseSlider.maxValue = newValue;

                SetNoise(noiseToggle.value, newValue);

                isUpdating = false;
            });
        }

        private void SetNoise(bool state, float value)
        {
            RenderManager.Instance.SetNoise(state, value);
        }

        public void Reset()
        {
            noiseToggle.value = false;
            noiseSlider.maxValue = noiseMinValue;
            noiseFloatField.value = noiseMinValue;
            // noiseSlider.SetEnabled(noiseState);
            // noiseFloatField.SetEnabled(noiseState);
        }

    }
    
}
