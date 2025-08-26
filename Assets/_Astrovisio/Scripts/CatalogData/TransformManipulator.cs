using UnityEngine;
using UnityEngine.InputSystem;

public class TransformManipulator : MonoBehaviour
{
    [Header("Target to Manipulate")]
    public Transform targetObject;

    [Header("Input Actions")]
    public InputActionProperty leftGripAction;
    public InputActionProperty rightGripAction;
    public InputActionProperty leftPositionAction;
    public InputActionProperty rightPositionAction;

    [Header("Controller References")]
    public Transform leftController;
    public Transform rightController;

    [Header("Line Renderers")]
    public LineRenderer lineBetweenControllers;
    public LineRenderer lineLeftToObject;
    public LineRenderer lineRightToObject;
    public LineRenderer lineTranslateToObject;

    private bool isLeftGripping;
    private bool isRightGripping;

    private Vector3 initialObjectPosition;
    private Vector3 initialObjectScale;
    private Quaternion initialObjectRotation;

    private Vector3 initialLeftPos;
    private Vector3 initialRightPos;
    private Vector3 initialMidpoint;

    private float initialDistance;
    private float initialAngleY;

    private Vector3 initialTranslateControllerPos;

    private bool initializedSingleGrip = false;
    private bool initializedDualGrip = false;
    private bool wasDualGrippingLastFrame = false;

    void OnEnable()
    {
        leftGripAction.action.Enable();
        rightGripAction.action.Enable();
        leftPositionAction.action.Enable();
        rightPositionAction.action.Enable();
    }

    void OnDisable()
    {
        leftGripAction.action.Disable();
        rightGripAction.action.Disable();
        leftPositionAction.action.Disable();
        rightPositionAction.action.Disable();
    }

    void Update()
    {
        if (targetObject == null) return;

        bool isComputingAreaSelection = targetObject.gameObject.GetComponent<KDTreeComponent>().IsComputingAreaSelection();
        if (isComputingAreaSelection)
        {
            initializedSingleGrip = false;
            initializedDualGrip = false;
            isLeftGripping = false;
            isRightGripping = false;
            UpdateLines();
            return;
        }

        isLeftGripping = leftGripAction.action.ReadValue<float>() > 0.5f;
        isRightGripping = rightGripAction.action.ReadValue<float>() > 0.5f;

        bool isDualGripping = isLeftGripping && isRightGripping;

        if (wasDualGrippingLastFrame && !isDualGripping)
        {
            initializedSingleGrip = false;
        }

        if (isDualGripping)
        {
            if (!initializedDualGrip) InitDualGrip();
            HandleScaleAndRotation();
        }
        else if (isLeftGripping || isRightGripping)
        {
            if (!initializedSingleGrip) InitSingleGrip();
            Transform active = isLeftGripping ? leftController : rightController;
            HandleTranslation(active);
        }
        else
        {
            initializedSingleGrip = false;
            initializedDualGrip = false;
        }

        wasDualGrippingLastFrame = isDualGripping;

        UpdateLines();
    }

    void InitSingleGrip()
    {
        initializedSingleGrip = true;
        initializedDualGrip = false;
        initialObjectPosition = targetObject.position;
        Transform active = isLeftGripping ? leftController : rightController;
        initialTranslateControllerPos = active.position;
    }

    void InitDualGrip()
    {
        initializedDualGrip = true;
        initializedSingleGrip = false;

        initialObjectPosition = targetObject.position;
        initialObjectRotation = targetObject.rotation;
        initialObjectScale = targetObject.localScale;

        initialLeftPos = leftController.position;
        initialRightPos = rightController.position;
        initialMidpoint = (initialLeftPos + initialRightPos) / 2f;

        initialDistance = Vector3.Distance(initialLeftPos, initialRightPos);
        initialAngleY = Mathf.Atan2(
            initialRightPos.x - initialLeftPos.x,
            initialRightPos.z - initialLeftPos.z
        ) * Mathf.Rad2Deg;
    }

    void HandleTranslation(Transform controller)
    {
        Vector3 delta = controller.position - initialTranslateControllerPos;
        targetObject.position = initialObjectPosition + delta;
    }

    void HandleScaleAndRotation()
    {
        Vector3 currentLeft = leftController.position;
        Vector3 currentRight = rightController.position;
        Vector3 currentMid = (currentLeft + currentRight) / 2f;

        float currentDistance = Vector3.Distance(currentLeft, currentRight);
        float scaleFactor = currentDistance / initialDistance;
        targetObject.localScale = initialObjectScale * scaleFactor;

        float currentAngleY = Mathf.Atan2(
            currentRight.x - currentLeft.x,
            currentRight.z - currentLeft.z
        ) * Mathf.Rad2Deg;

        float angleDeltaY = currentAngleY - initialAngleY;
        Quaternion deltaRotY = Quaternion.Euler(0, angleDeltaY, 0);

        Vector3 dirFromPivot = initialObjectPosition - initialMidpoint;
        dirFromPivot = deltaRotY * dirFromPivot;

        targetObject.rotation = deltaRotY * initialObjectRotation;
        targetObject.position = currentMid + dirFromPivot * scaleFactor;
    }

    void UpdateLines()
    {
        lineBetweenControllers.enabled = false;
        lineLeftToObject.enabled = false;
        lineRightToObject.enabled = false;
        lineTranslateToObject.enabled = false;

        if (targetObject == null) return;
        Vector3 objectCenter = targetObject.position;

        if (isLeftGripping && isRightGripping)
        {
            lineBetweenControllers.enabled = true;
            lineBetweenControllers.SetPosition(0, leftController.position);
            lineBetweenControllers.SetPosition(1, rightController.position);

            lineLeftToObject.enabled = true;
            lineLeftToObject.SetPosition(0, leftController.position);
            lineLeftToObject.SetPosition(1, objectCenter);

            lineRightToObject.enabled = true;
            lineRightToObject.SetPosition(0, rightController.position);
            lineRightToObject.SetPosition(1, objectCenter);
        }
        else if (isLeftGripping || isRightGripping)
        {
            Transform active = isLeftGripping ? leftController : rightController;
            lineTranslateToObject.enabled = true;
            lineTranslateToObject.SetPosition(0, active.position);
            lineTranslateToObject.SetPosition(1, objectCenter);
        }
    }
}
