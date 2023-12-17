using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
[CustomEditor(typeof(DoorScript))]
public class DoorScriptEditor : Editor
{
    void OnEnable()
    {
        CustomEditorStyle.OnEnable();
    }

    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        DoorScript myScript = target as DoorScript;

        Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/Door_CustomEditor") as Texture2D;
        GUILayout.Label(myTexture);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("GoldDoor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AppearOpen"));

        serializedObject.ApplyModifiedProperties();
    }
}
