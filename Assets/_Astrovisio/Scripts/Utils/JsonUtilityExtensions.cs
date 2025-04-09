using UnityEngine;

[System.Serializable]
public class JsonArrayWrapper<T>
{
    public T[] items;
}

public static class JsonUtilityExtensions
{
    public static T[] FromJsonWrapper<T>(string json)
    {
        // Aggiunge un wrapper attorno all'array JSON per poter utilizzare JsonUtility
        string newJson = "{\"items\":" + json + "}";
        JsonArrayWrapper<T> wrapper = JsonUtility.FromJson<JsonArrayWrapper<T>>(newJson);
        return wrapper.items;
    }
    
}
