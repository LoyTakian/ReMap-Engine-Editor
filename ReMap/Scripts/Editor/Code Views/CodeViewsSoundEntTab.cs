using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

using Build;
using static Build.Build;

namespace CodeViewsWindow
{
    public class SoundEntTab
    {
        internal static void OnGUIScriptEntTab()
        {
            GUILayout.BeginHorizontal( "box" );

                CodeViewsWindow.ObjectCount();

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical( "box" );
                CodeViewsWindow.scroll = EditorGUILayout.BeginScrollView( CodeViewsWindow.scroll );

                    GUILayout.TextArea( CodeViewsWindow.code, GUILayout.ExpandHeight( true ) );

                EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical( "box" );
        
                if (GUILayout.Button( "Copy To Clipboard" ) ) GenerateCode( true );

            GUILayout.EndVertical();
        }

        internal static string GenerateCode( bool copy )
        {
            string code = "";

            code += code += BuildObjectsWithEnum( ObjectType.Sound, BuildType.EntFile );

            return code;
        }
    }
}