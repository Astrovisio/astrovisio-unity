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

            noiseSlider.minValue = 0f;
            noiseSlider.maxValue = 0.1f;

            closeButton.onClick.AddListener(HandleCloseButton);
            noiseSlider.onValueChanged.AddListener(HandleNoiseSliderChange);
            UpdateUI();
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
            noiseSlider.value = RenderManager.Instance.GetNoise();
            noiseTMP.text = $"{noiseSlider.value:F3}%";
        }

        private void HandleNoiseSliderChange(float newValue)
        {
            RenderManager.Instance.SetNoise(newValue);
            UpdateUI();
        }

    }

}
