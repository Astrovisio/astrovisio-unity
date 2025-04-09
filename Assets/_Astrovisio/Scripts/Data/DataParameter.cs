
[System.Serializable]
public class DataParameter
{
    public string Name;
    public Axis SelectedAxis;
    public DataParameterThreshold Threshold;
    public bool Active;

    public DataParameter(string name, DataParameterThreshold threshold, bool active, Axis selectedAxis)
    {
        Name = name;
        Threshold = threshold;
        Active = active;
        SelectedAxis = selectedAxis;
    }

}
