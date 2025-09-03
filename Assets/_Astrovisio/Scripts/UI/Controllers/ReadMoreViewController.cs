using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ReadMoreViewController
    {
        public VisualElement Root { get; }
        public UIManager UIManager { get; }

        private Button closeButton;

        private Label titleLabel;
        private Label descriptionLabel;


        public ReadMoreViewController(VisualElement root, UIManager uiManager)
        {
            Root = root;
            UIManager = uiManager;

            closeButton = Root.Q<Button>("CloseButton");
            closeButton.clicked += Close;

            Init();
        }

        public void Init()
        {
            titleLabel = Root.Q<Label>("TitleLabel");
            descriptionLabel = Root.Q<Label>("DescriptionLabel");
        }

        public void Open(string title, string description)
        {
            Root.AddToClassList("active");
            titleLabel.text = title;
            descriptionLabel.text = description;
        }

        public void Close()
        {
            Root.RemoveFromClassList("active");
        }

    }

}
