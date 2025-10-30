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

namespace Astrovisio
{
    public class XRScreenshotUIController : MonoBehaviour
    {
        [SerializeField] private Image loaderImage;
        [SerializeField] private TextMeshProUGUI labelTMP;

        private void Start()
        {
            SetLoaderImage(false, 0f);
        }

        public void SetLoaderImage(bool state, float value)
        {
            loaderImage.gameObject.SetActive(state);
            loaderImage.fillAmount = value;
        }

        public void SetLabel(string text)
        {
            labelTMP.text = text;
        }

    }

}
