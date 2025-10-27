/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using CatalogData;
using UnityEngine;
// Alias to avoid name clash with your Astrovisio.SceneManager class
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Astrovisio
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private RenderManager renderManager;
        [SerializeField] private Camera mainCamera;

        [Header("Additive Scene (Gizmo)")]
        [Tooltip("Name of the scene that contains only the gizmo UI/objects.")]
        [SerializeField] private string gizmoSceneName = "GizmoScene";

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
            // Camera cache
            orbitController = mainCamera != null ? mainCamera.GetComponent<OrbitCameraController>() : null;
            if (orbitController != null && orbitController.target != null)
            {
                initialCameraTargetPosition = orbitController.target.position;
                initialCameraRotation = orbitController.transform.rotation.eulerAngles;
                initialCameraDistance = Vector3.Distance(orbitController.transform.position, orbitController.target.position);
            }

#if !UNITY_EDITOR
            // In build: load gizmo scene additively if requested and not already loaded
            if (!string.IsNullOrWhiteSpace(gizmoSceneName))
            {
                TryLoadGizmoSceneAdditive();
            }
#endif
        }

        /// <summary>
        /// Loads the gizmo scene additively if it's not already loaded.
        /// </summary>
        private void TryLoadGizmoSceneAdditive()
        {
            if (IsSceneLoaded(gizmoSceneName))
            {
                // Already there (e.g., if your boot scene loaded it before)
                return;
            }

            var op = USceneManager.LoadSceneAsync(gizmoSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogError($"[SceneManager] Failed to start loading additive scene '{gizmoSceneName}'. " +
                               $"Check Build Settings > Scenes In Build and the exact scene name.");
                return;
            }

            op.completed += _ =>
            {
                Debug.Log($"[SceneManager] Additive scene '{gizmoSceneName}' loaded in build.");
                // Keep the current (app) scene active for lighting/input unless you need otherwise.
                // USceneManager.SetActiveScene(gameObject.scene);
            };
        }

        /// <summary>
        /// Returns true if a scene with the given name is already loaded at runtime.
        /// </summary>
        private static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < USceneManager.sceneCount; i++)
            {
                var scn = USceneManager.GetSceneAt(i);
                if (scn.IsValid() && scn.isLoaded && scn.name == sceneName)
                    return true;
            }
            return false;
        }

        public void SetAxesGizmoVisibility(bool visibility)
        {
            DataRenderer dataRenderer = renderManager != null ? renderManager.DataRenderer : null;

            if (dataRenderer is not null)
            {
                AstrovisioDataSetRenderer astrovisioDataSetRenderer = dataRenderer.GetAstrovidioDataSetRenderer();
                AxesCanvasHandler axesCanvasHandler = astrovisioDataSetRenderer.GetComponentInChildren<AxesCanvasHandler>(true);
                axesCanvasHandler.gameObject.SetActive(visibility);
            }
        }

        public bool GetAxisGizmoVisibility()
        {
            DataRenderer dataRenderer = renderManager != null ? renderManager.DataRenderer : null;
            // AstrovisioDataSetRenderer datasetRenderer = dataRenderer != null ? dataRenderer.GetAstrovidioDataSetRenderer() : null;
            AxesCanvasHandler axesHandler = dataRenderer.axesCanvasHandler;

            // AxesCanvasHandler axesHandler = datasetRenderer != null ? datasetRenderer.GetComponentInChildren<AxesCanvasHandler>(true) : null;

            return axesHandler != null && axesHandler.gameObject.activeSelf;
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
