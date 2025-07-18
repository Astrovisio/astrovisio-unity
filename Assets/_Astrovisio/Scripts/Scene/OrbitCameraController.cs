using System;
using Astrovisio;
using UnityEngine;

public class OrbitCameraController : MonoBehaviour
{
    // Dependecies
    [SerializeField] private UIManager uiManager;

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

        if (uiManager == null)
        {
            Debug.LogWarning("Missing UIManager.");
        }
    }

    private void LateUpdate()
    {

        if (uiManager.gameObject.activeSelf && uiManager.IsInteractingWithUI())
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

    public void ResetCameraView(Vector3 position, Vector3 rotationEuler, float distance)
    {
        target.position = position;

        desiredRotation = rotationEuler;
        currentRotation = rotationEuler;
        rotationVelocity = Vector3.zero;

        desiredDistance = distance;
        currentDistance = distance;

        Quaternion rotation = Quaternion.Euler(rotationEuler.x, rotationEuler.y, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 cameraPosition = rotation * negDistance + target.position;

        transform.position = cameraPosition;
        transform.rotation = rotation;
    }

}
