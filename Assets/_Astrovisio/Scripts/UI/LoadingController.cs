using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class LoadingController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float spinnerSpeed = 180f;

        private VisualElement spinner;
        private float rotationAngle = 0f;

        private void Start()
        {
            var uiDocument = GetComponentInParent<UIDocument>();
            var loaderView = uiDocument.rootVisualElement.Q<VisualElement>("LoaderView");
            spinner = loaderView.Q<VisualElement>("LoadingSpinner");
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
    }
}
