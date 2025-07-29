using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class InspectorSettingController
    {
        public VisualElement Root { get; }


        private enum InspectorShape
        {
            Sphere,
            Cube
        }


        private Toggle inspectorToggle;
        private Button sphereButton;
        private Button cubeButton;
        private FloatField sizeInputFloatField;
        private DropdownField mediaDropdownField;
        private ScrollView inspectorScrollView;

        private bool inspectorState = false;
        private InspectorShape inspectorShape = InspectorShape.Sphere;

        public InspectorSettingController(VisualElement root)
        {
            Root = root;
            Init();
        }

        private void Init()
        {
            RenderManager.Instance.OnDataInspectorChanged += OnDataInspectorChanged;

            inspectorToggle = Root.Q<VisualElement>("InspectorToggle").Q<Toggle>("CheckboxRoot");
            sphereButton = Root.Q<Button>("SphereButton");
            cubeButton = Root.Q<Button>("CubeButton");
            sizeInputFloatField = Root.Q<FloatField>("SizeInputFloatField");
            mediaDropdownField = Root.Q<VisualElement>("MediaDropdown").Q<DropdownField>("DropdownField");
            inspectorScrollView = Root.Q<ScrollView>("DataInspectorScollView");

            sphereButton.parent.SetEnabled(false);
            sizeInputFloatField.parent.SetEnabled(false);
            mediaDropdownField.parent.SetEnabled(false);
            inspectorScrollView.parent.SetEnabled(false);

            inspectorToggle.value = inspectorState;
            inspectorToggle.RegisterValueChangedCallback(evt =>
            {
                inspectorState = evt.newValue;

                RenderManager.Instance.SetDataInspector(inspectorState, inspectorState);

                sphereButton.parent.SetEnabled(inspectorState);
                sizeInputFloatField.parent.SetEnabled(inspectorState);
                mediaDropdownField.parent.SetEnabled(inspectorState);
                inspectorScrollView.parent.SetEnabled(inspectorState);

                SetInspectorShape(inspectorShape);
            });


            // Shape
            SetInspectorShape(InspectorShape.Sphere);
            sphereButton.clicked += () => SetInspectorShape(InspectorShape.Sphere);
            cubeButton.clicked += () => SetInspectorShape(InspectorShape.Cube);

            // Size
            if (sizeInputFloatField != null)
            {
                sizeInputFloatField.RegisterValueChangedCallback(evt =>
                {
                    float newValue = evt.newValue;
                    Debug.Log($"Size changed: {newValue}");
                });
            }
        }

        private void OnDataInspectorChanged(string[] obj)
        {
            // Debug.Log("OnDataInspectorChanged");
            if (inspectorState)
            {
                SetInspectorScrollView(obj);
            }
        }

        private void SetInspectorShape(InspectorShape inspectorShape)
        {
            switch (inspectorShape)
            {
                case InspectorShape.Sphere:
                    sphereButton.AddToClassList("active");
                    cubeButton.RemoveFromClassList("active");
                    this.inspectorShape = InspectorShape.Sphere;
                    break;
                case InspectorShape.Cube:
                    cubeButton.AddToClassList("active");
                    sphereButton.RemoveFromClassList("active");
                    this.inspectorShape = InspectorShape.Cube;
                    break;
            }
        }

        private void SetInspectorScrollView(string[] data)
        {
            inspectorScrollView.Clear();

            foreach (string row in data)
            {
                Label label = new Label();
                label.text = row;
                inspectorScrollView.Add(label);
            }
        }

        private void Reset()
        {
            //
        }

    }

}
