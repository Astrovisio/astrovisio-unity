using System;
using System.Collections;
using UnityEngine;

public class ProjectManager : MonoBehaviour
{

    // Singleton
    private static ProjectManager _instance;
    private static readonly object _lock = new object();

    public static ProjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = FindFirstObjectByType<ProjectManager>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject("ProjectManager");
                        _instance = singletonObject.AddComponent<ProjectManager>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
            }
            return _instance;
        }
    }


    public event Action<Project[]> OnProjectsLoaded;

    private Project[] projects;
    // private Project currentProject;


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

    private void Start()
    {
        StartCoroutine(GetProjectsFromFakeAPI());
    }

    public IEnumerator GetProjectsFromFakeAPI()
    {
        yield return new WaitForSeconds(2f);

        string fakeJson = @"
        [
            { ""id"": 1, ""name"": ""Progetto A"", ""description"": ""Descrizione A"" },
            { ""id"": 2, ""name"": ""Progetto B"", ""description"": ""Descrizione B"" },
            { ""id"": 3, ""name"": ""Progetto C"", ""description"": ""Descrizione C"" }
        ]";

        projects = JsonUtilityExtensions.FromJsonWrapper<Project>(fakeJson);

        foreach (var project in projects)
        {
            Debug.Log($"ID: {project.id} - Nome: {project.name}");
        }
        
        OnProjectsLoaded?.Invoke(projects);
    }

}
