
using System;
using System.Collections.Generic;
using System.IO;
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
    public class CodeViewsWindow : EditorWindow
    {
        internal static CodeViewsWindow windowInstance;
        internal static string code = "";
        internal static string functionName = "unnamed";
        internal static string[] toolbarTab = new[] { "Squirrel Code", "DataTable Code", "Precache Code", "Ent Code" };
        internal static string[] toolbarSubTabEntCode = new[] { "script.ent", "snd.ent", "spawn.ent" };
        internal static Vector2 scroll;
        internal static Vector2 windowSize;
        internal static int paramToggleSize = 190;
        internal static int tab = 0;
        internal static int tab_temp = 0;
        internal static int tabEnt = 0;
        internal static int tabEnt_temp = 0;
        internal static Vector3 StartingOffset = Vector3.zero;
        internal static int GUILayoutToggleSize = 180;
        internal static int GUILayoutVector3FieldSize = 210;
        internal static int GUILayoutFunctionFieldSize = 210;
        internal static int GUILayoutLabelSize = 40;

        internal static bool ShowAdvancedMenu = false;
        internal static bool ShowFunction = false;
        internal static bool ShowFunctionTemp = false;
        internal static int EntityCount = 0;

        // Gen Settings
        public static Dictionary< string, bool > GenerateObjects = Helper.ObjectGenerateDictionaryInit();
        public static Dictionary< string, bool > GenerateObjectsFunction = Helper.ObjectGenerateDictionaryInit();
        public static Dictionary< string, bool > GenerateObjectsFunctionTemp = new Dictionary< string, bool >( GenerateObjects );
        public static string[] GenerateIgnore = new string[0];

        public static int greenPropCount = 1500;
        public static int yellowPropCount = 3000;


        [MenuItem("ReMap/Dev Test/Code Views", false, 25)]
        public static void Init()
        {
            TagHelper.CheckAndCreateTags();

            windowInstance = ( CodeViewsWindow ) GetWindow( typeof( CodeViewsWindow ), false, "Code Views" );
            windowInstance.minSize = new Vector2( 1100, 500 );
            windowInstance.Show();
        }

        void OnEnable()
        {
            EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
            EditorSceneManager.sceneSaved += EditorSceneManager_sceneSaved;

            GetLatestCounts();
            GenerateCorrectCode();
        }

        private void EditorSceneManager_sceneSaved( UnityEngine.SceneManagement.Scene arg0 )
        {
            GetLatestCounts();
            GenerateCorrectCode();
        }

        private void EditorSceneManager_sceneOpened( UnityEngine.SceneManagement.Scene arg0, OpenSceneMode mode )
        {
            GetLatestCounts();
            GenerateCorrectCode();
        }

        private static void GetLatestCounts()
        {
            switch ( tab )
            {
                case 0: // Squirrel Code
                    EntityCount = UnityInfo.GetAllCount();
                    break;

                case 1: // DataTable Code
                    EntityCount = UnityInfo.GetSpecificObjectCount( ObjectType.Prop );
                    break;

                case 2: // Precache Code
                    EntityCount = BuildProp.PrecacheList.Count;
                    break;

                case 3: // Ent Code
                    switch ( tabEnt )
                    {
                        case 0: // Script Code
                            EntityCount =
                            UnityInfo.GetSpecificObjectCount( ObjectType.Prop ) +
                            UnityInfo.GetSpecificObjectCount( ObjectType.VerticalZipLine ) +
                            UnityInfo.GetSpecificObjectCount( ObjectType.NonVerticalZipLine ) +
                            UnityInfo.GetSpecificObjectCount( ObjectType.Trigger );
                            break;

                        case 1: // Sound Code
                            EntityCount = UnityInfo.GetSpecificObjectCount( ObjectType.Sound );
                            break;
                        case 2:  // Spawn Code
                            EntityCount = UnityInfo.GetSpecificObjectCount( ObjectType.SpawnPoint );
                        break;
                    }
                break;
            }
        }

        internal static void GenerateCorrectCode()
        {
            code = "";

            switch ( tab )
            {
                case 0: // Squirrel Code
                    code += ScriptTab.GenerateCode( false );
                    functionName = Helper.GetSceneName();
                    break;

                case 1: // DataTable Code
                    code += DataTableTab.GenerateCode( false );
                    functionName = $"remap_datatable";
                    break;

                case 2: // Precache Code
                    code += PrecacheTab.GenerateCode( false );
                    functionName = $"{Helper.GetSceneName()}_Precache";
                    break;

                case 3: // Ent Code
                    switch ( tabEnt )
                    {
                        case 0: // Script Code
                            code += ScriptEntTab.GenerateCode( false );
                            functionName = $"mp_rr_remap_script";
                            break;

                        case 1: // Sound Code
                            code += SoundEntTab.GenerateCode( false );
                            functionName = $"mp_rr_remap_snd";
                            break;
                        case 2:  // Spawn Code
                            //code += ;
                            functionName = $"mp_rr_remap_spawn";
                        break;
                    }
                break;
            }
        }

        void OnGUI()
        {
            MainTab();

            GetEditorWindowSize();

            if( tab != tab_temp )
            {
                GetLatestCounts();
                GenerateCorrectCode();
                tab_temp = tab;
            }

            switch ( tab )
            {
                case 0: // Squirrel Code
                    ScriptTab.OnGUIScriptTab();
                    break;

                case 1: // DataTable Code
                    DataTableTab.OnGUIDataTableTab();
                    break;

                case 2: // Precache Code
                    PrecacheTab.OnGUIPrecacheTab();
                    break;

                case 3: // Ent Code
                    switch ( tabEnt )
                    {
                        case 0: // Script Code
                            ScriptEntTab.OnGUIScriptEntTab();
                            break;

                        case 1: // Sound Code
                            break;

                        case 2: // Spawn Code
                        break;
                    }
                break;
            }
        }

        private static bool IsIgnored( string key )
        {
            foreach ( string ignoredObj in GenerateIgnore )
            {
                if ( ignoredObj == key ) return true;
            }

            return false;
        }

        internal static void MainTab()
        {
            GUILayout.BeginVertical( "box" );
                GUILayout.BeginHorizontal( "box" );
                    tab = GUILayout.Toolbar ( tab, toolbarTab );
                    if ( GUILayout.Button("Refresh", GUILayout.Width( 100 ) ) ) Refresh();
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if ( tab == 3 )
            {
                GUILayout.BeginVertical( "box" );
                    GUILayout.BeginHorizontal( "box" );
                        tabEnt = GUILayout.Toolbar ( tabEnt, toolbarSubTabEntCode );
                    GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        internal static void OptionalUseOffset( string text = "Use Origin Offset" )
        {
            Helper.UseStartingOffset = EditorGUILayout.Toggle( text, Helper.UseStartingOffset, GUILayout.MaxWidth( CodeViewsWindow.GUILayoutToggleSize ) );
            if( Helper.UseStartingOffset != Helper.UseStartingOffsetTemp )
            {
                Helper.UseStartingOffsetTemp = Helper.UseStartingOffset;
                CodeViewsWindow.GenerateCorrectCode();
            }
        }

        internal static void OptionalShowOffset( string text = "Show Origin Offset" )
        {
            Helper.ShowStartingOffset = EditorGUILayout.Toggle( text, Helper.ShowStartingOffset, GUILayout.MaxWidth( GUILayoutToggleSize ) );
            if( Helper.ShowStartingOffset != Helper.ShowStartingOffsetTemp )
            {
                Helper.ShowStartingOffsetTemp = Helper.ShowStartingOffset;
                GenerateCorrectCode();
            }
        }

        internal static void OptionalOffsetField( string text = "Offset" )
        {
            EditorGUILayout.LabelField( text, GUILayout.MaxWidth( GUILayoutLabelSize ) );
            StartingOffset = EditorGUILayout.Vector3Field( "", StartingOffset, GUILayout.MaxWidth( GUILayoutVector3FieldSize ) );
        }

        internal static void ShowSquirrelFunction( string text = "Show Squirrel Function" )
        {
            ShowFunction = EditorGUILayout.Toggle( text, ShowFunction, GUILayout.MaxWidth( GUILayoutToggleSize ) );
            if( ShowFunction != ShowFunctionTemp )
            {
                ShowFunctionTemp = ShowFunction;
                GenerateCorrectCode();
            }
        }

        internal static void ObjectCount()
        {
            string info = tab == 2 ? "Models Precached Count" : "Entity Count";
            GUILayout.BeginHorizontal();

                SetCorrectColor( EntityCount );
                GUILayout.Label( $" // {info}: {EntityCount} | {SetCorrectEntityLabel( EntityCount )}", EditorStyles.boldLabel );
                GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
        }

        internal static void OptionalAdvancedOption( string text = "Show Advanced Options" )
        {
            ShowAdvancedMenu = EditorGUILayout.Toggle( text, ShowAdvancedMenu, GUILayout.MaxWidth( GUILayoutToggleSize ) );
        }

        internal static void AdvancedOptionMenu()
        {
            GUILayout.BeginVertical("box");

                int idx = 0;
                GUILayout.BeginHorizontal();
                    foreach ( string key in GenerateObjectsFunction.Keys )
                    {
                        if ( IsIgnored( key ) ) continue;

                        ObjectType? type = Helper.GetObjectTypeByObjName( key );
                        ObjectType typed = ( ObjectType ) type;
                        if ( UnityInfo.GetSpecificObjectCount( typed ) == 0 ) continue;
                        
                        string typedTag = Helper.GetObjTagNameWithEnum( typed );

                        if ( !IsValidScriptEntParam( typedTag ) ) continue;

                        GenerateObjects[key] = EditorGUILayout.Toggle( $"Build {key}", GenerateObjects[key], GUILayout.Width( paramToggleSize ) );

                        if ( GenerateObjects[key] != GenerateObjectsFunctionTemp[key] )
                        {
                            GenerateObjectsFunctionTemp[key] = GenerateObjects[key];
                            CodeViewsWindow.GenerateCorrectCode();
                        }

                        if ( idx == Mathf.FloorToInt( windowSize.x / paramToggleSize ) - 1 )
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            idx = 0;
                        } else idx++;
                    }
                GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            foreach ( string key in GenerateObjectsFunctionTemp.Keys )
            {
                GenerateObjectsFunction[key] = GenerateObjectsFunctionTemp[key];
            }
        }

        internal static void OptionalFunctionName()
        {
            EditorGUILayout.LabelField( "Name", GUILayout.MaxWidth( GUILayoutLabelSize ) );
            functionName = EditorGUILayout.TextField( "", functionName, GUILayout.Width( GUILayoutFunctionFieldSize ) );
        }

        internal static void ExportButton( string text = "Export Code" )
        {
            if ( GUILayout.Button( text, GUILayout.Width( 100 ) ) )
            {
                Helper.FixPropTags();

                EditorSceneManager.SaveOpenScenes();

                Refresh();

                string[] fileInfo = new string[4];

                switch ( tab )
                {
                    case 0: // Squirrel Code
                        fileInfo = new[] { "Squirrel Code Export", "", $"{functionName}.nut", "nut" };
                        break;

                    case 1: // DataTable Code
                        fileInfo = new[] { "DataTable Code Export", "", $"{functionName}.csv", "csv" };
                        break;

                    case 2: // Precache Code
                        fileInfo = new[] { "Precache Code Export", "", $"{functionName}.nut", "nut" };
                        break;

                    case 3: // Ent Code
                        fileInfo = new[] { "Precache Code Export", "", "", "ent" };
                        switch ( tabEnt )
                        {
                            case 0: // Script Code
                                fileInfo[2] = $"{functionName}.ent";
                                break;

                            case 1: // Sound Code
                                fileInfo[2] = $"{functionName}.ent";
                                break;

                            case 2: // Spawn Code
                                fileInfo[2] = $"{functionName}.ent";
                            break;
                        }
                    break;
                }

                var path = EditorUtility.SaveFilePanel( fileInfo[0], fileInfo[1], fileInfo[2], fileInfo[3] );

                if ( path.Length == 0 ) return;

                File.WriteAllText( path, code );
            }
        }

        internal static void CodeOutput()
        {
            GUILayout.BeginVertical( "box" );
                scroll = EditorGUILayout.BeginScrollView( scroll );
                    GUILayout.TextArea( code, GUILayout.ExpandHeight( true ) );
                EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        internal static void Refresh()
        {
            GenerateCorrectCode(); GetLatestCounts();
        }

        private static void SetCorrectColor( int count )
        {
            if( count < greenPropCount )
                GUI.contentColor = Color.green;

            else if( ( count < yellowPropCount ) ) 
                GUI.contentColor = Color.yellow;

            else GUI.contentColor = Color.red;
        }

        private static string SetCorrectEntityLabel( int count )
        {
            if( count < greenPropCount )
                return "Status: Safe";

            else if( ( count < yellowPropCount ) ) 
                return "Status: Safe";

            else return "Status: Warning! Game could crash!";
        }

        private static void GetEditorWindowSize()
        {
            EditorWindow editorWindow = windowInstance;
            if ( editorWindow != null )
            {
                windowSize = new Vector2( editorWindow.position.width, editorWindow.position.height );
            }
        }

        private static bool IsValidScriptEntParam( string type )
        {
            if ( tab == 3 && tabEnt == 0 ) // Ent Code/Script Code
                return ( type != Helper.GetObjTagNameWithEnum( ObjectType.Prop ) && type != Helper.GetObjTagNameWithEnum( ObjectType.VerticalZipLine ) && type != Helper.GetObjTagNameWithEnum( ObjectType.NonVerticalZipLine ) );

            return true;
        }
    }
}