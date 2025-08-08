using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Astrovisio.XR
{
    [RequireComponent(typeof(Button))]
    public class XRMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI infoTMP;
        [SerializeField] private GameObject panel;
        [SerializeField] private string infoText;

        private Button button;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            infoTMP.text = infoText;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            infoTMP.text = "";
        }

        private void OnButtonClick()
        {
            // TODO: Search panel, if not in scene, instantiate panel
            
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

    }

}
