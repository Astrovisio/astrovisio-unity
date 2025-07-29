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
            inspectorToggle = Root.Q<VisualElement>("InspectorToggle").Q<Toggle>("CheckboxRoot");
            sphereButton = Root.Q<Button>("SphereButton");
            cubeButton = Root.Q<Button>("CubeButton");
            sizeInputFloatField = Root.Q<FloatField>("SizeInputFloatField");
            mediaDropdownField = Root.Q<VisualElement>("MediaDropdown").Q<DropdownField>("DropdownField");
            // inspectorScrollView = Root.Q<ScrollView>("InspectorScrollView"); TODO

            sphereButton.parent.SetEnabled(false);
            sizeInputFloatField.parent.SetEnabled(false);
            mediaDropdownField.parent.SetEnabled(false);

            inspectorToggle.value = inspectorState;
            inspectorToggle.RegisterValueChangedCallback(evt =>
            {
                inspectorState = evt.newValue;

                sphereButton.parent.SetEnabled(inspectorState);
                sizeInputFloatField.parent.SetEnabled(inspectorState);

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

        private void Reset()
        {
            //
        }

    }

}
