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
