using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Astrovisio
{
    public class XRUIManager : MonoBehaviour
    {
        public static XRUIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

    }

}
