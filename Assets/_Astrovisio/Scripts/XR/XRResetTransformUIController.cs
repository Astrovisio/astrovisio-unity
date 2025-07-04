using UnityEngine;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRResetTransformUIController : MonoBehaviour
    {

        [SerializeField] private Image loaderImage;

        private void Start()
        {
            SetLoaderImage(false, 0f);
        }

        public void SetLoaderImage(bool state, float value)
        {
            loaderImage.gameObject.SetActive(state);
            loaderImage.fillAmount = value;
        }

    }

}
