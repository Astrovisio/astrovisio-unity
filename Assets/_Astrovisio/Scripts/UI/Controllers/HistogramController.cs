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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class HistogramController
    {
        public VisualElement Root { get; }
        public List<BinHistogram> BinHistogramList { get; private set; }

        private VisualElement histogramGraphicBackground;
        private VisualElement histogramGraphic;
        private int maxCount;

        public HistogramController(VisualElement root, List<BinHistogram> binHistogramList)
        {
            Root = root;
            BinHistogramList = binHistogramList ?? new List<BinHistogram>();
            maxCount = GetMax(BinHistogramList);
            InitFromBins();
        }

        private void InitFromBins()
        {
            histogramGraphicBackground = Root.Q<VisualElement>("HistogramGraphicBg");
            histogramGraphic = Root.Q<VisualElement>("HistogramGraphic");

            if (histogramGraphic == null)
            {
                Debug.LogError("HistogramGraphic not found.");
                return;
            }

            int targetBars = BinHistogramList != null ? BinHistogramList.Count : 0;
            if (targetBars <= 0)
            {
                ClearBars();
                return;
            }

            EnsureBars(targetBars);

            float containerHeight = 26f;
            if (histogramGraphicBackground != null)
            {
                float resolved = histogramGraphicBackground.resolvedStyle.height;
                if (!float.IsNaN(resolved) && resolved > 0f)
                {
                    containerHeight = resolved;
                }
            }

            int maxValue = maxCount > 0 ? maxCount : 1;

            for (int i = 0; i < targetBars; i++)
            {
                VisualElement bar = histogramGraphic.ElementAt(i);
                int v = Mathf.Max(0, BinHistogramList[i].Count);
                float h = (float)v / (float)maxValue * containerHeight;
                bar.style.height = h;
            }
        }

        public void UpdateFromBins(List<BinHistogram> binHistogramList)
        {
            BinHistogramList = binHistogramList ?? new List<BinHistogram>();
            maxCount = GetMax(BinHistogramList);
            InitFromBins();
        }

        private void EnsureBars(int target)
        {
            int current = histogramGraphic.childCount;

            if (current < target)
            {
                int toAdd = target - current;
                for (int i = 0; i < toAdd; i++)
                {
                    VisualElement bar = new VisualElement();
                    bar.AddToClassList("histogram-bar");
                    histogramGraphic.Add(bar);
                }
            }
            else if (current > target)
            {
                int toRemove = current - target;
                for (int i = 0; i < toRemove; i++)
                {
                    VisualElement child = histogramGraphic.ElementAt(target);
                    child.RemoveFromHierarchy();
                }
            }
        }

        private void ClearBars()
        {
            if (histogramGraphic == null) return;

            int current = histogramGraphic.childCount;
            for (int i = current - 1; i >= 0; i--)
            {
                VisualElement child = histogramGraphic.ElementAt(i);
                child.RemoveFromHierarchy();
            }
        }

        public int GetMax(List<BinHistogram> binHistogramList)
        {
            int result = 0;
            if (binHistogramList == null || binHistogramList.Count == 0) return result;

            for (int i = 0; i < binHistogramList.Count; i++)
            {
                int c = binHistogramList[i].Count;
                if (c > result) result = c;
            }

            return result;
        }
    }
}
