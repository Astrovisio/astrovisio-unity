using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRHelpUIController : MonoBehaviour
    {

        [SerializeField] private Image helpImage;

        private void Start() {
            SetHelpImage(false);
        }

        public void ToggleHelpImage()
        {
            bool state = !helpImage.gameObject.activeSelf;
            SetHelpImage(state);
        }

        public void SetHelpImage(bool state)
        {
            helpImage.gameObject.SetActive(state);
        }


    }

}
