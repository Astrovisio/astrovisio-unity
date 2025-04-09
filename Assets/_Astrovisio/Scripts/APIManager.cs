using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class APIManager : MonoBehaviour {
    public static APIManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        FetchProjects();
    }

    public void FetchProjects() {
        StartCoroutine(GetProjects());
    }

    private IEnumerator GetProjects() {
        string url = APIEndpoints.GetProjects();

        using (UnityWebRequest www = UnityWebRequest.Get(url)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log("Error");
            } else {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log(jsonResponse);
                // ProjectsWrapper wrapper = JsonUtility.FromJson<ProjectsWrapper>(jsonResponse);
                // onSuccess?.Invoke(wrapper.projects);
            }
        }
    }
    
}
