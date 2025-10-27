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
    public class ReadMoreViewController
    {
        public VisualElement Root { get; }
        public UIManager UIManager { get; }

        private Button closeButton;

        private Label titleLabel;
        private Label descriptionLabel;


        public ReadMoreViewController(VisualElement root, UIManager uiManager)
        {
            Root = root;
            UIManager = uiManager;

            closeButton = Root.Q<Button>("CloseButton");
            closeButton.clicked += Close;

            Init();
        }

        public void Init()
        {
            titleLabel = Root.Q<Label>("TitleLabel");
            descriptionLabel = Root.Q<Label>("DescriptionLabel");
        }

        public void Open(string title, string description)
        {
            Root.AddToClassList("active");
            titleLabel.text = title;
            descriptionLabel.text = description;
        }

        public void Close()
        {
            Root.RemoveFromClassList("active");
        }

    }

}
