#region Includes
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
#endregion

namespace TS.DoubleSlider
{
    [RequireComponent(typeof(Slider))]
    public class SingleSlider : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        // [SerializeField] private Label _label;
        [SerializeField] private TMP_InputField _inputField;
        private bool _isUpdating;

        private Slider _slider;

        public bool IsEnabled
        {
            get { return _slider.interactable; }
            set { _slider.interactable = value; }
        }
        public float Value
        {
            get { return _slider.value; }
            set
            {
                _isUpdating = true;
                _slider.value = value;
                _slider.onValueChanged.Invoke(_slider.value);
                // UpdateLabel();
                UpdateInputField();
                _isUpdating = false;
            }
        }
        public bool WholeNumbers
        {
            get { return _slider.wholeNumbers; }
            set { _slider.wholeNumbers = value; }
        }

        #endregion

        private void Awake()
        {
            if (!TryGetComponent<Slider>(out _slider))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing Slider Component");
#endif
            }
            if (_inputField != null)
                _inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        }

        public void Setup(float value, float minValue, float maxValue, UnityAction<float> valueChanged)
        {
            _slider.minValue = minValue;
            _slider.maxValue = maxValue;

            _slider.value = value;
            _slider.onValueChanged.AddListener(Slider_OnValueChanged);
            _slider.onValueChanged.AddListener(valueChanged);

            if (_inputField != null)
                _inputField.onEndEdit.AddListener(InputField_OnEndEdit);
        }

        private void Slider_OnValueChanged(float arg0)
        {
            if (_isUpdating) return;
            // UpdateLabel();
            UpdateInputField();
        }

        private void InputField_OnEndEdit(string val)
        {
            if (_isUpdating) return;
            if (float.TryParse(val, out float parsedValue))
            {
                parsedValue = Mathf.Clamp(parsedValue, _slider.minValue, _slider.maxValue);
                _isUpdating = true;
                _slider.value = parsedValue;
                _slider.onValueChanged.Invoke(_slider.value);
                // UpdateLabel();
                _isUpdating = false;
            }
            UpdateInputField();
        }

        // protected virtual void UpdateLabel()
        // {
        //     if (_label == null) { return; }
        //     _label.Text = Value.ToString();
        // }

        private void UpdateInputField()
        {
            if (_inputField == null) return;
            _inputField.text = _slider.value.ToString("0.#######");
        }

    }

}
