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

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Threading.Tasks;
using System;

namespace Astrovisio
{
    public class BotManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ProjectManager projectManager;

        [Header("Settings")]
        [SerializeField] private bool removeUploadFileLimit;

        [Header("Auto move to next reel")]
        [SerializeField] private bool isAutoNextReelEnabled;
        [SerializeField] private float interval = 1.5f;

        [Header("Auto process files param names")]
        [SerializeField] private string xParamName = "x";
        [SerializeField] private string yParamName = "y";
        [SerializeField] private string zParamName = "z";

        private void Start()
        {
            StartCoroutine(NextReelRoutine());
        }

        private IEnumerator NextReelRoutine()
        {
            while (true)
            {
                if (isAutoNextReelEnabled)
                {
                    NextReel();
                }

                yield return new WaitForSeconds(interval);
            }
        }

        private void NextReel()
        {
            Project project = projectManager.GetCurrentProject();
            if (project == null) return;

            RenderManager.Instance.RenderReelNext(project.Id);

            File file = ReelManager.Instance.GetReelCurrentFile(project.Id);
            if (file == null) return;

            // Debug.Log("GET SETTINGS");
            // Debug.Log((await SettingsManager.Instance.GetSettings(project.Id, file.Id)).ToString());
            
            SettingsManager.Instance.SetSettings(project.Id, file.Id);
        }

        public async Task AutoProcessFiles()
        {
            Project project = projectManager.GetCurrentProject();
            if (project == null)
            {
                Debug.LogWarning("[BotManager] Current project is null.");
                return;
            }

            foreach (File file in project.Files)
            {
                foreach (Variable variable in file.Variables)
                {
                    variable.Selected = true;

                    variable.XAxis = false;
                    variable.YAxis = false;
                    variable.ZAxis = false;

                    switch (variable.Name)
                    {
                        case var name when name == xParamName:
                            variable.XAxis = true;
                            break;

                        case var name when name == yParamName:
                            variable.YAxis = true;
                            break;

                        case var name when name == zParamName:
                            variable.ZAxis = true;
                            break;

                        default:
                            break;
                    }

                    variable.ThrMinSel = variable.ThrMin;
                    variable.ThrMaxSel = variable.ThrMax;
                }

                await projectManager.UpdateFile(project.Id, file);
                await projectManager.ProcessFile(project.Id, file.Id);
            }

            Debug.Log("[BotManager] AutoProcessFiles completed.");
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(BotManager))]
    public class BotManagerEditor : Editor
    {
        private bool _isRunning;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BotManager botManager = (BotManager)target;

            GUILayout.Space(12);
            GUILayout.Label("Actions", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!Application.isPlaying || _isRunning))
            {
                if (GUILayout.Button(_isRunning ? "Auto Process Files (Running...)" : "Auto Process Files"))
                {
                    _ = RunAutoProcessFiles(botManager);
                }
            }
        }

        private async Task RunAutoProcessFiles(BotManager botManager)
        {
            _isRunning = true;
            try
            {
                await botManager.AutoProcessFiles();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _isRunning = false;
                Repaint();
            }
        }
    }
}
#endif
