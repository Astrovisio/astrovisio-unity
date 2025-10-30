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
        [SerializeField] private XRMenuPanel xrMenuPanel;
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
            // SetDataInspector(true);
            // StartResetPosition();
            // ToggleDataInspector();
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
            // SetDataInspector(false);
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
            // xrMenuUIController.TogglePanel();
            xrMenuPanel.TogglePanel();
        }

        private void SetDataInspector(bool state)
        {
            if (state)
            {
                RenderManager.Instance.SetDataInspector(true, true);
            }
            else
            {
                RenderManager.Instance.SetDataInspector(false, true);
            }
        }

        private void ToggleHelp()
        {
            xrHelpUIController.ToggleHelp();
        }

        private void ToggleDataInspector()
        {
            // xrDataInfoUIController.TogglePanelVisibility();
        }

        private void ExitVR()
        {
            XRManager.Instance.ExitVR();
        }

    }

}
