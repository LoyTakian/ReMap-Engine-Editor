using UnityEditor;
using UnityEngine;

[System.Serializable]
[CustomEditor(typeof(VerticalDoorScript))]
[CanEditMultipleObjects]
public class VerticalDoorScriptEditor : Editor
{
    void OnEnable()
    {
        CustomEditorStyle.OnEnable();
    }

    override public void OnInspectorGUI()
    {
        serializedObject.Update();

        Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/VerticalDoor_CustomEditor") as Texture2D;
        GUILayout.Label(myTexture);

        EditorGUILayout.LabelField(" Door Settings:", CustomEditorStyle.LabelStyle);
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(CustomEditorStyle.BoxStyle);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AppearOpen"));
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
