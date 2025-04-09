using UnityEngine;

[System.Serializable]
public class DataParameterThreshold
{
    public float Min;
    public float Max;

    [SerializeField, HideInInspector] private float _originalMin;
    [SerializeField, HideInInspector] private float _originalMax;

    public DataParameterThreshold(float min, float max)
    {
        Min = min;
        Max = max;
        _originalMin = min;
        _originalMax = max;
    }

    public void ResetToOriginal()
    {
        Min = _originalMin;
        Max = _originalMax;
    }
    
}
