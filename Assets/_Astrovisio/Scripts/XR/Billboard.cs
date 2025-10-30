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
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private bool m_FlipForward = false;

        private Camera m_Camera;

        private void Awake()
        {
            m_Camera = Camera.main;
        }

        private void Update()
        {
            if (m_Camera == null)
            {
                UpdateCamera();
                if (m_Camera == null)
                    return;
            }

            Vector3 direction = transform.position - m_Camera.transform.position;

            if (m_FlipForward)
            {
                direction = -direction;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }

        private void UpdateCamera()
        {
            m_Camera = Camera.main;
        }

    }

}
