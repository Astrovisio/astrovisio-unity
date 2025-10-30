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

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Astrovisio
{
    public static class DebugUtility
    {
        /// <summary>
        /// Logs a (potentially) long string by splitting it into chunks to avoid Unity console truncation.
        /// Pretty-prints JSON when possibile.
        /// </summary>
        public static void LogLong(string header, string text, int chunkSize = 8000)
        {
            InternalLogChunked(header, text, chunkSize, InternalLogType.Log);
        }

        /// <summary>
        /// Warning variant of LogLong.
        /// </summary>
        public static void LogLongWarning(string header, string text, int chunkSize = 8000)
        {
            InternalLogChunked(header, text, chunkSize, InternalLogType.Warning);
        }

        /// <summary>
        /// Error variant of LogLong.
        /// </summary>
        public static void LogLongError(string header, string text, int chunkSize = 8000)
        {
            InternalLogChunked(header, text, chunkSize, InternalLogType.Error);
        }

        /// <summary>
        /// Save the text to a JSON file under persistentDataPath (or a custom directory).
        /// Returns the full path of the written file.
        /// </summary>
        public static string SaveJson(string prefix, string text, bool pretty = true, string directory = null)
        {
            string safePrefix = SanitizeFileName(prefix);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string fileName = $"{safePrefix}_{timestamp}.json";
            string dir = string.IsNullOrEmpty(directory) ? Application.persistentDataPath : directory;

            Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, fileName);
            string toWrite = pretty ? TryPrettifyJson(text) : (text ?? string.Empty);

            System.IO.File.WriteAllText(path, toWrite, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Debug.Log($"[DebugUtility] JSON written to: {path}");

#if UNITY_EDITOR
            // Quality-of-life: copy the path to clipboard in Editor.
            GUIUtility.systemCopyBuffer = path;
#endif
            return path;
        }

        /// <summary>
        /// Pretty-print JSON if valid; otherwise returns the original text.
        /// Handles both objects and arrays.
        /// </summary>
        public static string TryPrettifyJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
            try
            {
                var token = JToken.Parse(text);
                return token.ToString(Formatting.Indented);
            }
            catch
            {
                return text;
            }
        }

        /// <summary>
        /// Helper to log an outgoing JSON API request in full, optionally saving it to disk.
        /// Returns saved file path (or null if not saved).
        /// </summary>
        public static string LogJsonRequest(string method, string url, string json, bool saveToFile = true, int chunkSize = 8000)
        {
            string header = $"[API] {method} {url} - Payload";
            LogLong(header, json, chunkSize);
            return saveToFile ? SaveJson($"{method}_payload", json, true) : null;
        }

        /// <summary>
        /// Helper to log an incoming JSON API response in full, optionally saving it to disk.
        /// Returns saved file path (or null if not saved).
        /// </summary>
        public static string LogJsonResponse(string method, string url, long statusCode, string body, bool saveToFile = true, int chunkSize = 8000)
        {
            string header = $"[API] {method} {url} - Response ({statusCode})";
            LogLong(header, body, chunkSize);
            return saveToFile ? SaveJson($"{method}_response_{statusCode}", body, true) : null;
        }

        // -------------------- internals --------------------

        private enum InternalLogType { Log, Warning, Error }

        private static void InternalLogChunked(string header, string text, int chunkSize, InternalLogType type)
        {
            if (text == null)
            {
                RouteLog(type, $"{header} <NULL>");
                return;
            }

            string printable = TryPrettifyJson(text);
            int len = printable.Length;

            if (len <= chunkSize)
            {
                RouteLog(type, $"{header} (len={len})\n{printable}");
                return;
            }

            int parts = Mathf.CeilToInt(len / (float)chunkSize);
            RouteLog(type, $"{header} (len={len}) — splitting into {parts} chunks…");

            int part = 0;
            for (int i = 0; i < len; i += chunkSize)
            {
                int size = Math.Min(chunkSize, len - i);
                string s = printable.Substring(i, size);
                string msg = $"{header} [part {++part}/{parts}]\n{s}";
                RouteLog(type, msg);
            }
        }

        private static void RouteLog(InternalLogType type, string message)
        {
            switch (type)
            {
                case InternalLogType.Warning: Debug.LogWarning(message); break;
                case InternalLogType.Error: Debug.LogError(message); break;
                default: Debug.Log(message); break;
            }
        }

        private static string SanitizeFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "json";
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }

    }

}
