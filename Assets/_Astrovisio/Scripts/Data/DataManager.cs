using System.Collections;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    private static DataManager _instance;
    private static readonly object _lock = new object();

    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = FindFirstObjectByType<DataManager>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject("DataManager");
                        _instance = singletonObject.AddComponent<DataManager>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

}
