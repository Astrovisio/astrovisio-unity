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

using UnityEngine;

namespace Astrovisio
{

    public class Gizmo : MonoBehaviour
    {
        [SerializeField] private Canvas xPosCanvas;
        [SerializeField] private Canvas yPosCanvas;
        [SerializeField] private Canvas zPosCanvas;
        [SerializeField] private Canvas xNegCanvas;
        [SerializeField] private Canvas yNegCanvas;
        [SerializeField] private Canvas zNegCanvas;

        [SerializeField] private Camera gizmoCamera;

        private void Update()
        {
            if (gizmoCamera == null)
            {
                return;
            }

            BillboardToCamera(xPosCanvas);
            BillboardToCamera(yPosCanvas);
            BillboardToCamera(zPosCanvas);
            BillboardToCamera(xNegCanvas);
            BillboardToCamera(yNegCanvas);
            BillboardToCamera(zNegCanvas);
        }

        private void BillboardToCamera(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            Transform t = canvas.transform;

            t.LookAt(
                t.position + gizmoCamera.transform.rotation * Vector3.forward,
                gizmoCamera.transform.rotation * Vector3.up
            );

        }

    }

}