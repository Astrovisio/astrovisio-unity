namespace Astrovisio
{
    public static class APIEndpoints
    {

        public static string BaseUrl = "http://localhost:8000";

        // Default
        public static string GetHealth() => $"{BaseUrl}/api/health/";

        // Projects
        public static string GetProjects() => $"{BaseUrl}/api/projects/";
        public static string GetProject(int projectID) => $"{BaseUrl}/api/projects/{projectID}";
        public static string CreateProject() => $"{BaseUrl}/api/projects/";
        public static string UpdateProject(int projectID) => $"{BaseUrl}/api/projects/{projectID}";
        public static string DeleteProject(int projectID) => $"{BaseUrl}/api/projects/{projectID}";
        public static string DuplicateProject(int projectID) => $"{BaseUrl}/api/projects/{projectID}/duplicate";
        public static string UpdateProjectFiles(int projectID) => $"{BaseUrl}/api/projects/{projectID}/files";
        public static string GetFile(int projectID, int fileID) => $"{BaseUrl}/api/projects/{projectID}/file/{fileID}";
        public static string UpdateFile(int projectID, int fileID) => $"{BaseUrl}/api/projects/{projectID}/file/{fileID}";
        public static string GetProcessedProject(int projectID, int fileID) => $"{BaseUrl}/api/projects/{projectID}/file/{fileID}/process";
        public static string ProcessProject(int projectID, int fileID) => $"{BaseUrl}/api/projects/{projectID}/file/{fileID}/process";

        // Jobs
        public static string GetProjectJobStatus(int projectID, int jobID) => $"{BaseUrl}/api/projects/{projectID}/process/{jobID}/progress";
        public static string FetchProjectProcessedData(int projectID, int jobID) => $"{BaseUrl}/api/projects/{projectID}/process/{jobID}/result";

    }

}
