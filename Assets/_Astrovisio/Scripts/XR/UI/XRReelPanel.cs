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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRReelPanel : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI labelTMP;
        private ProjectManager projectManager;

        private void Start()
        {
            closeButton.onClick.AddListener(HandleCloseButton);

            projectManager = FindAnyObjectByType<ProjectManager>();

            prevButton.onClick.AddListener(OnPrevClick);
            nextButton.onClick.AddListener(OnNextClick);

            UpdateUI();
        }

        private void OnEnable()
        {
            projectManager = FindAnyObjectByType<ProjectManager>();
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(HandleCloseButton);
            prevButton.onClick.RemoveListener(OnPrevClick);
            nextButton.onClick.RemoveListener(OnNextClick);
        }

        private void HandleCloseButton()
        {
            Destroy(transform.parent.parent.gameObject, 0.1f);
        }

        [ContextMenu("Update")]
        public void UpdateUI()
        {
            if (projectManager == null || ReelManager.Instance == null)
            {
                if (labelTMP != null)
                {
                    labelTMP.text = "—";
                }
                return;
            }

            Project project = projectManager.GetCurrentProject();
            if (project == null)
            {
                if (labelTMP != null)
                {
                    labelTMP.text = "—";
                }
                return;
            }

            File file = ReelManager.Instance.GetReelCurrentFile(project.Id);
            if (labelTMP != null)
            {
                labelTMP.text = file?.Name ?? "—";
            }
        }

        private Project GetCurrentProjectId()
        {
            if (projectManager == null || ReelManager.Instance == null)
            {
                return null;
            }

            Project project = projectManager.GetCurrentProject();
            if (project == null)
            {
                return null;
            }

            return project;
        }

        private void OnPrevClick()
        {
            Project project = GetCurrentProjectId();
            if (project == null)
            {
                return;
            }

            CaptureDataTransform();

            RenderManager.Instance.RenderReelPrev(project.Id);
            int? currentFileId = RenderManager.Instance.GetReelCurrentFileId(project.Id);
            if (currentFileId.HasValue == false)
            {
                return;
            }

            RestoreDataTransform();

            SettingsManager.Instance.SetSettings(project.Id, currentFileId.Value);
            UpdateUI();
        }

        private void OnNextClick()
        {
            Project project = GetCurrentProjectId();
            if (project == null)
            {
                return;
            }

            CaptureDataTransform();

            RenderManager.Instance.RenderReelNext(project.Id);
            int? currentFileId = RenderManager.Instance.GetReelCurrentFileId(project.Id);
            if (currentFileId.HasValue == false)
            {
                return;
            }

            RestoreDataTransform();

            SettingsManager.Instance.SetSettings(project.Id, currentFileId.Value);
            UpdateUI();
        }

        private Transform dataTransform;

        private void CaptureDataTransform()
        {
            dataTransform = RenderManager.Instance.DataRenderer.GetAstrovidioDataSetRenderer().transform;
        }

        private void RestoreDataTransform()
        {
            RenderManager.Instance.DataRenderer.GetAstrovidioDataSetRenderer().transform.SetPositionAndRotation(dataTransform.position, dataTransform.rotation);
            RenderManager.Instance.DataRenderer.GetAstrovidioDataSetRenderer().transform.localScale = dataTransform.localScale;
        }

    }

}
