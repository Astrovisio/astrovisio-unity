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
    public class LoadingController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float spinnerSpeed = 180f;

        private VisualElement spinner;
        private VisualElement bar;
        private float rotationAngle = 0f;

        private void Start()
        {
            var uiDocument = GetComponentInParent<UIDocument>();
            var loaderView = uiDocument.rootVisualElement.Q<VisualElement>("LoaderView");
            spinner = loaderView.Q<VisualElement>("LoadingSpinner");
            bar = loaderView.Q<VisualElement>("LoadingBar");

            // SetLoaderValue(0.75f);
        }

        private void Update()
        {
            if (spinner == null)
            {
                return;
            }

            rotationAngle += spinnerSpeed * Time.deltaTime;
            rotationAngle %= 360f;
            spinner.style.rotate = new Rotate(new Angle(rotationAngle, AngleUnit.Degree));
        }

        public void SetSpinnerVisibility(bool visibility)
        {
            if (visibility)
            {
                spinner.style.display = DisplayStyle.Flex;
            }
            else
            {
                spinner.style.display = DisplayStyle.None;
            }
        }

        public void SetBarVisibility(bool visibility)
        {
            if (visibility)
            {
                bar.style.display = DisplayStyle.Flex;
            }
            else
            {
                bar.style.display = DisplayStyle.None;
            }
        }

        public void SetLoaderValue(float value)
        {
            VisualElement barFill = bar.Q<VisualElement>("BarFill");

            Vector3 scale = barFill.transform.scale;
            scale.x = value;
            barFill.transform.scale = scale;
        }

    }

}
