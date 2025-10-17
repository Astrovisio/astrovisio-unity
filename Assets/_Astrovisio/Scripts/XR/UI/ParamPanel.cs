using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class ParamPanel : MonoBehaviour
    {

        [SerializeField] private GameObject scrollViewGO;
        [SerializeField] private GameObject settingPanelGO;
        [SerializeField] private TextMeshProUGUI panelTitleTMP;

        private List<ParamButton> paramButtons;


        private void Start()
        {
            paramButtons = scrollViewGO.GetComponentsInChildren<ParamButton>().ToList();
            panelTitleTMP.text = "";

            foreach (ParamButton paramButton in paramButtons)
            {
                paramButton.SetButtonState(false);
                paramButton.OnParamButtonClicked += OnButtonClicked;
            }

            settingPanelGO.SetActive(false);
        }

        private void OnButtonClicked(ParamButton button)
        {
            bool isActive = button.State;

            if (isActive)
            {
                settingPanelGO.SetActive(true);
                ResetAllButton(button);
                // panelTitleTMP.text = button.settings.Name;
            }
            else
            {
                settingPanelGO.SetActive(false);
            }
        }

        private void ResetAllButton(ParamButton paramButtonToIgnore = null)
        {
            foreach (ParamButton paramButton in paramButtons)
            {
                if (paramButton != paramButtonToIgnore)
                {
                    paramButton.SetButtonState(false);
                }
            }
        }


    }

}
