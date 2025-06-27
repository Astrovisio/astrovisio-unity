using UnityEngine;
using UnityEngine.InputSystem;

public class TransformManipulator : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionProperty leftGripAction;
    public InputActionProperty rightGripAction;

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
    private float initialDistance;
    private float initialAngleY;

    private bool initializedSingleGrip = false;
    private bool initializedDualGrip = false;

    void OnEnable()
    {
        leftGripAction.action.Enable();
        rightGripAction.action.Enable();
    }

    void OnDisable()
    {
        leftGripAction.action.Disable();
        rightGripAction.action.Disable();
    }

    void Update()
    {
        isLeftGripping = leftGripAction.action.ReadValue<float>() > 0.5f;
        isRightGripping = rightGripAction.action.ReadValue<float>() > 0.5f;

        if (isLeftGripping && isRightGripping)
        {
            if (!initializedDualGrip)
            {
                InitDualGrip();
            }
            HandleScaleAndRotation();
        }
        else if (isLeftGripping || isRightGripping)
        {
            if (!initializedSingleGrip)
            {
                InitSingleGrip();
            }
            HandleTranslation(isLeftGripping ? leftController : rightController, isLeftGripping ? initialLeftPos : initialRightPos);
        }
        else
        {
            initializedSingleGrip = false;
            initializedDualGrip = false;
        }

        UpdateLines();
    }

    void InitSingleGrip()
    {
        initializedSingleGrip = true;
        initialObjectPosition = transform.position;
        if (isLeftGripping)
        {
            initialLeftPos = leftController.position;
        }
        else
        {
            initialRightPos = rightController.position;
        }
    }

    void InitDualGrip()
    {
        initializedDualGrip = true;
        initialObjectPosition = transform.position;
        initialObjectRotation = transform.rotation;
        initialObjectScale = transform.localScale;

        initialLeftPos = leftController.position;
        initialRightPos = rightController.position;

        initialDistance = Vector3.Distance(initialLeftPos, initialRightPos);
        initialAngleY = Mathf.Atan2(
            initialRightPos.x - initialLeftPos.x,
            initialRightPos.z - initialLeftPos.z
        ) * Mathf.Rad2Deg;
    }

    void HandleTranslation(Transform controller, Vector3 initialControllerPos)
    {
        Vector3 delta = controller.position - initialControllerPos;
        transform.position = initialObjectPosition + delta;
    }

    void HandleScaleAndRotation()
    {
        Vector3 leftPos = leftController.position;
        Vector3 rightPos = rightController.position;

        // --- Scale ---
        float currentDistance = Vector3.Distance(leftPos, rightPos);
        float scaleFactor = currentDistance / initialDistance;
        transform.localScale = initialObjectScale * scaleFactor;

        // --- Rotation around Y ---
        float currentAngleY = Mathf.Atan2(
            rightPos.x - leftPos.x,
            rightPos.z - leftPos.z
        ) * Mathf.Rad2Deg;

        float deltaAngleY = currentAngleY - initialAngleY;
        Quaternion deltaRotation = Quaternion.Euler(0, deltaAngleY, 0);
        transform.rotation = deltaRotation * initialObjectRotation;

        // --- Optional: Update position (center between controllers)
        Vector3 currentMidpoint = (leftPos + rightPos) / 2f;
        Vector3 initialMidpoint = (initialLeftPos + initialRightPos) / 2f;
        Vector3 deltaMid = currentMidpoint - initialMidpoint;
        transform.position = initialObjectPosition + deltaMid;
    }

    void UpdateLines()
    {
        lineBetweenControllers.enabled = false;
        lineLeftToObject.enabled = false;
        lineRightToObject.enabled = false;
        lineTranslateToObject.enabled = false;

        Vector3 objectCenter = transform.position;

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
