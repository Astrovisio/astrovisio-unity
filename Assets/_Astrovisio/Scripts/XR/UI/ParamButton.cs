using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{

    public class ParamButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
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
        public Setting Setting { get; private set; }

        private void Start()
        {
            SetButtonState(false);
        }

        public void InitButtonSetting(string name, Setting setting, Action onButtonClicked)
        {
            button.onClick.RemoveAllListeners();
            SetButtonState(false);

            labelTMP.text = name;
            Name = name;
            Setting = setting;

            button.onClick.AddListener(() =>
            {
                SetButtonState(!State);
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

    }

}
