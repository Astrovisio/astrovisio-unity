using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Astrovisio
{
    public class VRManager : MonoBehaviour
    {
        public static VRManager Instance { get; private set; }

        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject xrOrigin;

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
            if (XRGeneralSettings.Instance.Manager.isInitializationComplete && Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                Debug.Log("[VRManager] B button pressed. Exiting VR...");
                ExitVR();
            }
        }


        public void EnterVR()
        {
            mainCamera.gameObject.SetActive(false);
            xrOrigin.SetActive(true);
            StartCoroutine(StartXR());
        }

        public void ExitVR()
        {
            StopCoroutine(StartXR());
            StopXR();
            xrOrigin.SetActive(false);
            mainCamera.gameObject.SetActive(true);
        }

        private IEnumerator StartXR()
        {
            Debug.Log("[VRManager] Initializing XR...");
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogError("[VRManager] Failed to initialize XR Loader.");
            }
            else
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                Debug.Log("[VRManager] XR started.");
            }
        }

        private void StopXR()
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            Debug.Log("[VRManager] XR stopped.");
        }

    }
}
