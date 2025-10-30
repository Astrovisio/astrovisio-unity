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

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class MainViewController
    {
        private VisualElement mainViewRoot;
        private VisualElement body;
        private VisualElement header;
        private VisualElement side;
        private VisualElement content;

        public MainViewController(VisualElement root)
        {
            mainViewRoot = root.Q<VisualElement>("MainViewRoot");
            header = root.Q<VisualElement>("Header");
            body = root.Q<VisualElement>("Body");
            side = body.Q<VisualElement>("Side");
            content = body.Q<VisualElement>("Content");

            root.pickingMode = PickingMode.Ignore;
            mainViewRoot.pickingMode = PickingMode.Ignore;
            header.pickingMode = PickingMode.Position;
            body.pickingMode = PickingMode.Ignore;
        }

        public void SetBackgroundVisibility(bool state)
        {
            // Debug.Log("SetBackgroundVisibility");
            if (state)
            {
                mainViewRoot.AddToClassList("active");
            }
            else
            {
                mainViewRoot.RemoveFromClassList("active");
            }
        }

        public void SetHeaderVisibility(bool state)
        {
            // Debug.Log("SetHeaderVisibility");
            if (state)
            {
                header.style.visibility = Visibility.Visible;
            }
            else
            {
                header.style.visibility = Visibility.Hidden;
            }
        }

        public void SetSideVisibility(bool state)
        {
            // Debug.Log("SetSideVisibility");
            if (state)
            {
                side.style.visibility = Visibility.Visible;
            }
            else
            {
                side.style.visibility = Visibility.Hidden;
            }
        }

        public void SetContentVisibility(bool state)
        {
            // Debug.Log("SetContentVisibility");
            if (state)
            {
                content.style.visibility = Visibility.Visible;
            }
            else
            {
                content.style.visibility = Visibility.Hidden;
            }
        }

    }

}