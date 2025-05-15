
using System;
using UnityEngine;

namespace Astrovisio
{
    public class RenderManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private APIManager apiManager;
        [SerializeField] private ProjectManager projectManager;

        [SerializeField] private GameObject cubePrefab;


        private void Start()
        {
            projectManager.ProjectProcessed += OnProjectProcessed;
        }

        private void OnProjectProcessed(ProcessedData data)
        {
            // Instantiate(cubePrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        }

    }

}