using System;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class MenuPanelUI : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button valuesButton;
        [SerializeField] private Button vrReelButton;
        [SerializeField] private Button helpButton;
        [SerializeField] private Button exitVRButton;
        [SerializeField] private GameObject visual;

        private void Start()
        {
            ClosePanel();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
            else
            {
                Debug.LogWarning("Close Button non assegnato in MenuPanelUI!");
            }

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

        private void OnCloseButtonClick()
        {
            Debug.Log("Bottone 'Close' cliccato!");
            ClosePanel();
        }

        private void OnValuesButtonClick()
        {
            Debug.Log("Bottone 'Values' cliccato!");
            // Qui la logica per aprire un pannello dei valori, caricare dati, ecc.
        }

        private void OnVrReelButtonClick()
        {
            Debug.Log("Bottone 'VR Reel' cliccato!");
            // Qui la logica per avviare un video VR o una sequenza predefinita.
        }

        private void OnHelpButtonClick()
        {
            Debug.Log("Bottone 'Help' cliccato!");
            // Qui la logica per mostrare un pannello di aiuto o un tutorial.
        }

        private void OnExitVRButtonClick()
        {
            Debug.Log("Bottone 'Exit VR' cliccato!");
            VRManager.Instance.ExitVR();
            // Qui la logica per uscire dall'applicazione o dalla modalità VR.
            // Attenzione: Application.Quit() funziona solo nelle build, non nell'editor.
            // Application.Quit();
            // Per l'editor:
            // UnityEditor.EditorApplication.isPlaying = false;
        }

        private void OnDestroy()
        {
            // Rimuovi i listener quando l'oggetto viene distrutto per prevenire memory leak
            // o riferimenti a oggetti già distrutti. Questo è fondamentale per una gestione pulita.

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
            }

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
