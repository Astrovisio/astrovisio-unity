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
using UnityEngine;
using UnityEngine.InputSystem;

namespace Astrovisio
{

    public class XRInputController : MonoBehaviour
    {

        [Header("Poke Point References")]
        [SerializeField] private Transform leftPokePoint;
        [SerializeField] public Transform rightPokePoint;

        [Header("Input Bindings")]
        [SerializeField] public InputActionReference leftPrimaryButton;
        [SerializeField] public InputActionReference leftSecondaryButton;
        [SerializeField] public InputActionReference leftMenuButton;
        [SerializeField] public InputActionReference leftTrigger;
        [SerializeField] public InputActionReference leftGrip;
        [SerializeField] public InputActionReference rightPrimaryButton;
        [SerializeField] public InputActionReference rightSecondaryButton;
        [SerializeField] public InputActionReference rightMenuButton;
        [SerializeField] public InputActionReference rightTrigger;
        [SerializeField] public InputActionReference rightGrip;

        // Events
        public event Action OnLeftPrimaryButtonPressed;
        public event Action OnLeftSecondaryButtonPressed;
        public event Action OnLeftMenuButtonPressed;
        public event Action OnLeftTriggerPressed;
        public event Action OnLeftGripPressed;
        public event Action OnRightPrimaryButtonPressed;
        public event Action OnRightSecondaryButtonPressed;
        public event Action OnRightMenuButtonPressed;
        public event Action OnRightTriggerPressed;
        public event Action OnRightGripPressed;

        public event Action OnLeftSecondaryButtonReleased;
        public event Action OnRightSecondaryButtonReleased;
        public event Action OnRightPrimaryButtonReleased;


        private void OnEnable()
        {
            EnableInputActions();
            SubscribeToInputActions();
        }

        private void OnDisable()
        {
            DisableInputActions();
            UnsubscribeFromInputActions();
        }

        private void EnableInputActions()
        {
            leftPrimaryButton.action.Enable();
            leftSecondaryButton.action.Enable();
            leftMenuButton.action.Enable();
            leftTrigger.action.Enable();
            leftGrip.action.Enable();
            rightPrimaryButton.action.Enable();
            rightSecondaryButton.action.Enable();
            rightMenuButton.action.Enable();
            rightTrigger.action.Enable();
            rightGrip.action.Enable();
        }

        private void DisableInputActions()
        {
            leftPrimaryButton.action.Disable();
            leftSecondaryButton.action.Disable();
            leftMenuButton.action.Disable();
            leftTrigger.action.Disable();
            leftGrip.action.Disable();
            rightPrimaryButton.action.Disable();
            rightSecondaryButton.action.Disable();
            rightMenuButton.action.Disable();
            rightTrigger.action.Disable();
            rightGrip.action.Disable();
        }

        private void SubscribeToInputActions()
        {
            leftPrimaryButton.action.started += OnLeftPrimaryButtonStarted;
            leftSecondaryButton.action.started += OnLeftSecondaryButtonStarted;
            leftMenuButton.action.started += OnLeftMenuButtonStarted;
            leftTrigger.action.started += OnLeftTriggerStarted;
            leftGrip.action.started += OnLeftGripStarted;
            rightPrimaryButton.action.started += OnRightPrimaryButtonStarted;
            rightSecondaryButton.action.started += OnRightSecondaryButtonStarted;
            rightMenuButton.action.started += OnRightMenuButtonStarted;
            rightTrigger.action.started += OnRightTriggerStarted;
            rightGrip.action.started += OnRightGripStarted;

            leftSecondaryButton.action.canceled += OnLeftSecondaryButtonCancelled;
            rightSecondaryButton.action.canceled += OnRightSecondaryButtonCancelled;
            rightPrimaryButton.action.canceled += OnRightPrimaryButtonCancelled;
        }

        private void UnsubscribeFromInputActions()
        {
            leftPrimaryButton.action.started -= OnLeftPrimaryButtonStarted;
            leftSecondaryButton.action.started -= OnLeftSecondaryButtonStarted;
            leftMenuButton.action.started -= OnLeftMenuButtonStarted;
            leftTrigger.action.started -= OnLeftTriggerStarted;
            leftGrip.action.started -= OnLeftGripStarted;
            rightPrimaryButton.action.started -= OnRightPrimaryButtonStarted;
            rightSecondaryButton.action.started -= OnRightSecondaryButtonStarted;
            rightMenuButton.action.started -= OnRightMenuButtonStarted;
            rightTrigger.action.started -= OnRightTriggerStarted;
            rightGrip.action.started -= OnRightGripStarted;

            leftSecondaryButton.action.canceled -= OnLeftSecondaryButtonCancelled;
            rightSecondaryButton.action.canceled -= OnRightSecondaryButtonCancelled;
            rightPrimaryButton.action.canceled -= OnRightPrimaryButtonCancelled;
        }

        public Transform GetLeftPokePoint()
        {
            return leftPokePoint;
        }

        public Transform GetRightPokePoint()
        {
            return rightPokePoint;
        }

        // Started

        private void OnLeftPrimaryButtonStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnLeftPrimaryButtonStarted");
            OnLeftPrimaryButtonPressed?.Invoke();
        }

        private void OnLeftSecondaryButtonStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnLeftSecondaryButtonStarted");
            OnLeftSecondaryButtonPressed?.Invoke();
        }

        private void OnLeftMenuButtonStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnLeftMenuButtonStarted");
            OnLeftMenuButtonPressed?.Invoke();
            // menuPanelUI.TogglePanel();
        }

        private void OnLeftTriggerStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnLeftTriggerStarted");
            OnLeftTriggerPressed?.Invoke();
        }

        private void OnLeftGripStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnLeftGripStarted");
            OnLeftGripPressed?.Invoke();
        }

        private void OnRightPrimaryButtonStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnRightPrimaryButtonStarted");
            OnRightPrimaryButtonPressed?.Invoke();
        }

        private void OnRightSecondaryButtonStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnRightSecondaryButtonStarted");
            OnRightSecondaryButtonPressed?.Invoke();
        }

        private void OnRightMenuButtonStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnRightMenuButtonStarted");
            OnRightMenuButtonPressed?.Invoke();
        }

        private void OnRightTriggerStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnRightTriggerStarted");
            OnRightTriggerPressed?.Invoke();
        }

        private void OnRightGripStarted(InputAction.CallbackContext context)
        {
            // Debug.Log("OnRightGripStarted");
            OnRightGripPressed?.Invoke();
        }

        // Cancelled

        private void OnLeftSecondaryButtonCancelled(InputAction.CallbackContext context)
        {
            // Debug.Log("OnLeftSecondaryButtonCancelled");
            OnLeftSecondaryButtonReleased?.Invoke();
        }

        private void OnRightSecondaryButtonCancelled(InputAction.CallbackContext context)
        {
            // Debug.Log("OnRightSecondaryButtonCancelled");
            OnRightSecondaryButtonReleased?.Invoke();
        }

        private void OnRightPrimaryButtonCancelled(InputAction.CallbackContext context)
        {
            // Debug.Log("OnRightPrimaryButtonCancelled");
            OnRightPrimaryButtonReleased?.Invoke();
        }

    }

}
