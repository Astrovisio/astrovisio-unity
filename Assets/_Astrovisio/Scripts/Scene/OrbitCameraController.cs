/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
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

using Astrovisio;
using UnityEngine;

public class OrbitCameraController : MonoBehaviour
{
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
        if (uiManager.gameObject.activeSelf && uiManager.HasClickStartedOnUI())
        {
            return;
        }

        // Zoom
        float scrollInput = 0f;
        if (uiManager == null || !uiManager.IsPointerOverVisibleUI())
        {
            scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollInput) > 0.001f)
            {
                desiredDistance -= scrollInput * zoomSpeed * desiredDistance;
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
            }
        }
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime / zoomDamping);

        // Orbit
        if (Input.GetMouseButton(0))
        {
            desiredRotation.y += Input.GetAxis("Mouse X") * rotationSpeed;
            desiredRotation.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
            desiredRotation.x = Mathf.Clamp(desiredRotation.x, -90f, 90f);
        }
        currentRotation = Vector3.SmoothDamp(currentRotation, desiredRotation, ref rotationVelocity, rotationDamping);
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

        // Pan
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
