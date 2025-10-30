/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
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

using System;
using System.Collections;
using Astrovisio.XR;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Astrovisio
{
    public class XRManager : MonoBehaviour
    {
        public static XRManager Instance { get; private set; }

        private bool VRActive = false;
        public bool IsVRActive => VRActive;

        private Coroutine startXRCoroutine;

        [Header("Dependencies")]
        [SerializeField] private XRUIManager xrUIManager;
        [SerializeField] private XRInputController xrInputController;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject xrOrigin;
        [SerializeField] private GameObject worldCanvasGO;

        [Header("Others")]
        [SerializeField] private bool beginOnPlay = false;

        // === Events ===
        public event Action OnVRStart;
        public event Action OnVREnd;

        // === Local ===
        private Vector3 xrOriginOriginalPosition;
        private Quaternion xrOriginOriginalRotation;
        private Vector3 xrOriginOriginalScale;
        private bool isStopping;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (beginOnPlay)
            {
                EnterVR(() => { });
            }

            xrOriginOriginalPosition = xrOrigin.transform.position;
            xrOriginOriginalRotation = xrOrigin.transform.rotation;
            xrOriginOriginalScale = xrOrigin.transform.localScale;
        }

        private void OnDestroy()
        {
            try
            {
                if (VRActive)
                {
                    XRManagerSettings mgr = XRGeneralSettings.Instance?.Manager;
                    if (mgr != null)
                    {
                        if (mgr.isInitializationComplete)
                        {
                            mgr.StopSubsystems();
                        }
                        mgr?.DeinitializeLoader();
                    }
                }
            }
            catch
            {
                Debug.LogWarning("XRManager Destroyed");
            }
        }


        [ContextMenu("Enter VR")]
        public void EnterDebugVR()
        {
            mainCamera.gameObject.SetActive(false);
            xrOrigin.SetActive(true);
            StartCoroutine(StartDebugXR());
        }

        public void EnterVR(Action OnSuccess)
        {
            if (VRActive)
            {
                return;
            }

            mainCamera.gameObject.SetActive(false);
            xrOrigin.SetActive(true);
            startXRCoroutine = StartCoroutine(StartXR(OnSuccess));
        }

        public void ResetXROriginTransform()
        {
            xrOrigin.transform.position = xrOriginOriginalPosition;
            xrOrigin.transform.rotation = xrOriginOriginalRotation;
            xrOrigin.transform.localScale = xrOriginOriginalScale;
        }

        public void ResetDataRendererTransform()
        {
            DataRenderer dataRenderer = RenderManager.Instance.DataRenderer;
            dataRenderer.ResetDatasetTransform();
        }

        [ContextMenu("Exit VR")]
        public void ExitVR()
        {
            if (!VRActive || isStopping) return;
            isStopping = true;

            if (startXRCoroutine != null)
            {
                StopCoroutine(startXRCoroutine);
                startXRCoroutine = null;
            }

            StartCoroutine(StopXRAndReturnToDesktop());
            ResetDataRendererTransform();
        }

        private IEnumerator StartDebugXR()
        {
            // Debug.Log("[VRManager] Initializing XR...");
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogError("[VRManager] Failed to initialize XR Loader.");
            }
            else
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                InitVRSettings(); // uncomment!
                VRActive = true;
                OnVRStart?.Invoke();
            }
        }

        private IEnumerator StartXR(Action OnSuccess)
        {
            Debug.Log("[XRManager] Initializing XR...");
            uiManager.SetLoadingView(true);

            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogWarning("[XRManager] Failed to initialize XR Loader. Make sure an XR Plug-in is enabled in Project Settings > XR Plug-in Management.");
                OnXRFailed();
                uiManager.SetLoadingView(false);
                yield break;
            }

            try
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();

                InitVRSettings();
                VRActive = true;
                worldCanvasGO.SetActive(true);
                uiManager.SwitchEventSystemToVR();

                if (RenderManager.Instance.isInspectorModeActive)
                {
                    RenderManager.Instance.SetDataInspector(false, true);
                }
                else
                {
                    RenderManager.Instance.SetDataInspector(false, false);
                }

                Debug.Log("[XRManager] XR successfully initialized.");
                OnVRStart?.Invoke();

            }
            catch (Exception ex)
            {
                Debug.LogError($"[XRManager] Exception during XR subsystem startup: {ex.Message}");
                OnXRFailed();
            }
            finally
            {
                uiManager.SetLoadingView(false);
                OnSuccess();
            }
        }

        private IEnumerator StopXRAndReturnToDesktop()
        {
            Debug.Log("[XRManager] Stopping XR...");

            if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                XRGeneralSettings.Instance.Manager.StopSubsystems();
            }
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();

            yield return null;

            xrOrigin.SetActive(false);
            mainCamera.gameObject.SetActive(true);
            worldCanvasGO.SetActive(false);
            uiManager.SwitchEventSystemToDesktop();
            InitDesktopSettings();
            RenderManager.Instance.SetDataInspector(false, false);
            uiManager.SetDataInspectorVisibility(false);

            RemoveXRUserInterface();
            VRActive = false;
            isStopping = false;
            OnVREnd?.Invoke();
            Debug.Log("[XRManager] XR stopped and returned to desktop mode.");
        }

        private void OnXRFailed()
        {
            VRActive = false;
            uiManager.SetLoadingView(false);
            uiManager.SetErrorVR(true);
            Debug.Log("[XRManager] Fallback to non-VR mode.");

            xrOrigin.SetActive(false);
            mainCamera.gameObject.SetActive(true);
        }

        private void InitVRSettings()
        {
            KDTreeComponent kdTreeComponent = FindAnyObjectByType<KDTreeComponent>();
            TransformManipulator transformManipulator = FindAnyObjectByType<TransformManipulator>();
            XRInputController xrController = FindAnyObjectByType<XRInputController>();

            if (kdTreeComponent != null && xrController != null)
            {
                kdTreeComponent.SetControllerTransform(xrController.GetRightPokePoint());
                if (transformManipulator != null)
                {
                    transformManipulator.targetObject = kdTreeComponent.gameObject.transform;
                }
            }
            if (transformManipulator != null && xrController != null)
            {
                transformManipulator.leftController = xrController.GetLeftPokePoint();
                transformManipulator.rightController = xrController.GetRightPokePoint();
            }
        }

        private void InitDesktopSettings()
        {
            KDTreeComponent kdTreeComponent = FindAnyObjectByType<KDTreeComponent>();
            CameraTarget cameraTarget = FindAnyObjectByType<CameraTarget>();


            if (kdTreeComponent != null)
            {
                // kdTreeComponent.controllerTransform = cameraTarget.gameObject.transform;
                kdTreeComponent.SetControllerTransform(cameraTarget.gameObject.transform);
            }
        }

        public void InstantiatePanel(GameObject panelGO)
        {
            if (panelGO == null)
            {
                Debug.LogWarning("[XRManager] InstantiatePanel: panelGO is null.");
                return;
            }

            // Target distance from the user's view
            const float targetDistance = 1.5f;

            // Get the user view transform (XR camera if in VR, otherwise mainCamera)
            Transform viewTf = GetUserViewTransform();
            if (viewTf == null)
            {
                Debug.LogWarning("[XRManager] InstantiatePanel: could not determine user view transform.");
                return;
            }

            // Compute position and rotation in front of the user
            Vector3 spawnPos = viewTf.position + viewTf.forward * targetDistance;
            Quaternion spawnRot = Quaternion.LookRotation(viewTf.forward, Vector3.up);

            // Instantiate panel
            GameObject instance = Instantiate(panelGO);
            instance.transform.SetPositionAndRotation(spawnPos, spawnRot);
        }

        private Transform GetUserViewTransform()
        {
            if (VRActive && xrOrigin != null)
            {
                // Try to find the XR camera under the origin
                Camera xrCamera = xrOrigin.GetComponentInChildren<Camera>(true);
                if (xrCamera != null)
                    return xrCamera.transform;
            }

            // Desktop mode or fallback
            if (mainCamera != null)
                return mainCamera.transform;

            if (xrOrigin != null)
                return xrOrigin.transform;

            // Final fallback: search for any camera
            Camera anyCamera = FindAnyObjectByType<Camera>();
            return anyCamera != null ? anyCamera.transform : null;
        }

        private void RemoveXRUserInterface()
        {
            XRMenuPanel xrMenuPanel = FindAnyObjectByType<XRMenuPanel>();
            if (xrMenuPanel != null)
            {
                xrMenuPanel.DestroyAllPanels();
            }
        }

    }

}
