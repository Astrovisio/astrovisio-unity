using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class OrbitCameraController : MonoBehaviour
{
    [Header("Target & Distance")]
    public Transform target;
    public float minDistance = 2.0f;
    public float maxDistance = 50.0f;

    [Header("Speed Settings")]
    public float rotationSpeed = 5.0f;
    public float panSpeed = 0.5f;
    public float zoomSpeed = 2.0f;

    [Header("Damping")]
    public float rotationDamping = 0.1f;
    public float zoomDamping = 0.1f;


    private Vector3 desiredRotation;
    private Vector3 currentRotation;
    private Vector3 rotationVelocity;


    private float desiredDistance;
    private float currentDistance;

    // UI
    private bool isInteractingWithUI = false;
    private VisualElement rootVisualElement;

    private void Start()
    {
        if (target == null)
        {
            GameObject go = new GameObject("OrbitTarget");
            target = go.transform;
        }
        target.position = Vector3.zero;

        desiredDistance = Vector3.Distance(transform.position, target.position);
        currentDistance = desiredDistance;

        Vector3 offset = transform.position - target.position;
        Quaternion initialRotation = Quaternion.LookRotation(-offset);
        desiredRotation = initialRotation.eulerAngles;
        currentRotation = initialRotation.eulerAngles;

        var uiDocument = FindFirstObjectByType<UIDocument>();
        if (uiDocument != null)
        {
            rootVisualElement = uiDocument.rootVisualElement;
        }
    }

    private bool IsInteractingWithUI()
    {
        isInteractingWithUI = IsPointerOverVisibleUI();
        return isInteractingWithUI;
    }


    void LateUpdate()
    {

        if (IsInteractingWithUI())
        {
            return;
        }

        // --- Zoom ---
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.001f)
        {
            desiredDistance -= scrollInput * zoomSpeed * desiredDistance;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime / zoomDamping);

        // --- Rotazione (Orbit) ---
        if (Input.GetMouseButton(0))
        {
            desiredRotation.y += Input.GetAxis("Mouse X") * rotationSpeed;
            desiredRotation.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
            desiredRotation.x = Mathf.Clamp(desiredRotation.x, -90f, 90f);
        }
        currentRotation = Vector3.SmoothDamp(currentRotation, desiredRotation, ref rotationVelocity, rotationDamping);
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

        // --- Pan ---
        if (Input.GetMouseButton(1))
        {
            Vector3 panInput = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0);
            Vector3 panDelta = (rotation * panInput) * panSpeed * (currentDistance / maxDistance);
            target.position += panDelta;
        }

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        Vector3 position = rotation * negDistance + target.position;

        transform.position = position;
        transform.rotation = rotation;
    }

    private bool IsPointerOverVisibleUI()
    {
        if (rootVisualElement == null)
        {
            Debug.Log("rootVisualElement is null");
            return false;
        }

        // LogUITreeUnderMouse();

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 uiPosition = RuntimePanelUtils.ScreenToPanel(rootVisualElement.panel, mousePosition);
        VisualElement picked = rootVisualElement.panel.Pick(uiPosition);

        if (picked == null)
        {
            // Debug.Log("Nessun elemento UI sotto il mouse");
            return false;
        }

        // Debug.Log($"Elemento UI sotto mouse: {picked.name}, visibility: {picked.resolvedStyle.visibility}, display: {picked.resolvedStyle.display}, pickingMode: {picked.pickingMode}");

        while (picked != null)
        {
            if (picked.resolvedStyle.visibility == Visibility.Visible)
                return true;

            picked = picked.parent;
        }

        return false;
    }

    private void LogUITreeUnderMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 uiPosition = RuntimePanelUtils.ScreenToPanel(rootVisualElement.panel, mousePosition);

        Debug.Log($"Mouse Screen Position: {mousePosition}, UI Position: {uiPosition}");

        var picked = rootVisualElement.panel.Pick(uiPosition);
        if (picked == null)
        {
            Debug.Log("UI Pick: null");
            return;
        }

        Debug.Log($"[Picked] name={picked.name}, class={picked.GetType().Name}, pickingMode={picked.pickingMode}, visible={picked.resolvedStyle.visibility}");

        VisualElement current = picked;
        while (current != null)
        {
            Debug.Log($"  > Parent: {current.name} - pickingMode: {current.pickingMode} - display: {current.resolvedStyle.display} - visible: {current.resolvedStyle.visibility}");
            current = current.parent;
        }
    }

    private bool IsAnyVisibleUIElementUnderMouse()
    {
        var mousePos = Mouse.current.position.ReadValue();
        var panelPos = RuntimePanelUtils.ScreenToPanel(rootVisualElement.panel, mousePos);

        List<VisualElement> hits = new();
        rootVisualElement.panel.PickAll(panelPos, hits);

        foreach (var el in hits)
        {
            Debug.Log($"[Hit] {el.name} picking: {el.pickingMode}, visible: {el.resolvedStyle.visibility}");
            if (el.resolvedStyle.visibility == Visibility.Visible && el.pickingMode != PickingMode.Ignore)
                return true;
        }
        return false;
    }


}
