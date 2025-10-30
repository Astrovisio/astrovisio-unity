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

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ToastMessageController
    {
        public VisualElement Root { get; }

        private VisualElement successToastMessage;
        private VisualElement errorToastMessage;

        public ToastMessageController(VisualElement root)
        {
            Root = root;
            Root.pickingMode = PickingMode.Ignore;

            successToastMessage = Root.Q<VisualElement>("ContainerSuccess");
            errorToastMessage = Root.Q<VisualElement>("ContainerError");
        }

        public void SetToastSuccessMessage(string message)
        {
            // Debug.Log($"SetToastSuccessMessage: {message}");
            Label successToastMessageLabel = successToastMessage.Q<Label>();
            successToastMessageLabel.text = message;
            successToastMessage.AddToClassList("active");
            RemoveClassAfterDelay(successToastMessage, 4);
        }

        public void SetToastErrorMessage(string message)
        {
            // Debug.Log($"SetToastErrorMessage: {message}");
            Label errorToastMessageLabel = errorToastMessage.Q<Label>();
            errorToastMessageLabel.text = message;
            errorToastMessage.AddToClassList("active");
            RemoveClassAfterDelay(errorToastMessage, 4);
        }

        private async void RemoveClassAfterDelay(VisualElement element, int seconds)
        {
            await Task.Delay(seconds * 1000);
            element.RemoveFromClassList("active");
        }

    }

}
