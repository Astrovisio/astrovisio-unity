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

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class AboutViewController
    {

        public VisualElement Root { get; }
        public UIManager UIManager { get; }

        private Button closeButton;
        private Label title;

        public AboutViewController(VisualElement root, UIManager uiManager)
        {
            Root = root;
            UIManager = uiManager;

            closeButton = Root.Q<Button>("CloseButton");
            closeButton.clicked += Close;

            title = Root.Q<Label>("Title");
            title.text = "Astrovisio v " + Application.version;

            Init();
        }

        public void Init()
        {
            Label idavieLabel = Root.Q<Label>("iDaVIELabel");

            VisualElement alkemyLogo = Root.Q<VisualElement>("AlkemyLogo");
            VisualElement dgiLogo = Root.Q<VisualElement>("DGILogo");
            VisualElement metaversoLogo = Root.Q<VisualElement>("MetaversoLogo");
            VisualElement scuolaNormaleLogo = Root.Q<VisualElement>("ScuolaNormaleLogo");
            VisualElement idiaLogo = Root.Q<VisualElement>("IDIALogo");
            VisualElement vislabLogo = Root.Q<VisualElement>("VislabLogo");
            VisualElement inafLogo = Root.Q<VisualElement>("INAFLogo");

            AddClickUrl(idavieLabel, "https://github.com/idia-astro/iDaVIE/");
            AddClickUrl(alkemyLogo, "https://www.alkemy.com/");
            AddClickUrl(dgiLogo, "https://www.designgroupitalia.com/");
            AddClickUrl(metaversoLogo, "https://www.metaverso.it/");
            AddClickUrl(scuolaNormaleLogo, "http://cosmology.sns.it/");
            AddClickUrl(idiaLogo, "https://idia.ac.za/");
            AddClickUrl(vislabLogo, "https://vislab.idia.ac.za/");
            AddClickUrl(inafLogo, "http://www.inaf.it/");
        }

        private void AddClickUrl(VisualElement element, string url)
        {
            element.RegisterCallback<PointerEnterEvent>(evt =>
                UnityEngine.Cursor.SetCursor(UIManager.GetUIContext().linkCursor, new Vector2(8, 2), CursorMode.Auto)
            );

            element.RegisterCallback<PointerLeaveEvent>(evt =>
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto)
            );

            element.RegisterCallback<ClickEvent>(evt =>
                Application.OpenURL(url)
            );
        }

        public void Open()
        {
            Root.AddToClassList("active");
        }

        public void Close()
        {
            Root.RemoveFromClassList("active");
        }

    }

}