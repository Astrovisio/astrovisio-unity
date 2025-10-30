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

namespace Astrovisio
{
    public class DataRendererAxisController : MonoBehaviour
    {
        [SerializeField] private Canvas xAxeCanvas;
        [SerializeField] private Canvas yAxeCanvas;
        [SerializeField] private Canvas zAxeCanvas;
        [SerializeField] private Canvas refCanvas;

        private Camera cameraToLookAt;

        private void Start()
        {
            if (cameraToLookAt == null || !cameraToLookAt.isActiveAndEnabled)
            {
                cameraToLookAt = Camera.main;
                if (cameraToLookAt == null || !cameraToLookAt.isActiveAndEnabled)
                {
                    return;
                }
            }
        }

        private void Update()
        {
            // if (cameraToLookAt == null)
            // {
            //     return;
            // }

            // BillboardToCamera(xAxeCanvas);
            // BillboardToCamera(yAxeCanvas);
            // BillboardToCamera(zAxeCanvas);
            // BillboardToCamera(refCanvas);
        }

        private void BillboardToCamera(Canvas canvas)
        {
            if (canvas == null)
                return;

            Transform t = canvas.transform;
            t.LookAt(
                t.position + cameraToLookAt.transform.rotation * Vector3.forward,
                cameraToLookAt.transform.rotation * Vector3.up
            );
        }

    }
    
}
