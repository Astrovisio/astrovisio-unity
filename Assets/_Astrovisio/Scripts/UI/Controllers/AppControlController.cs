/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
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
    public class AppControlController
    {
        public VisualElement Root { get; }

        private Button fullscreenButton;
        private Button closeButton;

        public AppControlController(VisualElement root)
        {
            Root = root;

            fullscreenButton = root.Q<Button>("FullscreenButton");
            closeButton = root.Q<Button>("CloseButton");

            if (fullscreenButton != null)
            {
                fullscreenButton.clicked += OnFullscreenClicked;
            }

            if (closeButton != null)
            {
                closeButton.clicked += OnCloseClicked;
            }
        }

        private void OnFullscreenClicked()
        {
            // Debug.Log("HandleFullscreenButton");
#if UNITY_EDITOR
            Debug.Log("Fullscreen toggle is only active in build.");
#else
            Screen.fullScreen = !Screen.fullScreen;
            if (fullscreenButton != null)
            {
                if (Screen.fullScreen)
                {
                    fullscreenButton.RemoveFromClassList("active");
                }
                else
                {
                    fullscreenButton.AddToClassList("active");
                }
            }
#endif
        }

        private void OnCloseClicked()
        {
            // Debug.Log("HandleCloseButton");
            UIManager.Instance.SetCloseViewVisibility(true);
        }

        public void Dispose()
        {
            fullscreenButton.clicked -= OnFullscreenClicked;
            closeButton.clicked -= OnCloseClicked;
        }

    }
}
