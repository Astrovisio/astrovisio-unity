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
using System.Globalization;
using System.Threading.Tasks;
using CatalogData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRDataInspectorPanel : MonoBehaviour
    {
        [Header("Shape")]
        [SerializeField] private Button sphereButton;
        [SerializeField] private Image sphereIcon;
        [SerializeField] private TextMeshProUGUI sphereText;
        [SerializeField] private Button cubeButton;
        [SerializeField] private Image cubeIcon;
        [SerializeField] private TextMeshProUGUI cubeText;
        [SerializeField] private Color inactiveButtonColor;
        [SerializeField] private Color activeButtonColor;
        [SerializeField] private Color inactiveIconColor;
        [SerializeField] private Color activeIconColor;

        [Header("Size")]
        [SerializeField] private TextMeshProUGUI sizeLabel;
        [SerializeField] private Slider sizeSlider;
        [SerializeField] private TextMeshProUGUI sizeTMP;

        [Header("Aggregation")]
        [SerializeField] private TMP_Dropdown aggregationDropdown;

        [Header("Other")]
        [SerializeField] private Button processButton;
        [SerializeField] private Toggle isolateToggle;
        [SerializeField] private TextMeshProUGUI dataTMP;
        [SerializeField] private Button closeButton;

        // Local
        private SelectionMode selectionMode = SelectionMode.Sphere;
        private float defaultSelectionSize = 0.05f;
        private float selectionSize;
        private AggregationMode aggregationMode = AggregationMode.Average;
        private bool selectionState = true;
        private KDTreeComponent kDTreeComponent;


        private void Start()
        {
            closeButton.onClick.AddListener(HandleCloseButton);

            // Shape
            sphereButton.onClick.AddListener(() => SetSelectionMode(SelectionMode.Sphere));
            cubeButton.onClick.AddListener(() => SetSelectionMode(SelectionMode.Cube));

            // Size
            sizeSlider.minValue = 0.01f;
            sizeSlider.maxValue = 1f;
            sizeSlider.onValueChanged.AddListener(value =>
            {
                selectionSize = value;
                SetSelectionSize(value);
            });
            SetSelectionSize(defaultSelectionSize);

            // Aggregation
            aggregationDropdown.ClearOptions();
            aggregationDropdown.AddOptions(new List<string>(Enum.GetNames(typeof(AggregationMode))));
            aggregationDropdown.onValueChanged.AddListener(a => SetAggregationMode((AggregationMode)a));

            // Process button
            processButton.onClick.AddListener(OnProcessClicked);

            // Isolate toggle
            isolateToggle.onValueChanged.AddListener(SetIsolateToggle);

            // Data
            dataTMP.text = "";



            // ----
            DataRenderer dataRenderer = RenderManager.Instance.DataRenderer;
            kDTreeComponent = dataRenderer?.GetKDTreeComponent();

            if (kDTreeComponent == null)
            {
                return;
            }
            kDTreeComponent.showSelectionGizmo = true;

            kDTreeComponent.OnSelectionPerformed += OnSelectionPerformed;
        }

        private void OnDestroy()
        {
            if (kDTreeComponent == null)
            {
                return;
            }
            kDTreeComponent.showSelectionGizmo = false;

            closeButton.onClick.RemoveListener(HandleCloseButton);
            processButton.onClick.RemoveListener(OnProcessClicked);
            isolateToggle.onValueChanged.RemoveListener(SetIsolateToggle);
        }

        private void HandleCloseButton()
        {
            Destroy(transform.parent.parent.gameObject, 0.1f);
        }

        private void SetSelectionMode(SelectionMode selectionMode)
        {
            DataRenderer dataRenderer = RenderManager.Instance.DataRenderer;
            KDTreeComponent kDTreeComponent = dataRenderer?.GetKDTreeComponent();

            if (kDTreeComponent == null)
            {
                return;
            }

            switch (selectionMode)
            {
                case SelectionMode.Sphere:
                    SetButtonStyle(sphereButton, sphereIcon, sphereText, true);
                    SetButtonStyle(cubeButton, cubeIcon, cubeText, false);
                    this.selectionMode = SelectionMode.Sphere;
                    kDTreeComponent.selectionMode = SelectionMode.Sphere;
                    sizeLabel.text = "Diameter";
                    break;
                case SelectionMode.Cube:
                    SetButtonStyle(cubeButton, cubeIcon, cubeText, true);
                    SetButtonStyle(sphereButton, sphereIcon, sphereText, false);
                    this.selectionMode = SelectionMode.Cube;
                    kDTreeComponent.selectionMode = SelectionMode.Cube;
                    sizeLabel.text = "Side";
                    break;
            }

            SetSelectionSize(selectionSize);
        }

        private void SetButtonStyle(Button button, Image icon, TextMeshProUGUI label, bool active)
        {
            button.gameObject.GetComponent<Image>().color = active ? activeButtonColor : inactiveButtonColor;
            // button.GetComponentInChildren<Image>().color = active ? activeIconColor : inactiveIconColor;
            // button.GetComponentInChildren<TextMeshProUGUI>().color = active ? activeIconColor : inactiveIconColor;

            icon.color = active ? activeIconColor : inactiveIconColor;
            label.color = active ? activeIconColor : inactiveIconColor;
        }

        private void SetSelectionSize(float selectionSize)
        {
            DataRenderer dataRenderer = RenderManager.Instance.DataRenderer;
            KDTreeComponent kDTreeComponent = dataRenderer?.GetKDTreeComponent();

            if (kDTreeComponent == null)
            {
                return;
            }

            switch (selectionMode)
            {
                case SelectionMode.Sphere:
                    kDTreeComponent.selectionRadius = selectionSize;
                    break;
                case SelectionMode.Cube:
                    kDTreeComponent.selectionCubeHalfSize = selectionSize;
                    break;
            }

            sizeTMP.text = selectionSize.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void SetAggregationMode(AggregationMode aggregationMode)
        {
            DataRenderer dataRenderer = RenderManager.Instance.DataRenderer;
            KDTreeComponent kDTreeComponent = dataRenderer?.GetKDTreeComponent();

            if (kDTreeComponent == null)
            {
                return;
            }

            this.aggregationMode = aggregationMode;
            kDTreeComponent.aggregationMode = aggregationMode;
        }

        private async void OnProcessClicked()
        {
            KDTreeComponent kDTreeComponent = FindAnyObjectByType<KDTreeComponent>();
            if (kDTreeComponent is not null)
            {
                SelectionResult selectionResult = await kDTreeComponent.PerformSelection();
                SetInspectorInfo(selectionResult.AggregatedValues);
            }
        }

        private void SetInspectorInfo(float[] obj)
        {
            // Debug.Log("SetInspectorInfo " + obj[0] + obj[1]);

            DataRenderer dataRenderer = RenderManager.Instance.DataRenderer;

            if (dataRenderer == null)
            {
                return;
            }

            string[] headers = dataRenderer.GetDataContainer().DataPack.Columns;
            float[] info = obj;
            string[] data = new string[info.Length];
            for (int i = 0; i < info.Length; i++)
            {
                data[i] = headers[i] + ": " + info[i];
            }

            if (selectionState)
            {
                string result = "";
                foreach (string s in data)
                {
                    result += s + "\n";
                }
                dataTMP.text = result;
            }
        }

        private void SetIsolateToggle(bool value)
        {
            DataRenderer dataRenderer = RenderManager.Instance.DataRenderer;
            dataRenderer.GetAstrovidioDataSetRenderer().DataMapping.isolateSelection = value;
        }

        private void OnSelectionPerformed(float[] obj)
        {
            SetInspectorInfo(obj);
        }

    }

}
