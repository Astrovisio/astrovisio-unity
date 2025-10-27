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

using System.Collections.Generic;

namespace Astrovisio
{
    public static class ProcessingStatusMessages
    {
        private static readonly Dictionary<string, string> statusMap = new Dictionary<string, string>
        {
            { "sending", "Sending request..." },
            { "processing", "Processing data..." },
            { "loading", "Loading data..." }
        };

        public static string GetClientMessage(string serverStatus)
        {
            if (string.IsNullOrEmpty(serverStatus))
            {
                return "Unknown status";
            }

            if (statusMap.TryGetValue(serverStatus.ToLower(), out string message))
            {
                return message;
            }

            return $"Unknown status: {serverStatus}";
        }
    }

}
