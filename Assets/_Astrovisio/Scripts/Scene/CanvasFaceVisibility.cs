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
    [RequireComponent(typeof(Canvas))]
    public class CanvasFaceVisibility : MonoBehaviour
    {
        private Canvas canvas;
        [SerializeField] private Camera cam;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        private void Update()
        {
            if (cam == null)
            {
                return;
            }

            Vector3 toCam = cam.transform.position - canvas.transform.position;

            bool isBack = Vector3.Dot(canvas.transform.forward, toCam) > 0;

            if (isBack)
            {
                transform.Rotate(0f, 180f, 0f, Space.Self);
            }
        }

    }
    
}
