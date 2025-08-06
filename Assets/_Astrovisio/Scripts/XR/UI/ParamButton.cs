using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class TestSettings
    {
        public string Name { get; set; }
    }

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
        public event Action<ParamButton> OnButtonClicked;

        // Local
        public bool State { get; private set; }
        public TestSettings settings { get; private set; }

        private void Start()
        {
            SetButtonState(false);

            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    SetButtonState(!State);
                    OnButtonClicked?.Invoke(this);
                });
            }

            settings = new TestSettings
            {
                Name = gameObject.name
            };

            SetSettings(settings);
        }

        public void SetSettings(TestSettings settings)
        {
            labelTMP.text = settings.Name;
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
