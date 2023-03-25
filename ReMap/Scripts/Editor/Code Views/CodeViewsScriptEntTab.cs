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
    public class ScriptEntTab
    {
        internal static void OnGUIScriptEntTab()
        {
            GUILayout.BeginHorizontal( "box" );

                CodeViewsWindow.ObjectCount();

                GUILayout.FlexibleSpace();

                CodeViewsWindow.ShowAdvanced = EditorGUILayout.Toggle( "Show Advanced Options", CodeViewsWindow.ShowAdvanced, GUILayout.MaxWidth( 180 ) );

            GUILayout.EndHorizontal();

            if ( CodeViewsWindow.ShowAdvanced ) CodeViewsWindow.OptionalOption();

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

            if ( Helper.GetBoolFromGenerateObjects( ObjectType.Prop ) ) code += BuildObjectsWithEnum( ObjectType.Prop, BuildType.EntFile );
            if ( Helper.GetBoolFromGenerateObjects( ObjectType.VerticalZipLine ) ) code += BuildObjectsWithEnum( ObjectType.VerticalZipLine, BuildType.EntFile );
            if ( Helper.GetBoolFromGenerateObjects( ObjectType.NonVerticalZipLine ) ) code += BuildObjectsWithEnum( ObjectType.NonVerticalZipLine, BuildType.EntFile );

            return code;
        }
    }
}