using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Management;

namespace Astrovisio
{
    public class VRManager : MonoBehaviour
    {
        public static VRManager Instance { get; private set; }

        private bool VRActive = false;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject xrOrigin;

        private Transform dataRendererTransform;
        private Vector3 originalScale;
        private Quaternion originalRotation;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!VRActive || !XRGeneralSettings.Instance.Manager.isInitializationComplete || dataRendererTransform == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                Debug.Log("[VRManager] B button pressed. Exiting VR...");
                ExitVR();
                return;
            }

            // HandleCubeInteraction();
        }

        private void HandleCubeInteraction()
        {
            // Scaling
            float scaleInput = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");
            if (Mathf.Abs(scaleInput) > 0.1f)
            {
                float scaleSpeed = 2f;
                float scaleFactor = 1 + scaleInput * scaleSpeed * Time.deltaTime;
                dataRendererTransform.localScale *= scaleFactor;
            }

            // Rotation Y
            float rotationInputHorizontal = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
            if (Mathf.Abs(rotationInputHorizontal) > 0.1f)
            {
                float rotationSpeed = 90f;
                dataRendererTransform.Rotate(Vector3.up, rotationInputHorizontal * rotationSpeed * Time.deltaTime, Space.World);
            }

            // Rotation X
            float rotationInputVertical = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical");
            if (Mathf.Abs(rotationInputVertical) > 0.1f)
            {
                float rotationSpeed = 90f;
                dataRendererTransform.Rotate(Vector3.right, -rotationInputVertical * rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        [ContextMenu("Enter VR")]
        public void EnterVR()
        {
            mainCamera.gameObject.SetActive(false);
            xrOrigin.SetActive(true);
            StartCoroutine(StartXR());

            if (RenderManager.Instance != null && RenderManager.Instance.GetCurrentDataRenderer() != null)
            {
                dataRendererTransform = RenderManager.Instance.GetCurrentDataRenderer().transform;
                originalScale = dataRendererTransform.localScale;
                originalRotation = dataRendererTransform.rotation;
            }
        }

        [ContextMenu("Exit VR")]
        public void ExitVR()
        {
            StopCoroutine(StartXR());
            StopXR();
            xrOrigin.SetActive(false);
            mainCamera.gameObject.SetActive(true);

            if (dataRendererTransform != null)
            {
                dataRendererTransform.localScale = originalScale;
                dataRendererTransform.rotation = originalRotation;
            }
        }


        private IEnumerator StartXR()
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
                // Debug.Log("[VRManager] XR started.");
                InitVRSettings();
                VRActive = true;
            }
        }

        private void InitVRSettings()
        {
            KDTreeComponent kdTreeComponent = FindAnyObjectByType<KDTreeComponent>();
            XRController xrController = FindAnyObjectByType<XRController>();
            kdTreeComponent.controllerTransform = xrController.GetPokePoint();
        }

        private void StopXR()
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            // Debug.Log("[VRManager] XR stopped.");
            VRActive = false;
        }

        private void OnDestroy()
        {
            if (VRActive)
            {
                StopXR();
            }
        }

    }
}
