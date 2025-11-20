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

    public class CloseViewController
    {
        public VisualElement Root { get; }

        private Button exitButton;
        private Button cancelButton;

        public CloseViewController(VisualElement root)
        {
            Root = root;

            exitButton = root.Q<VisualElement>("ExitButton").Q<Button>();
            cancelButton = root.Q<VisualElement>("CancelButton").Q<Button>();

            if (exitButton != null)
            {
                exitButton.clicked += OnExitClicked;
            }

            if (cancelButton != null)
            {
                cancelButton.clicked += OnCancelClicked;
            }
        }

        public void Open()
        {
            Root.AddToClassList("active");
        }

        public void Close()
        {
            Root.RemoveFromClassList("active");
        }

        private void OnExitClicked()
        {
            // Debug.Log("OnExitClicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnCancelClicked()
        {
            // Debug.Log("OnCancelClicked");
            Root.RemoveFromClassList("active");
        }

    }

}