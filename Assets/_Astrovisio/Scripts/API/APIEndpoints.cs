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

namespace Astrovisio
{
    public static class APIEndpoints
    {

        public static string BaseUrl = "http://localhost:8000";

        // Default
        public static string GetHealth() => $"{BaseUrl}/api/health/";

        // Projects
        public static string GetProjects() => $"{BaseUrl}/api/projects/";
        public static string GetProject(int projectId) => $"{BaseUrl}/api/projects/{projectId}";
        public static string CreateProject() => $"{BaseUrl}/api/projects/";
        public static string UpdateProject(int projectId) => $"{BaseUrl}/api/projects/{projectId}";
        public static string DeleteProject(int projectId) => $"{BaseUrl}/api/projects/{projectId}";
        public static string DuplicateProject(int projectId) => $"{BaseUrl}/api/projects/{projectId}/duplicate";

        // Files
        public static string UpdateProjectFiles(int projectId) => $"{BaseUrl}/api/projects/{projectId}/files";
        public static string GetFile(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}";
        public static string UpdateFile(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}";
        public static string ProcessFile(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}/process";
        public static string GetProcessedFile(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}/process";
        public static string GetHistogram(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}/histos";

        // Renderer
        public static string GetSettings(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}/render";
        public static string UpdateSettings(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}/render";

        // Jobs
        public static string GetJobProgress(int jobId) => $"{BaseUrl}/api/jobs/{jobId}/progress";
        public static string GetJobResult(int jobId) => $"{BaseUrl}/api/jobs/{jobId}/result";

    }

}
