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

    public class DebugAPIManager : MonoBehaviour
    {
        public APIManager apiManager;

        public void FetchProjectsDebug()
        {
            if (apiManager == null)
            {
                Debug.LogError("APIManager non assegnato!");
                return;
            }

            Debug.Log("Chiamata API: Get Projects");

            // apiManager.FetchProjects();
        }
        
    }

}
