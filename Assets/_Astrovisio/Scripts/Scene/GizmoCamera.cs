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

using UnityEngine;

public class GizmoCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;


    private float fixedDistance;
    private OrbitCameraController orbitCamera;


    private void Start()
    {
        orbitCamera = FindFirstObjectByType<OrbitCameraController>();
        if (orbitCamera == null)
        {
            Debug.LogError("OrbitCameraController not found.");
            return;
        }

        if (target != null && orbitCamera.target != null)
        {
            Vector3 initialDir = orbitCamera.transform.position - orbitCamera.target.position;
            fixedDistance = initialDir.magnitude;
        }
    }

    private void LateUpdate()
    {
        if (orbitCamera != null && target != null && orbitCamera.target != null)
        {
            Vector3 dir = (orbitCamera.transform.position - orbitCamera.target.position).normalized;
            transform.position = target.position + dir * fixedDistance;
            transform.rotation = orbitCamera.transform.rotation;
        }
    }


}
