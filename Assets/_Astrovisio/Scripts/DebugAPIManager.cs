using UnityEngine;
using UnityEditor;

public class DebugAPIManager : MonoBehaviour
{
    public APIManager apiManager;

    public void FetchProjectsDebug()
    {
        if (apiManager == null)
        {
            Debug.LogError("APIManager non assegnato!");
            return;
        }

        Debug.Log("Chiamata API: Get Projects");
        
        apiManager.FetchProjects();
    }
}
