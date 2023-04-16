using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MaterialBlenderDemo))]
public class MaterialBlenderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        var editor = (MaterialBlenderDemo)target;
        var inspectorWidth = EditorGUIUtility.currentViewWidth;
        var rect = EditorGUILayout.GetControlRect(false, inspectorWidth / 2);
        EditorGUI.DrawPreviewTexture(rect, editor.Result);

        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        var segmentWidth = rect.width / 4f;
        var textureWidth = segmentWidth * 0.9f;
        var labelRect = new Rect(rect.x + segmentWidth / 2, rect.yMax - 30, textureWidth, 20);
        GUI.Label(labelRect, "Linear Blend", labelStyle);
        labelRect.x += segmentWidth + 10;
        GUI.Label(labelRect, "Masked Blend", labelStyle);
        labelRect.x += segmentWidth + 10;
        GUI.Label(labelRect, "Masked Depth Blend", labelStyle);
    }
}
