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

using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    // Deprecated ?
    public class DataInspectorController
    {
        public VisualElement Root { get; }

        private ScrollView paramScrollView;

        public DataInspectorController(VisualElement root)
        {
            Root = root;

            paramScrollView = Root.Q<ScrollView>("ParamScrollView");
            // Debug.Log(paramScrollView);

            SetVisibility(false);
        }

        public void SetVisibility(bool visibility)
        {
            if (visibility)
            {
                Root.style.display = DisplayStyle.Flex;
            }
            else
            {
                Root.style.display = DisplayStyle.None;
            }
        }

        public void SetData(string[] header, float[] dataInfo)
        {
            paramScrollView.Clear();

            for (int i = 0; i < dataInfo.Length; i++)
            {
                AddParamRow(header[i] + ": " + dataInfo[i]);
            }
        }

        private void AddParamRow(string text)
        {
            Label label = new Label();
            label.text = text;
            label.AddToClassList("param-row");
            paramScrollView.Add(label);
            // Debug.Log(text);
        }

    }

}