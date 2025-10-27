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
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class HistogramController
    {
        // === Dependencies ===
        public VisualElement Root { get; }

        // === Local ===
        private VisualElement histogramGraphicBackground;
        private VisualElement histogramGraphic;

        public HistogramController(VisualElement root, int[] values = null)
        {
            Root = root;

            Init(values);
        }

        private void Init(int[] values)
        {
            histogramGraphicBackground = Root.Q<VisualElement>("HistogramGraphicBg");
            histogramGraphic = Root.Q<VisualElement>("HistogramGraphic");

            if (histogramGraphic == null)
            {
                Debug.LogError("HistogramGraphic not found.");
                return;
            }

            int childCount = histogramGraphic.childCount;
            if (childCount <= 0)
            {
                Debug.LogWarning("HistogramGraphic has no children to update.");
                return;
            }

            if (values == null || values.Length != childCount)
            {
                // values = GenerateGaussianLikeArray(childCount, 100);
                // values = GenerateRandomArray(childCount, 100);
                values = GeneratePerlinNoiseArray(childCount, 100);
            }

            int len = Mathf.Min(childCount, values.Length);

            float containerHeight = 26f;
            int maxValue = values.Max();

            for (int i = 0; i < len; i++)
            {
                VisualElement bar = histogramGraphic.ElementAt(i);
                int v = Mathf.Max(0, values[i]);
                float h = (maxValue > 0) ? v / (float)maxValue * containerHeight : 0f;
                bar.style.height = h;
            }
        }

        private int[] GenerateGaussianLikeArray(int length, int peak)
        {
            var result = new int[length];
            if (length <= 0) return result;

            double mid = (length - 1) / 2.0;
            double sigma = Math.Max(1.0, length / 6.0);

            for (int i = 0; i < length; i++)
            {
                double x = (i - mid) / sigma;
                double g = Math.Exp(-0.5 * x * x);
                result[i] = Math.Max(1, (int)(g * peak));
            }
            return result;
        }

        private int[] GenerateRandomArray(int length, int peak)
        {
            var result = new int[length];
            if (length <= 0) return result;

            for (int i = 0; i < length; i++)
            {
                // genera un numero intero tra 1 e peak (inclusi)
                result[i] = UnityEngine.Random.Range(1, peak + 1);
            }

            return result;
        }

        private int[] GeneratePerlinNoiseArray(int length, int peak, float scale = 0.1f)
        {
            var result = new int[length];
            if (length <= 0) return result;

            float offsetX = UnityEngine.Random.Range(0f, 10000f);
            float offsetY = UnityEngine.Random.Range(0f, 10000f);

            for (int i = 0; i < length; i++)
            {
                float noise = Mathf.PerlinNoise(i * scale + offsetX, offsetY);
                int value = Mathf.Clamp(Mathf.RoundToInt(noise * peak), 1, peak);
                result[i] = value;
            }

            return result;
        }

    }

}
