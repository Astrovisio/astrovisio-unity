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
        [SerializeField] private XRMenuUIController xrMenuUIController;
        [SerializeField] private XRDataInfoUIController xrDataInfoUIController;
        [SerializeField] private XRResetTransformUIController xrResetTransformUIController;
        [SerializeField] private XRHelpUIController xrHelpUIController;


        // Reset position
        private Coroutine holdCoroutine;
        private float holdDuration = 1f;
        private float minHoldDuration = 0.15f;


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
            // Left Pressed
            xrInputController.OnLeftMenuButtonPressed += OnLeftMenuButtonPressed;
            xrInputController.OnLeftPrimaryButtonPressed += OnLeftPrimaryButtonPressed;
            xrInputController.OnLeftSecondaryButtonPressed += OnLeftSecondaryButtonPressed;

            // Left Released
            xrInputController.OnLeftSecondaryButtonReleased += OnLeftSecondaryButtonReleased;

            // Right Pressed
            xrInputController.OnRightPrimaryButtonPressed += OnRightPrimaryButtonPressed;
            xrInputController.OnRightSecondaryButtonPressed += OnRightSecondaryButtonPressed;

            // Right Released
            xrInputController.OnRightPrimaryButtonReleased += OnRightPrimaryButtonReleased;
            xrInputController.OnRightSecondaryButtonReleased += OnRightSecondaryButtonReleased;
        }

        private void OnDisable()
        {
            // Left Pressed
            xrInputController.OnLeftMenuButtonPressed -= OnLeftMenuButtonPressed;
            xrInputController.OnLeftPrimaryButtonPressed -= OnLeftPrimaryButtonPressed;
            xrInputController.OnLeftSecondaryButtonPressed -= OnLeftSecondaryButtonPressed;

            // Left Released
            xrInputController.OnLeftSecondaryButtonReleased -= OnLeftSecondaryButtonReleased;

            // Right Pressed
            xrInputController.OnRightPrimaryButtonPressed -= OnRightPrimaryButtonPressed;
            xrInputController.OnRightSecondaryButtonPressed -= OnRightSecondaryButtonPressed;

            // Right Released
            xrInputController.OnRightPrimaryButtonReleased -= OnRightPrimaryButtonReleased;
            xrInputController.OnRightSecondaryButtonReleased -= OnRightSecondaryButtonReleased;
        }

        private void OnLeftMenuButtonPressed()
        {
            ToggleMenu();
        }

        private void OnLeftPrimaryButtonPressed()
        {
            // ToggleDataInspector();
        }

        private void OnLeftSecondaryButtonPressed()
        {
            // ToggleHelp();
            StartResetPosition();
        }

        private void OnRightPrimaryButtonPressed()
        {
            Debug.Log("START");
            // StartResetPosition();
            ToggleDataInspector();
        }

        private void OnRightSecondaryButtonPressed()
        {
            // ExitVR();
            StartResetPosition();
        }

        private void OnLeftSecondaryButtonReleased()
        {
            StopResetPosition();
        }

        private void OnRightPrimaryButtonReleased()
        {
            Debug.Log("STOP");
        }

        private void OnRightSecondaryButtonReleased()
        {
            StopResetPosition();
        }


        // -----------------------------------------------------------

        private void StartResetPosition()
        {
            if (holdCoroutine == null)
            {
                holdCoroutine = StartCoroutine(HoldRoutine());
            }
        }

        private void StopResetPosition()
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
            yield return new WaitForSeconds(minHoldDuration);

            float elapsed = 0f;
            float adjustedDuration = holdDuration - minHoldDuration;

            xrResetTransformUIController.SetLoaderImage(true, 0f);

            while (elapsed < adjustedDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / adjustedDuration);
                xrResetTransformUIController.SetLoaderImage(true, progress);
                yield return null;
            }

            xrResetTransformUIController.SetLoaderImage(false, 0f);

            XRManager.Instance.ResetXROriginTransform();
            XRManager.Instance.ResetDataRendererTransform();

            holdCoroutine = null;
        }

        private void ToggleMenu()
        {
            xrMenuUIController.TogglePanel();
        }

        private void ToggleHelp()
        {
            xrHelpUIController.ToggleHelp();
        }

        private void ToggleDataInspector()
        {
            xrDataInfoUIController.TogglePanelVisibility();
            RenderManager.Instance.SetDebugSphere(xrDataInfoUIController.GetPanelVisibility());
        }

        private void ExitVR()
        {
            XRManager.Instance.ExitVR();
        }

    }

}
