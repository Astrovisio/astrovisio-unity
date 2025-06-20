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
        [SerializeField] private InputActionReference rightPrimaryButton;
        [SerializeField] private InputActionReference rightSecondaryButton;


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
            rightPrimaryButton.action.started += OnRightPrimaryButtonStarted;
            rightSecondaryButton.action.started += OnRightSecondaryButtonStarted;
        }

        private void OnLeftPrimaryButtonStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnLeftPrimaryButtonStarted");
        }

        private void OnLeftSecondaryButtonStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnLeftSecondaryButtonStarted");
        }

        private void OnRightPrimaryButtonStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnRightPrimaryButtonStarted");
        }

        private void OnRightSecondaryButtonStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnRightSecondaryButtonStarted");
        }

        private void Update()
        {
            // Debug.Log($"PrimaryGrip (L): {Input.GetAxis("Oculus_CrossPlatform_PrimaryHandTrigger")}, SecondaryGrip (R): {Input.GetAxis("Oculus_CrossPlatform_SecondaryHandTrigger")}");


            // DebugInput();
            HandleButtonBPressed();

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

        private void HandleButtonBPressed()
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                Debug.Log("[VRManager] B button pressed. Exiting VR...");
                vrManager.ExitVR();
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

    }
}
