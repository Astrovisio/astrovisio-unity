#region Includes
using UnityEngine;
using TMPro;
#endregion

namespace TS.DoubleSlider
{
    public class InputField : MonoBehaviour
    {
        #region Variables

        public string Text
        {
            get { return _label.text; }
            set { _label.text = value; }
        }

        private TextMeshProUGUI _label;

        #endregion

        private void Awake()
        {
            if (!TryGetComponent<TextMeshProUGUI>(out _label))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing TextMeshProUGUI Component");
#endif
            }
        }
    }
}
