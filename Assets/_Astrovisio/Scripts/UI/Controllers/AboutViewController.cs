using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class AboutViewController
    {

        public VisualElement Root { get; }

        private Button closeButton;

        public AboutViewController(VisualElement root)
        {
            Root = root;

            closeButton = Root.Q<Button>("CloseButton");
            closeButton.clicked += Close;

            Init();
        }

        public void Init()
        {
            Label idavieLabel = Root.Q<Label>("iDaVIELabel");

            VisualElement alkemyLogo = Root.Q<VisualElement>("AlkemyLogo");
            VisualElement dgiLogo = Root.Q<VisualElement>("DGILogo");
            VisualElement metaversoLogo = Root.Q<VisualElement>("MetaversoLogo");
            VisualElement scuolaNormaleLogo = Root.Q<VisualElement>("ScuolaNormaleLogo");
            VisualElement idiaLogo = Root.Q<VisualElement>("IDIALogo");
            VisualElement vislabLogo = Root.Q<VisualElement>("VislabLogo");
            VisualElement inafLogo = Root.Q<VisualElement>("INAFLogo");

            AddClickUrl(idavieLabel, "https://github.com/idia-astro/iDaVIE/");
            AddClickUrl(alkemyLogo, "https://www.alkemy.com/");
            AddClickUrl(dgiLogo, "https://www.designgroupitalia.com/");
            AddClickUrl(metaversoLogo, "https://www.metaverso.it/");
            AddClickUrl(scuolaNormaleLogo, "http://cosmology.sns.it/");
            AddClickUrl(idiaLogo, "https://idia.ac.za/");
            AddClickUrl(vislabLogo, "https://vislab.idia.ac.za/");
            AddClickUrl(inafLogo, "http://www.inaf.it/");
        }

        private void AddClickUrl(VisualElement element, string url)
        {
            element.RegisterCallback<ClickEvent>(evt =>
            {
                Application.OpenURL(url);
            });
        }


        public void Open()
        {
            Root.AddToClassList("active");
        }

        public void Close()
        {
            Root.RemoveFromClassList("active");
        }

    }

}