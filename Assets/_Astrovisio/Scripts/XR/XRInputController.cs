using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Astrovisio
{
    public class XRInputController : MonoBehaviour
    {
        [Header("Dependecies")]
        [SerializeField] private VRManager vrManager;

        [Header("Controller References")]
        [SerializeField] private Transform leftController;
        [SerializeField] private Transform rightController;

        [Header("Poke Point References")]
        [SerializeField] private Transform leftPokePoint;
        [SerializeField] private Transform rightPokePoint;

        [Header("Target to Manipulate")]
        [SerializeField] private Transform target;

        [Header("Options")]
        [SerializeField] private bool enableScaling = true;
        [SerializeField] private bool inPlaceScaling = true;
        [SerializeField] private float rotationCutoff = 5f;

        [Header("Input Bindings")]
        [SerializeField] private InputActionReference leftPrimaryButton;
        [SerializeField] private InputActionReference leftSecondaryButton;
        [SerializeField] private InputActionReference leftMenuButton;
        [SerializeField] private InputActionReference leftTrigger;
        [SerializeField] private InputActionReference leftGrip;
        [SerializeField] private InputActionReference rightPrimaryButton;
        [SerializeField] private InputActionReference rightSecondaryButton;
        [SerializeField] private InputActionReference rightMenuButton;
        [SerializeField] private InputActionReference rightTrigger;
        [SerializeField] private InputActionReference rightGrip;
        [SerializeField] private MenuPanelUI menuPanelUI;


        private XRScaleRotateController scaleRotateController;
        private bool isScaling = false;


        private void Start()
        {
            if (target != null)
            {
                scaleRotateController = new XRScaleRotateController(
                    target,
                    target.localScale.magnitude,
                    rotationCutoff,
                    inPlaceScaling,
                    enableScaling
                );
            }

            leftPrimaryButton.action.started += OnLeftPrimaryButtonStarted;
            leftSecondaryButton.action.started += OnLeftSecondaryButtonStarted;
            leftMenuButton.action.started += OnLeftMenuButton;
            leftTrigger.action.started += OnLeftTrigger;
            leftGrip.action.started += OnLeftGrip;
            rightPrimaryButton.action.started += OnRightPrimaryButtonStarted;
            rightSecondaryButton.action.started += OnRightSecondaryButtonStarted;
            rightMenuButton.action.started += OnRightMenuButton;
            rightTrigger.action.started += OnRightTrigger;
            rightGrip.action.started += OnRightGrip;
        }

        private void Update()
        {

            if (BothHandsActive())
            {
                Debug.Log("OK");
                if (!isScaling)
                {
                    scaleRotateController.Begin(leftController, rightController);
                    isScaling = true;
                }

                scaleRotateController.Update(leftController, rightController);
            }
            else
            {
                isScaling = false;
            }
        }

        private void DebugInput()
        {
            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "JoystickButton" + i)))
                {
                    Debug.Log("Joystick Button " + i + " pressed");
                }
            }
        }

        public Transform GetLeftPokePoint()
        {
            return leftPokePoint;
        }

        public Transform GetRightPokePoint()
        {
            return rightPokePoint;
        }

        private bool BothHandsActive()
        {
            return Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.H);
        }

        private void OnLeftPrimaryButtonStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnLeftPrimaryButtonStarted");
        }

        private void OnLeftSecondaryButtonStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnLeftSecondaryButtonStarted");
        }

        private void OnLeftMenuButton(InputAction.CallbackContext context)
        {
            Debug.Log("OnLeftMenuButton");
            menuPanelUI.TogglePanel();
        }

        private void OnLeftTrigger(InputAction.CallbackContext context)
        {
            Debug.Log("OnLeftTrigger");
        }

        private void OnLeftGrip(InputAction.CallbackContext context)
        {
            Debug.Log("OnLeftGrip");
        }

        private void OnRightPrimaryButtonStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnRightPrimaryButtonStarted");
        }

        private void OnRightSecondaryButtonStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnRightSecondaryButtonStarted");
            Debug.Log("[VRManager] B button pressed. Exiting VR...");
            vrManager.ExitVR();
        }

        private void OnRightMenuButton(InputAction.CallbackContext context)
        {
            Debug.Log("OnRightMenuButton");
        }

        private void OnRightTrigger(InputAction.CallbackContext context)
        {
            Debug.Log("OnRightTrigger");
        }

        private void OnRightGrip(InputAction.CallbackContext context)
        {
            Debug.Log("OnRightGrip");
        }

    }
}
