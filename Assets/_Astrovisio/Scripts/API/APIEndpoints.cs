namespace Astrovisio
{
    public static class APIEndpoints
    {

        public static string BaseUrl = "http://localhost:8000";

        public static string GetAllProjects() => $"{BaseUrl}/api/projects/";
        public static string GetProjectById(int projectID) => $"{BaseUrl}/api/projects/{projectID}";
        public static string CreateProject() => $"{BaseUrl}/api/projects/";
        public static string UpdateProject(int projectID) => $"{BaseUrl}/api/projects/{projectID}";
        public static string DeleteProject(int projectID) => $"{BaseUrl}/api/projects/{projectID}";
        public static string ProcessProject(int projectID) => $"{BaseUrl}/api/projects/{projectID}/process";
        public static string GetProjectJobStatus(int projectID, int jobID) => $"{BaseUrl}/api/projects/{projectID}/process/{jobID}/progress";
        public static string FetchProjectProcessedData(int projectID, int jobID) => $"{BaseUrl}/api/projects/{projectID}/process/{jobID}/result";

    }

}
