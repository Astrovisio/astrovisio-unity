using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Astrovisio
{
    public class VRManager : MonoBehaviour
    {
        public static VRManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of VRManager found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }

        public void StartVRMode()
        {
            StartCoroutine(StartXR());
        }

        public void StopVRMode()
        {
            StopXR();
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
                Debug.Log("[VRManager] XR Loader initialized. Starting subsystems...");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }
        }

        private void StopXR()
        {
            Debug.Log("[VRManager] Stopping XR...");
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}
