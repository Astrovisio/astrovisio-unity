using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildIncrementor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        int buildNumber = PlayerPrefs.GetInt("BuildNumber", 0);
        buildNumber++;
        PlayerPrefs.SetInt("BuildNumber", buildNumber);
        PlayerSettings.bundleVersion = "0.0." + buildNumber;
        Debug.Log("Set build version to: " + PlayerSettings.bundleVersion);
    }

}
