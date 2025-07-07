using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Astrovisio
{
    public class XRManager : MonoBehaviour
    {
        public static XRManager Instance { get; private set; }

        private bool VRActive = false;
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


        private Vector3 xrOriginOriginalPosition;
        private Quaternion xrOriginOriginalRotation;
        private Vector3 xrOriginOriginalScale;


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
                EnterVR();
            }

            xrOriginOriginalPosition = xrOrigin.transform.position;
            xrOriginOriginalRotation = xrOrigin.transform.rotation;
            xrOriginOriginalScale = xrOrigin.transform.localScale;
        }

        [ContextMenu("Enter VR")]
        public void EnterDebugVR()
        {
            mainCamera.gameObject.SetActive(false);
            xrOrigin.SetActive(true);
            StartCoroutine(StartDebugXR());
        }

        public void EnterVR()
        {
            if (VRActive)
            {
                return;
            }

            mainCamera.gameObject.SetActive(false);
            xrOrigin.SetActive(true);
            startXRCoroutine = StartCoroutine(StartXR());
        }

        public void ResetXROriginTransform()
        {
            xrOrigin.transform.position = xrOriginOriginalPosition;
            xrOrigin.transform.rotation = xrOriginOriginalRotation;
            xrOrigin.transform.localScale = xrOriginOriginalScale;
        }

        public void ResetDataRendererTransform()
        {
            DataRenderer dataRenderer = RenderManager.Instance.GetCurrentDataRenderer();
            dataRenderer.ResetDatasetTransform();
        }

        [ContextMenu("Exit VR")]
        public void ExitVR()
        {
            if (!VRActive)
            {
                return;
            }

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
            }
        }

        private IEnumerator StartXR()
        {
            Debug.Log("[XRManager] Initializing XR...");
            uiManager.SetLoading(true);

            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogWarning("[XRManager] Failed to initialize XR Loader. Make sure an XR Plug-in is enabled in Project Settings > XR Plug-in Management.");
                OnXRFailed();
                uiManager.SetLoading(false);
                yield break;
            }

            try
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();

                InitVRSettings();
                VRActive = true;
                worldCanvasGO.SetActive(true);
                Debug.Log("[XRManager] XR successfully initialized.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[XRManager] Exception during XR subsystem startup: {ex.Message}");
                OnXRFailed();
            }
            finally
            {
                uiManager.SetLoading(false);
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
            VRActive = false;
            worldCanvasGO.SetActive(false);
            Debug.Log("[XRManager] XR stopped and returned to desktop mode.");
        }


        private void OnXRFailed()
        {
            VRActive = false;
            uiManager.SetLoading(false);
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
                kdTreeComponent.controllerTransform = xrController.GetRightPokePoint();
                transformManipulator.targetObject = kdTreeComponent.gameObject.transform;
            }
            if (transformManipulator != null && xrController != null)
            {
                transformManipulator.leftController = xrController.GetLeftPokePoint();
                transformManipulator.rightController = xrController.GetRightPokePoint();
            }
        }

        private void OnDestroy()
        {
            if (VRActive)
            {
                XRGeneralSettings.Instance.Manager.StopSubsystems();
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
        }

    }

}
