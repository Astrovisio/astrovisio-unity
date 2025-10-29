/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class NoiseSettingController
    {
        public VisualElement Root { get; }

        // === Project  ===
        private Project project;
        private File file;
        private Settings settings;

        // === UI ===
        private MinMaxSlider noiseSlider;
        private FloatField noiseFloatField;

        // === Local ===
        private bool noiseState = false;
        private float noiseMinValue = 0.0f;
        private float noiseMaxValue = 0.1f;
        private float noiseValue = 0.0f;
        private bool isUpdating = false;

        public NoiseSettingController(VisualElement root)
        {
            Root = root;

            RenderManager.Instance.OnFileRenderEnd += OnFileRenderEnd;
            SettingsManager.Instance.OnSettingsAdd += OnSettingsAdd;

            Init();
        }

        private void OnFileRenderEnd(Project project, File file)
        {
            this.project = project;
            this.file = file;

            // Init();
        }

        private void OnSettingsAdd(ProjectFile projectFile)
        {
            SettingsManager.Instance.TryGetSettings(project.Id, file.Id, out settings);

            SetNoise(settings.Noise);
            noiseSlider.maxValue = settings.Noise;
            noiseFloatField.SetValueWithoutNotify(settings.Noise);
        }

        private void Init()
        {
            noiseSlider = Root.Q<VisualElement>("NoiseSlider").Q<MinMaxSlider>();
            noiseFloatField = Root.Q<FloatField>("NoiseInputField");
            noiseFloatField.formatString = "F3";

            // // Unregistering
            // noiseSlider.UnregisterValueChangedCallback(HandleOnSliderValueChanged);
            // noiseSlider.UnregisterCallback<PointerCaptureOutEvent>(HandleOnPointerUp);
            // noiseFloatField.UnregisterValueChangedCallback(HandleFloatFieldValueChanged);

            // Slider
            noiseSlider[0].pickingMode = PickingMode.Ignore;
            noiseSlider[0][0].pickingMode = PickingMode.Ignore;
            noiseSlider[0][1].pickingMode = PickingMode.Ignore;
            noiseSlider.lowLimit = noiseMinValue;
            noiseSlider.highLimit = noiseMaxValue;
            noiseSlider.minValue = noiseMinValue;
            noiseSlider.maxValue = noiseValue;
            noiseSlider.RegisterValueChangedCallback(HandleOnSliderValueChanged);
            noiseSlider.RegisterCallback<PointerCaptureOutEvent>(HandleOnPointerUp);

            // FloatField
            noiseFloatField.value = noiseValue;
            noiseFloatField.RegisterValueChangedCallback(HandleFloatFieldValueChanged);
        }

        private void HandleOnSliderValueChanged(ChangeEvent<Vector2> evt)
        {
            if (isUpdating)
            {
                return;
            }
            isUpdating = true;

            float newValue = Mathf.Clamp(noiseSlider.maxValue, noiseMinValue, noiseMaxValue);
            noiseValue = newValue;
            noiseFloatField.SetValueWithoutNotify(newValue);
            SetNoise(newValue);

            isUpdating = false;
        }

        private void HandleOnPointerUp(PointerCaptureOutEvent evt)
        {
            // Debug.Log("HandleOnPointerUp");
            UpdateSettings();
        }

        private void HandleFloatFieldValueChanged(ChangeEvent<float> evt)
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
            UpdateSettings();

            isUpdating = false;
        }

        private async void UpdateSettings()
        {
            if (SettingsManager.Instance.TryGetSettings(project.Id, file.Id, out settings))
            {
                settings.Noise = noiseValue;
                await SettingsManager.Instance.UpdateSettings(project.Id, file.Id, settings);
            }
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
        }

    }

}
