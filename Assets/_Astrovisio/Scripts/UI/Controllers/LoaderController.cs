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

using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public enum LoaderType
    {
        Spinner,
        Bar
    }

    public class LoaderController
    {
        public VisualElement Root { get; }
        private VisualElement spinner;

        private VisualElement loadingBarContainer;
        private MinMaxSlider loadingBar;
        private Label loadingBarMessage;

        public LoaderController(VisualElement root)
        {
            Root = root;

            VisualElement loaderView = root.Q<VisualElement>("LoaderView");

            spinner = loaderView.Q<VisualElement>("LoadingSpinner");

            loadingBarContainer = loaderView.Q<VisualElement>("LoadingBarContainer");
            loadingBarMessage = loadingBarContainer.Q<Label>("Message");
            loadingBar = loadingBarContainer.Q<VisualElement>("LoadingBar")?.Q<MinMaxSlider>("MinMaxSlider");
            loadingBar.lowLimit = 0f;
            loadingBar.highLimit = 1f;

            SetSpinnerVisibility(false);
            SetBarVisibility(false);
        }

        public void SetSpinnerVisibility(bool visibility)
        {
            if (visibility)
            {
                spinner.style.display = DisplayStyle.Flex;
            }
            else
            {
                spinner.style.display = DisplayStyle.None;
            }
        }

        public void SetBarVisibility(bool visibility)
        {
            if (visibility)
            {
                loadingBarContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                loadingBarContainer.style.display = DisplayStyle.None;
            }
        }

        public void SetBarProgress(float value, string text, bool visibility)
        {
            // Debug.Log("SetBarProgress");
            loadingBarMessage.text = text;
            loadingBar.maxValue = value;
            SetBarVisibility(visibility);
        }

    }

}
