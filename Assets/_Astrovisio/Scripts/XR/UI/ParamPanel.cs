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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class ParamPanel : MonoBehaviour
    {

        [SerializeField] private GameObject scrollViewGO;
        [SerializeField] private GameObject settingPanelGO;
        [SerializeField] private TextMeshProUGUI panelTitleTMP;

        private List<ParamButton> paramButtons;


        private void Start()
        {
            paramButtons = scrollViewGO.GetComponentsInChildren<ParamButton>().ToList();
            panelTitleTMP.text = "";

            foreach (ParamButton paramButton in paramButtons)
            {
                paramButton.SetButtonState(false);
                paramButton.OnParamButtonClicked += OnButtonClicked;
            }

            settingPanelGO.SetActive(false);
        }

        private void OnButtonClicked(ParamButton button)
        {
            bool isActive = button.State;

            if (isActive)
            {
                settingPanelGO.SetActive(true);
                ResetAllButton(button);
                // panelTitleTMP.text = button.settings.Name;
            }
            else
            {
                settingPanelGO.SetActive(false);
            }
        }

        private void ResetAllButton(ParamButton paramButtonToIgnore = null)
        {
            foreach (ParamButton paramButton in paramButtons)
            {
                if (paramButton != paramButtonToIgnore)
                {
                    paramButton.SetButtonState(false);
                }
            }
        }


    }

}
