using CatalogData;
using UnityEngine;

namespace Astrovisio
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        [SerializeField]
        private RenderManager renderManager;


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

    }

}
