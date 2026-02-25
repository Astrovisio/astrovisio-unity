/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraPositioner : MonoBehaviour
{
    [Header("Configuration")]
    public OrbitCameraController orbitController;
    public Camera cameraToMove;

    [Header("Target Settings")]
    public Vector3 staticTargetPos = Vector3.zero;

    [Header("Position Settings")]
    public float distance = 10f;
    public enum Side { Front, Back, Left, Right, Top, Bottom }

    [HideInInspector]
    public Side currentSide = Side.Front;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (cameraToMove == null)
        {
            cameraToMove = GetComponentInChildren<Camera>();
        }
        if (orbitController == null && cameraToMove != null)
        {
            orbitController = cameraToMove.GetComponent<OrbitCameraController>();
        }
    }

    public void MoveToSide(Side side)
    {
        currentSide = side;
        RefreshPosition();
    }

    public void RefreshPosition()
    {
        if (orbitController == null || cameraToMove == null)
        {
            Initialize();
        }

        Vector3 targetPos = staticTargetPos;
        Vector3 requiredRotationEuler = Vector3.zero;

        switch (currentSide)
        {
            case Side.Front:
                requiredRotationEuler = new Vector3(0, 0, 0);
                break;
            case Side.Back:
                requiredRotationEuler = new Vector3(0, 180, 0);
                break;
            case Side.Left:
                requiredRotationEuler = new Vector3(0, 90, 0);
                break;
            case Side.Right:
                requiredRotationEuler = new Vector3(0, -90, 0);
                break;
            case Side.Top:
                requiredRotationEuler = new Vector3(90, 0, 0);
                break;
            case Side.Bottom:
                requiredRotationEuler = new Vector3(-90, 0, 0);
                break;
        }

        if (orbitController != null)
        {
            orbitController.ResetCameraView(targetPos, requiredRotationEuler, distance);
        }
        else
        {
            if (cameraToMove == null) return;
            Vector3 offset = Quaternion.Euler(requiredRotationEuler) * new Vector3(0, 0, -distance);
            cameraToMove.transform.position = targetPos + offset;
            cameraToMove.transform.LookAt(targetPos);
        }
    }

    public void SetFront() => MoveToSide(Side.Front);
    public void SetBack() => MoveToSide(Side.Back);
    public void SetLeft() => MoveToSide(Side.Left);
    public void SetRight() => MoveToSide(Side.Right);
    public void SetTop() => MoveToSide(Side.Top);
    public void SetBottom() => MoveToSide(Side.Bottom);
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraPositioner))]
public class CameraPositionerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CameraPositioner s = (CameraPositioner)target;

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            s.RefreshPosition();
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Quick Views", EditorStyles.boldLabel);

        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 12;
        bigButtonStyle.fixedHeight = 40;
        bigButtonStyle.fontStyle = FontStyle.Bold;

        float availableWidth = EditorGUIUtility.currentViewWidth - 40;
        float halfWidth = availableWidth / 2f;

        void DrawTwoButtons(string label1, CameraPositioner.Side side1, string label2, CameraPositioner.Side side2)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(label1, bigButtonStyle, GUILayout.Width(halfWidth)))
            {
                s.MoveToSide(side1);
            }

            if (GUILayout.Button(label2, bigButtonStyle, GUILayout.Width(halfWidth)))
            {
                s.MoveToSide(side2);
            }

            EditorGUILayout.EndHorizontal();
        }

        DrawTwoButtons("Front", CameraPositioner.Side.Front, "Back", CameraPositioner.Side.Back);
        DrawTwoButtons("Left", CameraPositioner.Side.Left, "Right", CameraPositioner.Side.Right);
        DrawTwoButtons("Top", CameraPositioner.Side.Top, "Bottom", CameraPositioner.Side.Bottom);
    }
}
#endif
