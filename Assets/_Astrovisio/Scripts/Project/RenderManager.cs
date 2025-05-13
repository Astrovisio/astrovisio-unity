
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
            Debug.Log("RenderManager");

            projectManager.ProjectProcessed += OnProjectProcessed;
        }

        private void OnProjectProcessed(Project project)
        {
            Debug.Log("OnProjectProcessed");
            Instantiate(cubePrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        }

    }

}