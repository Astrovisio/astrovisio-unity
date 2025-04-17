namespace Astrovisio
{
    public static class APIEndpoints
    {
        public static string BaseUrl = "http://localhost:8000";

        public static string GetAllProjects() => $"{BaseUrl}/api/projects/";
        public static string GetProjectById(int id) => $"{BaseUrl}/api/projects/{id}";
        public static string CreateProject() => $"{BaseUrl}/api/projects/";
        public static string UpdateProject(int id) => $"{BaseUrl}/api/projects/{id}";
        public static string DeleteProject(int id) => $"{BaseUrl}/api/projects/{id}";
        public static string ProcessProject(int id) => $"{BaseUrl}/api/projects/{id}/process";
    }
}
