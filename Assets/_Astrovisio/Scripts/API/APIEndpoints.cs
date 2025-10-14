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

        // Renderer
        public static string GetSettings(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}/render";
        public static string UpdateSettings(int projectId, int fileId) => $"{BaseUrl}/api/projects/{projectId}/file/{fileId}/render";

        // Jobs
        public static string GetJobProgress(int jobId) => $"{BaseUrl}/api/jobs/{jobId}/progress";
        public static string GetJobResult(int jobId) => $"{BaseUrl}/api/jobs/{jobId}/result";

    }

}
