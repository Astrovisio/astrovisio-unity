using System;
using CatalogData;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class InspectorSettingController
    {

        // === Dependencies ===
        public VisualElement Root { get; }

        private Toggle inspectorToggle;
        private Button sphereButton;
        private Button cubeButton;
        private FloatField sizeInputFloatField;
        private DropdownField mediaDropdownField;
        private ScrollView inspectorInfoScrollView;

        // === Local ===
        private KDTreeComponent _subscribedKDTree;
        private bool selectionState = false;
        private SelectionMode selectionMode = SelectionMode.Sphere;
        private float defaultSectionSize = 0.05f;
        private float selectionSize;
        private AggregationMode aggregationMode = AggregationMode.Average;



        public InspectorSettingController(VisualElement root)
        {
            Root = root;

            // RenderManager.Instance.OnProjectRenderReady += OnProjectRenderReady;
            RenderManager.Instance.OnProjectRenderStart += OnProjectRenderStart;
            RenderManager.Instance.OnProjectRenderEnd += OnProjectRenderEnd;

            Init();
        }

        private void Init()
        {
            inspectorToggle = Root.Q<VisualElement>("InspectorToggle").Q<Toggle>("CheckboxRoot");
            sphereButton = Root.Q<Button>("SphereButton");
            cubeButton = Root.Q<Button>("CubeButton");
            sizeInputFloatField = Root.Q<FloatField>("SizeInputFloatField");
            mediaDropdownField = Root.Q<VisualElement>("MediaDropdown").Q<DropdownField>("DropdownField");
            inspectorInfoScrollView = Root.Q<ScrollView>("DataInspectorScollView");

            sphereButton.parent.SetEnabled(false);
            sizeInputFloatField.parent.SetEnabled(false);
            mediaDropdownField.parent.SetEnabled(false);
            inspectorInfoScrollView.parent.SetEnabled(false);

            inspectorToggle.value = selectionState;
            inspectorToggle.RegisterValueChangedCallback(evt =>
            {
                selectionState = evt.newValue;

                RenderManager.Instance.SetDataInspector(selectionState, selectionState);
                sphereButton.parent.SetEnabled(selectionState);
                sizeInputFloatField.parent.SetEnabled(selectionState);
                mediaDropdownField.parent.SetEnabled(selectionState);
                inspectorInfoScrollView.parent.SetEnabled(selectionState);

                SetSelectionMode(selectionMode);
                ShowSelectionGizmo(selectionState);
            });


            // Shape
            SetSelectionMode(SelectionMode.Sphere);
            sphereButton.clicked += () => SetSelectionMode(SelectionMode.Sphere);
            cubeButton.clicked += () => SetSelectionMode(SelectionMode.Cube);

            // Size
            if (sizeInputFloatField != null)
            {
                sizeInputFloatField.RegisterValueChangedCallback(evt =>
                {
                    selectionSize = evt.newValue;
                    // Debug.Log($"Size changed: {newValue}");
                    SetSelectionSize(selectionSize);
                });

                sizeInputFloatField.value = defaultSectionSize;
            }

            // Aggregation
            if (mediaDropdownField != null)
            {
                mediaDropdownField.choices = new System.Collections.Generic.List<string>(
                     Enum.GetNames(typeof(AggregationMode))
                );

                mediaDropdownField.value = aggregationMode.ToString();

                mediaDropdownField.RegisterValueChangedCallback(evt =>
                {
                    if (Enum.TryParse<AggregationMode>(evt.newValue, out var newAggregationMode))
                    {
                        SetAggregationMode(newAggregationMode);
                    }
                });
            }

            ShowSelectionGizmo(false);
        }

        public void InitInspectorSettings(SelectionMode selectionMode, float selectionSize, string selectionFunction)
        {
            selectionState = false;

            SetSelectionMode(selectionMode);
            SetSelectionSize(selectionSize);
            SetSelectionFunction(selectionFunction);
        }

        private void OnProjectRenderStart(Project project)
        {
            Unhook();
        }

        private void OnProjectRenderEnd(Project project)
        {
            Reset();
            TryHookToCurrentKDTree();
        }

        private void TryHookToCurrentKDTree()
        {
            DataRenderer dr = RenderManager.Instance?.GetCurrentDataRenderer();
            KDTreeComponent kd = dr?.GetKDTreeComponent();
            Hook(kd);
        }

        private void Hook(KDTreeComponent kd)
        {
            if (kd == null)
            {
                return;
            }

            if (ReferenceEquals(_subscribedKDTree, kd))
            {
                return;
            }

            Unhook();

            _subscribedKDTree = kd;
            _subscribedKDTree.OnSelectionPerformed += OnSelectionPerformed;
            // Debug.Log("Hook to " + RenderManager.Instance.GetCurrentDataRenderer().GetDataContainer().Project.Name);
        }

        private void Unhook()
        {
            if (_subscribedKDTree == null)
            {
                return;
            }
            _subscribedKDTree.OnSelectionPerformed -= OnSelectionPerformed;
            // Debug.Log("Unhook from " + RenderManager.Instance.GetCurrentDataRenderer().GetDataContainer().Project.Name);
            _subscribedKDTree = null;
        }

        private void OnSelectionPerformed(float[] obj)
        {
            Debug.Log("OnSelectionPerformed " + obj[0] + obj[1]);

            DataRenderer dataRenderer = RenderManager.Instance.GetCurrentDataRenderer();

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
                SetInspectorInfo(data);
            }
        }

        private void ShowSelectionGizmo(bool visibility)
        {
            DataRenderer dataRenderer = RenderManager.Instance.GetCurrentDataRenderer();
            KDTreeComponent kDTreeComponent = dataRenderer?.GetKDTreeComponent();

            if (kDTreeComponent == null)
            {
                return;
            }

            kDTreeComponent.showSelectionGizmo = visibility;
        }

        private void SetSelectionMode(SelectionMode selectionMode)
        {
            DataRenderer dataRenderer = RenderManager.Instance.GetCurrentDataRenderer();
            KDTreeComponent kDTreeComponent = dataRenderer?.GetKDTreeComponent();

            if (kDTreeComponent == null)
            {
                return;
            }

            switch (selectionMode)
            {
                case SelectionMode.Sphere:
                    sphereButton.AddToClassList("active");
                    cubeButton.RemoveFromClassList("active");
                    this.selectionMode = SelectionMode.Sphere;
                    kDTreeComponent.selectionMode = SelectionMode.Sphere;
                    break;
                case SelectionMode.Cube:
                    cubeButton.AddToClassList("active");
                    sphereButton.RemoveFromClassList("active");
                    this.selectionMode = SelectionMode.Cube;
                    kDTreeComponent.selectionMode = SelectionMode.Cube;
                    break;
            }

            SetSelectionSize(selectionSize);
        }

        private void SetAggregationMode(AggregationMode aggregationMode)
        {
            DataRenderer dataRenderer = RenderManager.Instance.GetCurrentDataRenderer();
            KDTreeComponent kDTreeComponent = dataRenderer?.GetKDTreeComponent();

            if (kDTreeComponent == null)
            {
                return;
            }

            this.aggregationMode = aggregationMode;
            kDTreeComponent.aggregationMode = aggregationMode;
        }

        private void SetSelectionSize(float selectionSize)
        {
            DataRenderer dataRenderer = RenderManager.Instance.GetCurrentDataRenderer();
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
        }

        private void SetSelectionFunction(string function)
        {
            throw new NotImplementedException();
        }

        private void SetInspectorInfo(string[] data)
        {
            inspectorInfoScrollView.Clear();

            foreach (string row in data)
            {
                Label label = new Label();
                label.text = row;
                inspectorInfoScrollView.Add(label);
            }
        }

        public void Reset()
        {
            selectionState = false;
            SetSelectionMode(SelectionMode.Sphere);
            SetSelectionSize(defaultSectionSize);
            SetAggregationMode(AggregationMode.Average);
            ShowSelectionGizmo(false);
            inspectorToggle.value = false;
        }

    }

}
