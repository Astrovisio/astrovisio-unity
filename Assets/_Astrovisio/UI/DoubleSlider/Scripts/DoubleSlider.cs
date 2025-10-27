#region Includes
using TMPro;
using UnityEngine;
using UnityEngine.Events;
#endregion

namespace TS.DoubleSlider
{
    [RequireComponent(typeof(RectTransform))]
    public class DoubleSlider : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private SingleSlider _sliderMin;
        [SerializeField] private SingleSlider _sliderMax;
        [SerializeField] private RectTransform _fillArea;
        [SerializeField] private TextMeshProUGUI _minLabel;
        [SerializeField] private TextMeshProUGUI _maxLabel;

        [Header("Configuration")]
        [SerializeField] private bool _setupOnStart;
        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue;
        [SerializeField] private float _minDistance;
        [SerializeField] private bool _wholeNumbers;
        [SerializeField] private float _initialMinValue;
        [SerializeField] private float _initialMaxValue;

        [Header("Events")]
        public UnityEvent<float, float> OnValueChanged;

        private bool _isInternalSet;

        public bool IsEnabled
        {
            get { return _sliderMax.IsEnabled && _sliderMin.IsEnabled; }
            set
            {
                _sliderMin.IsEnabled = value;
                _sliderMax.IsEnabled = value;
            }
        }
        public float MinValue
        {
            get { return _sliderMin.Value; }
        }
        public float MaxValue
        {
            get { return _sliderMax.Value; }
        }
        public bool WholeNumbers
        {
            get { return _wholeNumbers; }
            set
            {
                _wholeNumbers = value;

                _sliderMin.WholeNumbers = _wholeNumbers;
                _sliderMax.WholeNumbers = _wholeNumbers;
            }
        }

        private RectTransform _fillRect;

        #endregion

        private void Awake()
        {
            if (_sliderMin == null || _sliderMax == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

                Debug.LogError("Missing slider min: " + _sliderMin + ", max: " + _sliderMax);
#endif
                return;
            }

            if (_fillArea == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

                Debug.LogError("Missing fill area");
#endif
                return;
            }

            _fillRect = _fillArea.transform.GetChild(0).transform as RectTransform;
        }
        private void Start()
        {
            if (!_setupOnStart) { return; }
            Setup(_minValue, _maxValue, _initialMinValue, _initialMaxValue);
        }

        public void Setup(float minValue, float maxValue, float initialMinValue, float initialMaxValue)
        {
            Debug.LogError($"{minValue} {initialMinValue} - {maxValue} {initialMaxValue}");

            _minValue = minValue;
            _maxValue = maxValue;
            _initialMinValue = initialMinValue;
            _initialMaxValue = initialMaxValue;

            _sliderMin.Setup(_initialMinValue, minValue, maxValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, minValue, maxValue, MaxValueChanged);

            MinValueChanged(_initialMinValue);
            MaxValueChanged(_initialMaxValue);
            UpdateLabels();
        }

        private void MinValueChanged(float value)
        {
            if (_isInternalSet)
            {
                _isInternalSet = false;
                return;
            }

            float range = _maxValue - _minValue;
            if (Mathf.Approximately(range, 0f) || _fillArea.rect.width <= 0f)
                return;

            if ((MaxValue - value) < _minDistance)
            {
                float clamped = MaxValue - _minDistance;
                _isInternalSet = true;
                _sliderMin.SetValueWithoutNotify(clamped);
                value = clamped;
            }

            float offset = ((MinValue - _minValue) / range) * _fillArea.rect.width;
            _fillRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);

            UpdateLabels();

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMin.transform.SetAsLastSibling();
        }


        private void MaxValueChanged(float value)
        {
            if (_isInternalSet)
            {
                _isInternalSet = false;
                return;
            }

            float range = _maxValue - _minValue;
            if (Mathf.Approximately(range, 0f) || _fillArea.rect.width <= 0f)
                return;

            if ((value - MinValue) < _minDistance)
            {
                float clamped = MinValue + _minDistance;
                _isInternalSet = true;
                _sliderMax.SetValueWithoutNotify(clamped);
                value = clamped;
            }

            float offset = (1 - ((MaxValue - _minValue) / range)) * _fillArea.rect.width;
            _fillRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);

            UpdateLabels();

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMax.transform.SetAsLastSibling();
        }

        private string FormatValue(float v)
        {
            return _wholeNumbers ? Mathf.RoundToInt(v).ToString() : v.ToString("0.###");
        }

        private void UpdateLabels()
        {
            if (_minLabel != null) _minLabel.text = FormatValue(MinValue);
            if (_maxLabel != null) _maxLabel.text = FormatValue(MaxValue);
        }

    }
}