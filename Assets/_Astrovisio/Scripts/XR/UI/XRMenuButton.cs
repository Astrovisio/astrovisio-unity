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

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Astrovisio.XR
{
    [RequireComponent(typeof(Button))]
    public class XRMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI infoTMP;
        [SerializeField] private GameObject panel;
        [SerializeField] private string infoText;

        private Button button;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            infoTMP.text = infoText;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            infoTMP.text = "";
        }

        private void OnButtonClick()
        {
            // TODO: Search panel, if not in scene, instantiate panel
            
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

    }

}
