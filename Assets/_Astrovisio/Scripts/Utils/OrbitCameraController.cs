using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class OrbitCameraController : MonoBehaviour
{
    [Header("Target & Distance")]
    public Transform target;            // Target attorno al quale orbitare (inizialmente a (0,0,0))
    public float minDistance = 2.0f;      // Distanza minima per lo zoom
    public float maxDistance = 50.0f;     // Distanza massima per lo zoom

    [Header("Speed Settings")]
    public float rotationSpeed = 5.0f;    // Velocità di rotazione
    public float panSpeed = 0.5f;         // Velocità di pan
    public float zoomSpeed = 2.0f;        // Velocità di zoom

    [Header("Damping")]
    public float rotationDamping = 0.1f;  // Smoothing per la rotazione
    public float zoomDamping = 0.1f;      // Smoothing per lo zoom

    // Variabili interne per l'interpolazione della rotazione
    private Vector3 desiredRotation;
    private Vector3 currentRotation;
    private Vector3 rotationVelocity;

    // Variabili per la distanza
    private float desiredDistance;
    private float currentDistance;

    // UI
    private bool isInteractingWithUI = false;
    private VisualElement rootVisualElement;

    private void Start()
    {
        // Se non è assegnato un target, lo creiamo e lo posizioniamo in (0,0,0)
        if (target == null)
        {
            GameObject go = new GameObject("OrbitTarget");
            target = go.transform;
        }
        target.position = Vector3.zero;

        // Calcola la distanza iniziale dalla camera al target
        desiredDistance = Vector3.Distance(transform.position, target.position);
        currentDistance = desiredDistance;

        // Calcola la rotazione iniziale basata sull'offset tra la camera e il target
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

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isInteractingWithUI = IsPointerOverVisibleUI();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isInteractingWithUI = false;
        }

        if (isInteractingWithUI)
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
        if (Input.GetMouseButton(0)) // Tasto sinistro per orbitare
        {
            desiredRotation.y += Input.GetAxis("Mouse X") * rotationSpeed;
            desiredRotation.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
            desiredRotation.x = Mathf.Clamp(desiredRotation.x, -90f, 90f);
        }
        currentRotation = Vector3.SmoothDamp(currentRotation, desiredRotation, ref rotationVelocity, rotationDamping);
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

        // --- Pan ---
        if (Input.GetMouseButton(1)) // Tasto destro per pan
        {
            Vector3 panInput = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0);
            // Calcola lo spostamento in base alla rotazione attuale della camera e alla distanza corrente
            Vector3 panDelta = (rotation * panInput) * panSpeed * (currentDistance / maxDistance);
            // Aggiorna direttamente la posizione del target
            target.position += panDelta;
        }

        // --- Calcolo della posizione finale della camera ---
        // La posizione della camera è data dalla rotazione applicata a un offset negativo (in base alla distanza)
        // sommata alla posizione del target (che può essere stata modificata dal pan)
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
