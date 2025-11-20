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
                fullscreenButton.clicked += HandleFullscreenButton;
            }

            if (closeButton != null)
            {
                closeButton.clicked += HandleCloseButton;
            }
        }

        private void HandleFullscreenButton()
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

        private void HandleCloseButton()
        {
            // Debug.Log("HandleCloseButton");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Dispose()
        {
            fullscreenButton.clicked -= HandleFullscreenButton;
            closeButton.clicked -= HandleCloseButton;
        }

    }
}
