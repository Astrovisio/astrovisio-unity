using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRScreenshotUIController : MonoBehaviour
    {
        [SerializeField] private Image loaderImage;
        [SerializeField] private TextMeshProUGUI labelTMP;

        private void Start()
        {
            SetLoaderImage(false, 0f);
        }

        public void SetLoaderImage(bool state, float value)
        {
            loaderImage.gameObject.SetActive(state);
            loaderImage.fillAmount = value;
        }

        public void SetLabel(string text)
        {
            labelTMP.text = text;
        }

    }

}
