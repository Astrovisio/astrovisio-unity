using System;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRMenuUIController : MonoBehaviour
    {
        [SerializeField] private Button valuesButton;
        [SerializeField] private Button vrReelButton;
        [SerializeField] private Button helpButton;
        [SerializeField] private Button exitVRButton;
        [SerializeField] private GameObject visual;

        private void Start()
        {
            ClosePanel();

            if (valuesButton != null)
            {
                valuesButton.onClick.AddListener(OnValuesButtonClick);
            }
            else
            {
                Debug.LogWarning("Values Button non assegnato in MenuPanelUI!");
            }

            if (vrReelButton != null)
            {
                vrReelButton.onClick.AddListener(OnVrReelButtonClick);
            }
            else
            {
                Debug.LogWarning("VR Reel Button non assegnato in MenuPanelUI!");
            }

            if (helpButton != null)
            {
                helpButton.onClick.AddListener(OnHelpButtonClick);
            }
            else
            {
                Debug.LogWarning("Help Button non assegnato in MenuPanelUI!");
            }

            if (exitVRButton != null)
            {
                exitVRButton.onClick.AddListener(OnExitVRButtonClick);
            }
            else
            {
                Debug.LogWarning("Exit VR Button non assegnato in MenuPanelUI!");
            }
        }

        public void OpenPanel()
        {
            visual.SetActive(true);
        }

        public void ClosePanel()
        {
            visual.SetActive(false);
        }

        public void TogglePanel()
        {
            if (visual.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        private void OnValuesButtonClick()
        {
            Debug.Log("Bottone 'Values' cliccato!");
        }

        private void OnVrReelButtonClick()
        {
            Debug.Log("Bottone 'VR Reel' cliccato!");
        }

        private void OnHelpButtonClick()
        {
            Debug.Log("Bottone 'Help' cliccato!");
        }

        private void OnExitVRButtonClick()
        {
            Debug.Log("Bottone 'Exit VR' cliccato!");
            XRManager.Instance.ExitVR();
        }

        private void OnDestroy()
        {
            if (valuesButton != null)
            {
                valuesButton.onClick.RemoveListener(OnValuesButtonClick);
            }

            if (vrReelButton != null)
            {
                vrReelButton.onClick.RemoveListener(OnVrReelButtonClick);
            }

            if (helpButton != null)
            {
                helpButton.onClick.RemoveListener(OnHelpButtonClick);
            }

            if (exitVRButton != null)
            {
                exitVRButton.onClick.RemoveListener(OnExitVRButtonClick);
            }
        }

    }

}
