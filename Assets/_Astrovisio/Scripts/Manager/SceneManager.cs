using CatalogData;
using UnityEngine;

namespace Astrovisio
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        [SerializeField] private RenderManager renderManager;
        [SerializeField] private Camera mainCamera;

        // Camera
        private Vector3 initialCameraTargetPosition;
        private Vector3 initialCameraRotation;
        private float initialCameraDistance;
        private OrbitCameraController orbitController;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of SceneManager found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            orbitController = mainCamera.GetComponent<OrbitCameraController>();

            if (orbitController != null)
            {
                initialCameraTargetPosition = orbitController.target.position;
                initialCameraRotation = orbitController.transform.rotation.eulerAngles;
                initialCameraDistance = Vector3.Distance(orbitController.transform.position, orbitController.target.position);
            }
        }

        public void SetAxesGizmoVisibility(bool visibility)
        {
            DataRenderer dataRenderer = renderManager.DataRenderer;

            if (dataRenderer is not null)
            {
                AstrovisioDataSetRenderer astrovisioDataSetRenderer = dataRenderer.GetAstrovidioDataSetRenderer();
                AxesCanvasHandler axesCanvasHandler = astrovisioDataSetRenderer.GetComponentInChildren<AxesCanvasHandler>(true);
                axesCanvasHandler.gameObject.SetActive(visibility);
            }
        }

        public void ResetCameraTransform()
        {
            if (orbitController != null)
            {
                orbitController.ResetCameraView(initialCameraTargetPosition, initialCameraRotation, initialCameraDistance);
            }
        }

    }

}
