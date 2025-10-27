/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
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

    public class ParamButton : MonoBehaviour
    {
        [SerializeField] private bool isAxis;
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite colormapSprite;
        [SerializeField] private Sprite opacitySprite;
        [SerializeField] private TextMeshProUGUI labelTMP;

        [Header("Background")]
        [SerializeField] private Color normalButtonColorBackground;
        [SerializeField] private Color activeButtonColorBackground;

        [Header("Label")]
        [SerializeField] private Color normalButtonColorLabel;
        [SerializeField] private Color activeButtonColorLabel;

        [Header("Icon")]
        [SerializeField] private Color normalButtonColorIcon;
        [SerializeField] private Color activeButtonColorIcon;

        // Events
        public event Action<ParamButton> OnParamButtonClicked;

        // Local
        public bool State { get; private set; }
        public string Name { get; private set; }

        private void Start()
        {
            SetButtonState(false);
            SetButtonIcon(null);
        }

        public void InitButtonSetting(string name, Action onButtonClicked)
        {
            button.onClick.RemoveAllListeners();
            SetButtonState(false);

            labelTMP.text = name;
            Name = name;

            button.onClick.AddListener(() =>
            {
                OnParamButtonClicked?.Invoke(this);
                onButtonClicked?.Invoke();
            });
        }

        public void SetButtonState(bool state)
        {
            State = state;

            if (backgroundImage != null)
            {
                backgroundImage.color = state ? activeButtonColorBackground : normalButtonColorBackground;
            }

            if (iconImage != null)
            {
                iconImage.color = state ? activeButtonColorIcon : normalButtonColorIcon;
            }

            if (labelTMP != null)
            {
                labelTMP.color = state ? activeButtonColorLabel : normalButtonColorLabel;
            }
        }

        public void SetButtonIcon(string value)
        {
            switch (value)
            {
                case null:
                    iconImage.gameObject.SetActive(false);
                    iconImage.sprite = null;
                    break;
                case "Opacity":
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = opacitySprite;
                    break;
                case "Colormap":
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = colormapSprite;
                    break;
            }
        }

        public bool GetButtonState()
        {
            return State;
        }

    }

}
