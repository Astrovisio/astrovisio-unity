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
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRMenuUIController : MonoBehaviour
    {
        [SerializeField] private Button valuesButton;
        [SerializeField] private Button vrReelButton;
        [SerializeField] private Button helpButton;
        [SerializeField] private Button exitVRButton;
        [SerializeField] private GameObject visual;

        private void Start()
        {
            ClosePanel();

            if (valuesButton != null)
            {
                valuesButton.onClick.AddListener(OnValuesButtonClick);
            }
            else
            {
                Debug.LogWarning("Values Button non assegnato in MenuPanelUI!");
            }

            if (vrReelButton != null)
            {
                vrReelButton.onClick.AddListener(OnVrReelButtonClick);
            }
            else
            {
                Debug.LogWarning("VR Reel Button non assegnato in MenuPanelUI!");
            }

            if (helpButton != null)
            {
                helpButton.onClick.AddListener(OnHelpButtonClick);
            }
            else
            {
                Debug.LogWarning("Help Button non assegnato in MenuPanelUI!");
            }

            if (exitVRButton != null)
            {
                exitVRButton.onClick.AddListener(OnExitVRButtonClick);
            }
            else
            {
                Debug.LogWarning("Exit VR Button non assegnato in MenuPanelUI!");
            }
        }

        public void OpenPanel()
        {
            visual.SetActive(true);
        }

        public void ClosePanel()
        {
            visual.SetActive(false);
        }

        public void TogglePanel()
        {
            if (visual.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        private void OnValuesButtonClick()
        {
            // Debug.Log("OnValuesButtonClick");
        }

        private void OnVrReelButtonClick()
        {
            // Debug.Log("OnVrReelButtonClick");
        }

        private void OnHelpButtonClick()
        {
            // Debug.Log("OnHelpButtonClick");
        }

        private void OnExitVRButtonClick()
        {
            Debug.Log("OnExitVRButtonClick");
            XRManager.Instance.ExitVR();
            ClosePanel();
        }

        private void OnDestroy()
        {
            if (valuesButton != null)
            {
                valuesButton.onClick.RemoveListener(OnValuesButtonClick);
            }

            if (vrReelButton != null)
            {
                vrReelButton.onClick.RemoveListener(OnVrReelButtonClick);
            }

            if (helpButton != null)
            {
                helpButton.onClick.RemoveListener(OnHelpButtonClick);
            }

            if (exitVRButton != null)
            {
                exitVRButton.onClick.RemoveListener(OnExitVRButtonClick);
            }
        }

    }

}
