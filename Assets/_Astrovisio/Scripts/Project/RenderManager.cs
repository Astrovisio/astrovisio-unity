using UnityEngine;

namespace Astrovisio
{
    public class RenderManager : MonoBehaviour
    {
        public static RenderManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private APIManager apiManager;
        [SerializeField] private ProjectManager projectManager;

        [Header("Other")]
        [SerializeField] private DataRenderer dataRendererPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of RenderManager found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RenderDataContainer(DataContainer dataContainer)
        {
            DataRenderer[] allDataRenderer = FindObjectsByType<DataRenderer>(FindObjectsSortMode.None);
            foreach (DataRenderer dR in allDataRenderer)
            {
                Destroy(dR.gameObject);
            }

            DataRenderer dataRenderer = Instantiate(dataRendererPrefab);
            dataRenderer.RenderDataContainer(dataContainer);
        }

    }
}
