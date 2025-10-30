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

using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;


namespace Astrovisio
{

    public class BuildIncrementor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Increment global bundleVersion (e.g., "0.0.12" -> "0.0.13")
            string oldVersion = PlayerSettings.bundleVersion;
            string newVersion = IncrementVersionString(oldVersion);
            PlayerSettings.bundleVersion = newVersion;

            Debug.Log($"[BuildIncrementor] Set bundleVersion: {oldVersion} -> {newVersion}");
        }

        /// <summary>
        /// Increments the last numeric group in a semantic-like version string.
        /// Examples:
        ///  "0.0.12" -> "0.0.13"
        ///  "1.2"    -> "1.3"
        ///  "1"      -> "2"
        ///  "1.2.3-beta" -> "1.2.4-beta" (keeps suffix)
        ///  "" or null -> "0.0.1"
        /// </summary>
        private static string IncrementVersionString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "0.0.1";

            // Capture optional prefix parts, last numeric, and optional suffix
            // Groups:
            //  1: prefix including dots before the last number (may be empty)
            //  2: the last number
            //  3: any trailing text after the last number (e.g., "-beta", "+build")
            var rx = new Regex(@"^(.*?)(\d+)([^0-9]*)$", RegexOptions.Singleline);
            var m = rx.Match(input.Trim());

            if (!m.Success)
            {
                // If no trailing number is found, append ".1"
                return input.Trim().Length > 0 ? input.Trim() + ".1" : "0.0.1";
            }

            string prefix = m.Groups[1].Value; // may include dots
            string numberStr = m.Groups[2].Value;
            string suffix = m.Groups[3].Value; // e.g. "-beta"

            if (!int.TryParse(numberStr, out int n))
                n = 0;

            n++;

            return $"{prefix}{n}{suffix}";
        }

        // Optional: a menu item to test increment without running a full build
        [MenuItem("Tools/Versioning/Increment bundleVersion (Patch)")]
        private static void MenuIncrementBundleVersion()
        {
            string oldVersion = PlayerSettings.bundleVersion;
            string newVersion = IncrementVersionString(oldVersion);
            PlayerSettings.bundleVersion = newVersion;
            Debug.Log($"[BuildIncrementor] Manual increment: {oldVersion} -> {newVersion}");
        }

    }

}
