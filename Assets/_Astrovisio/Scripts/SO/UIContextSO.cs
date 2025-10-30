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
    [CreateAssetMenu(fileName = "UIContext", menuName = "Astrovisio SO/UI Context")]
    public class UIContextSO : ScriptableObject
    {


        [Header("Header")]
        public VisualTreeAsset projectButtonTemplate;


        [Space(3)]
        [Header("Side")]

        [Space(1)]
        [Header("- Home")]
        public VisualTreeAsset favouriteProjectButton;
        public VisualTreeAsset projectSidebarTemplate;

        [Space(1)]
        [Header("- Project")]
        public VisualTreeAsset sidebarParamRowTemplate;
        public VisualTreeAsset paramRowSettingsTemplate;
        public ColorMapSO colorMapSO;


        [Header("Content")]

        [Space(1)]
        [Header("- Home")]
        public VisualTreeAsset projectRowHeaderTemplate;
        public VisualTreeAsset projectRowTemplate;

        [Space(1)]
        [Header("- Project")]
        public VisualTreeAsset projectViewTemplate;
        public VisualTreeAsset paramRowTemplate;
        public VisualTreeAsset listItemFileStateTemplate;

        [Space(1)]
        [Header("- New Project")]
        public VisualTreeAsset listItemFileTemplate;

        [Header("Cursor")]
        public Texture2D linkCursor;

    }
}
