#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace Astrovisio
{
    [CustomEditor(typeof(DebugAPIManager))]
    public class DebugAPIManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DebugAPIManager debugAPIManager = (DebugAPIManager)target;
            InitGetProjects(debugAPIManager);
        }

        private void InitGetProjects(DebugAPIManager debugAPIManager)
        {
            if (GUILayout.Button("GET Projects"))
            {
                debugAPIManager.FetchProjectsDebug();
            }
        }
    }
#endif

}