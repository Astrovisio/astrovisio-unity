using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class NoiseSettingController
    {
        public VisualElement Root { get; }

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
            noiseSlider = Root.Q<VisualElement>("NoiseSlider").Q<MinMaxSlider>();
            noiseFloatField = Root.Q<FloatField>("NoiseInputField");

            noiseFloatField.formatString = "F3";

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

                SetNoise(newValue);

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

                SetNoise(newValue);

                isUpdating = false;
            });
        }

        public bool GetState()
        {
            return noiseState;
        }

        private void SetNoise(float value)
        {
            RenderManager.Instance.SetNoise(value);
        }

        public void Reset()
        {
            noiseSlider.maxValue = noiseMinValue;
            noiseFloatField.value = noiseMinValue;
            // noiseSlider.SetEnabled(noiseState);
            // noiseFloatField.SetEnabled(noiseState);
        }

    }
    
}
