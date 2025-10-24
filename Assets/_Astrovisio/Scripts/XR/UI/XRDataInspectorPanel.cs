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
        [SerializeField] private Button cubeButton;

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
                    // sphereButton.AddToClassList("active");
                    // cubeButton.RemoveFromClassList("active");
                    this.selectionMode = SelectionMode.Sphere;
                    kDTreeComponent.selectionMode = SelectionMode.Sphere;
                    sizeLabel.text = "Diameter";
                    break;
                case SelectionMode.Cube:
                    // cubeButton.AddToClassList("active");
                    // sphereButton.RemoveFromClassList("active");
                    this.selectionMode = SelectionMode.Cube;
                    kDTreeComponent.selectionMode = SelectionMode.Cube;
                    sizeLabel.text = "Side";
                    break;
            }

            SetSelectionSize(selectionSize);
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

    }

}
