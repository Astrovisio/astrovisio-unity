using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Astrovisio
{

    [RequireComponent(typeof(XRInputController))]
    public class XRInputHandler : MonoBehaviour
    {

        private XRInputController xrInputController;

        [SerializeField] private Canvas xrCanvas;
        [SerializeField] private Canvas leftCanvas;
        [SerializeField] private Canvas rightCanvas;


        [Header("Controllers")]
        [SerializeField] private XRDataInfoUIController xrDataInfoUIController;
        [SerializeField] private XRResetTransformUIController xrResetTransformUIController;
        [SerializeField] private XRHelpUIController xrHelpUIController;


        private void Awake()
        {
            xrInputController = GetComponent<XRInputController>();

            if (xrInputController == null)
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            xrInputController.OnLeftPrimaryButtonPressed += OnLeftPrimaryButtonPressed;
            xrInputController.OnLeftSecondaryButtonPressed += OnLeftSecondaryButtonPressed;
            xrInputController.OnLeftMenuButtonPressed += OnLeftMenuButtonPressed;
            xrInputController.OnRightPrimaryButtonPressed += OnRightPrimaryButtonPressed;
            xrInputController.OnRightSecondaryButtonPressed += OnRightSecondaryButtonPressed;

            xrInputController.OnRightPrimaryButtonReleased += OnRightPrimaryButtonReleased;
        }

        private void OnDisable()
        {
            xrInputController.OnLeftPrimaryButtonPressed -= OnLeftPrimaryButtonPressed;
            xrInputController.OnLeftSecondaryButtonPressed += OnLeftSecondaryButtonPressed;
            xrInputController.OnLeftMenuButtonPressed -= OnLeftMenuButtonPressed;
            xrInputController.OnRightPrimaryButtonPressed -= OnRightPrimaryButtonPressed;
            xrInputController.OnRightSecondaryButtonPressed -= OnRightSecondaryButtonPressed;
        }

        private void OnLeftPrimaryButtonPressed()
        {
            xrDataInfoUIController.TogglePanelVisibility();
            RenderManager.Instance.SetDebugSphere(xrDataInfoUIController.GetPanelVisibility());
        }

        private void OnLeftSecondaryButtonPressed()
        {
            xrHelpUIController.ToggleHelpImage();
        }

        private void OnLeftMenuButtonPressed()
        {
            xrCanvas.gameObject.SetActive(xrCanvas.gameObject);
        }

        private Coroutine holdCoroutine;
        private float holdDuration = 1f;

        private void OnRightPrimaryButtonPressed()
        {
            if (holdCoroutine == null)
            {
                holdCoroutine = StartCoroutine(HoldRoutine());
            }
        }

        private void OnRightPrimaryButtonReleased()
        {
            if (holdCoroutine != null)
            {
                xrResetTransformUIController.SetLoaderImage(false, 0f);
                StopCoroutine(holdCoroutine);
                holdCoroutine = null;
            }
        }

        private IEnumerator HoldRoutine()
        {
            float elapsed = 0f;

            xrResetTransformUIController.SetLoaderImage(true, elapsed);
            while (elapsed < holdDuration)
            {
                // Debug.Log($"elapsed: {elapsed}");
                elapsed += Time.deltaTime;
                xrResetTransformUIController.SetLoaderImage(true, elapsed);
                yield return null;
            }
            xrResetTransformUIController.SetLoaderImage(false, 0f);

            XRManager.Instance.ResetXROriginTransform();
            XRManager.Instance.ResetDataRendererTransform();

            holdCoroutine = null;
        }

        private void OnRightSecondaryButtonPressed()
        {
            XRManager.Instance.ExitVR();
        }

    }

}
