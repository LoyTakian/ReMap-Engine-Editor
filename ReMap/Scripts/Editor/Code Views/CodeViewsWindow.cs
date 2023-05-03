
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

using Build;
using static Build.Build;
using WindowUtility;

namespace CodeViewsWindow
{
    public class CodeViewsWindow : EditorWindow
    {
        internal static CodeViewsWindow windowInstance;
        internal static string code = "";
        internal static string functionName = "Unnamed";
        internal static string[] toolbarTab = new[] { "Squirrel Code", "DataTable Code", "Precache Code", "Ent Code" };
        internal static string[] toolbarSubTabEntCode = new[] { "script.ent", "snd.ent", "spawn.ent" };
        internal static Vector2 scroll;
        internal static Vector2 scrollSettings;
        internal static Vector2 windowSize;
        internal static int tab = 0;
        internal static int tab_temp = 0;
        internal static int tabEnt = 0;
        internal static int tabEnt_temp = 0;
        internal static Vector3 StartingOffset = Vector3.zero;

        // Menus
        // Show / Hide Settings Menu
        internal static bool ShowSettingsMenu = false;

        // Menu Names
        internal static string SquirrelMenu = "SquirrelMenu";
        internal static string SquirrelMenuShowFunction = "SquirrelMenuFunction";
        internal static string SquirrelMenuShowInBlockAdditionalCode = "SquirrelMenuInBlockAdditionalCode";

        internal static string OffsetMenu = "OffsetMenu";
        internal static string OffsetMenuOffset = "OffsetMenuOffset";
        internal static string OffsetMenuShowOffset = "OffsetMenuShowOffset";

        internal static string SelectionMenu = "SelectionMenu";

        internal static string LiveCodeMenu = "LiveCodeMenu";
        internal static string LiveCodeMenuRespawn = "LiveCodeMenuRespawn";
        internal static string LiveCodeMenuTeleportation = "LiveCodeMenuTeleportation";
        internal static string LiveCodeMenuAutoSend = "LiveCodeMenuAutoSend";
        internal static string LiveCodeMenuAdvanced = "LiveCodeMenuAdvanced";

        internal static string AdditionalCodeMenu = "AdditionalCodeMenu";

        internal static string AdvancedMenu = "AdvancedMenu";

        internal static string FullFileEntMenu = "FullFileEntMenu";
        internal static string FullFileEntSubMenu = "FullFileEntSubMenu";

        internal static string DevMenu = "DevMenu";
        internal static string DevMenuDebugInfo = "DevMenuDebugInfo";
        //

        internal static bool GenerationIsActive = false;

        public static bool SendingObjects = false;
        internal static int EntityCount = 0;
        internal static int SendedEntityCount = 0;
        internal static int EntFileID = 27;
        internal static Vector3 InfoPlayerStartOrigin = Vector3.zero;
        internal static Vector3 InfoPlayerStartAngles = Vector3.zero;

        internal static Texture2D enableLogo;
        internal static Texture2D disableLogo;

        // Gen Settings
        public static Dictionary< string, bool > GenerateObjects = Helper.ObjectGenerateDictionaryInit();
        public static ObjectType[] GenerateIgnore = new ObjectType[0];

        public static int greenPropCount = 1500;
        public static int yellowPropCount = 3000;


        [ MenuItem( "ReMap/Code Views", false, 25 ) ]
        public static void Init()
        {
            TagHelper.CheckAndCreateTags(); tab = 0; tabEnt = 0;

            windowInstance = ( CodeViewsWindow ) GetWindow( typeof( CodeViewsWindow ), false, "Code Views" );
            windowInstance.minSize = new Vector2( 1230, 500 );
            windowInstance.Show();
            GetFunctionName();

            Helper.SetShowStartingOffset( true );
        }

        void OnEnable()
        {
            EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
            EditorSceneManager.sceneSaved += EditorSceneManager_sceneSaved;

            enableLogo = Resources.Load( "icons/codeViewEnable" ) as Texture2D;
            disableLogo = Resources.Load( "icons/codeViewDisable" ) as Texture2D;

            Refresh();
        }

        void OnGUI()
        {
            MainTab();

            GetEditorWindowSize(); ShortCut();

            if( tab != tab_temp || tabEnt != tabEnt_temp )
            {
                GetFunctionName();
                Refresh( false );
                tab_temp = tab;
                tabEnt_temp = tabEnt;
            }

            GUILayout.BeginVertical( "box" );
                GUILayout.BeginHorizontal();

                    CodeOutput();
                    
                    if ( ShowSettingsMenu )
                    {
                        GUILayout.BeginVertical( "box", GUILayout.Width( 340 ) );
                            SettingsMenu();
                        GUILayout.EndVertical();
                    }

                GUILayout.EndHorizontal();
        
                if ( GUILayout.Button( "Copy To Clipboard" ) ) CopyCode();
            GUILayout.EndVertical();
        }

        public static bool IsHided( ObjectType objectType )
        {
            // Ensure the objectData is not empty
            GameObject[] objectData;
            if ( CodeViewsWindow.SelectionEnable() )
                objectData = Helper.GetSelectedObjectWithEnum( objectType );
            else objectData = Helper.GetObjArrayWithEnum( objectType );

            if ( objectData.Length == 0 ) return true;

            // Check if objectType are flaged hide
            foreach ( ObjectType hidedObjectType in GenerateIgnore )
            {
                if ( hidedObjectType == objectType ) return true;
            }

            return false;
        }

        //  ██╗███╗   ██╗████████╗███████╗██████╗ ███╗   ██╗ █████╗ ██╗         ███████╗██╗   ██╗███╗   ██╗ ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //  ██║████╗  ██║╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██╔══██╗██║         ██╔════╝██║   ██║████╗  ██║██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //  ██║██╔██╗ ██║   ██║   █████╗  ██████╔╝██╔██╗ ██║███████║██║         █████╗  ██║   ██║██╔██╗ ██║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //  ██║██║╚██╗██║   ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██╔══██║██║         ██╔══╝  ██║   ██║██║╚██╗██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //  ██║██║ ╚████║   ██║   ███████╗██║  ██║██║ ╚████║██║  ██║███████╗    ██║     ╚██████╔╝██║ ╚████║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //  ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝    ╚═╝      ╚═════╝ ╚═╝  ╚═══╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝

        //  ██╗   ██╗██╗    ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗   ██╗
        //  ██║   ██║██║    ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝╚██╗ ██╔╝
        //  ██║   ██║██║    ██║   ██║   ██║   ██║██║     ██║   ██║    ╚████╔╝ 
        //  ██║   ██║██║    ██║   ██║   ██║   ██║██║     ██║   ██║     ╚██╔╝  
        //  ╚██████╔╝██║    ╚██████╔╝   ██║   ██║███████╗██║   ██║      ██║   
        //   ╚═════╝ ╚═╝     ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝      ╚═╝   
        internal static void Refresh( bool reSetScroll = true )
        {
            EntityCount = 0; GenerateCorrectCode();
            if ( reSetScroll ) SetScrollView( scroll );
            if ( AdditionalCodeWindow.windowInstance != null ) AdditionalCodeWindow.Refresh();
        }

        internal static void CopyCode()
        {
            GUIUtility.systemCopyBuffer = code;
        }

        internal static async void SetScrollView( Vector2 scroll )
        {
            // Hack: As long as the code generation is not finished, then do not continue the script
            while ( GenerationIsActive ) await Task.Delay( 100 ); // 100 ms

            CodeViewsWindow.scroll = scroll;
        }

        internal static async void ExportFunction()
        {
            Helper.FixPropTags();

            EditorSceneManager.SaveOpenScenes();

            Refresh();

            // Hack: As long as the code generation is not finished, then do not continue the script
            while ( GenerationIsActive ) await Task.Delay( 100 ); // 100 ms

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
                    fileInfo = new[] { "Ent Code Export", "", "", "ent" };
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

        //  ██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗    ███████╗██╗   ██╗███╗   ██╗ ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //  ██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝    ██╔════╝██║   ██║████╗  ██║██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //  ██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗      █████╗  ██║   ██║██╔██╗ ██║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //  ██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝      ██╔══╝  ██║   ██║██║╚██╗██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //  ██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗    ██║     ╚██████╔╝██║ ╚████║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //  ╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝    ╚═╝      ╚═════╝ ╚═╝  ╚═══╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝

        private void EditorSceneManager_sceneSaved( UnityEngine.SceneManagement.Scene arg0 )
        {
            Refresh();
        }

        private void EditorSceneManager_sceneOpened( UnityEngine.SceneManagement.Scene arg0, OpenSceneMode mode )
        {
            Refresh();
        }


        //  ███╗   ███╗ █████╗ ██╗███╗   ██╗    ██╗   ██╗██╗
        //  ████╗ ████║██╔══██╗██║████╗  ██║    ██║   ██║██║
        //  ██╔████╔██║███████║██║██╔██╗ ██║    ██║   ██║██║
        //  ██║╚██╔╝██║██╔══██║██║██║╚██╗██║    ██║   ██║██║
        //  ██║ ╚═╝ ██║██║  ██║██║██║ ╚████║    ╚██████╔╝██║
        //  ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝     ╚═════╝ ╚═╝
        private static void MainTab()
        {
            GUILayout.BeginVertical( "box" );
                GUILayout.BeginHorizontal();
                    tab = GUILayout.Toolbar ( tab, toolbarTab );
                    if ( GUILayout.Button( new GUIContent( "Refresh", "Refresh Window" ), GUILayout.Width( 100 ) ) ) Refresh();
                GUILayout.EndHorizontal();

                if ( tab == 3 )
                {
                    GUILayout.BeginHorizontal();
                        tabEnt = GUILayout.Toolbar ( tabEnt, toolbarSubTabEntCode );
                    GUILayout.EndHorizontal();           
                }

                GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 2 ) );

                GUILayout.BeginHorizontal();
                        ObjectCount();

                        if ( MenuInit.IsEnable( CodeViewsWindow.DevMenuDebugInfo ) )
                        {
                            CodeViewsMenu.Space( 10 );
                            GUILayout.Label( $"Window Size: {CodeViewsWindow.windowSize.x} x {CodeViewsWindow.windowSize.y}" );
                            GUILayout.Label( $"Scroll Position: {Helper.ReplaceComma( CodeViewsWindow.scroll.x )} x {Helper.ReplaceComma( CodeViewsWindow.scroll.y )}" );
                        }

                        GUILayout.FlexibleSpace();

                        ExportButton();
                        if ( tab == 0 ) AdditionalCodeButton();
                        SettingsMenuButton();
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static void SettingsMenu()
        {
            switch ( tab )
            {
                case 0: // Squirrel Code
                    ScriptTab.OnGUISettingsTab();
                    break;

                case 1: // DataTable Code
                    DataTableTab.OnGUISettingsTab();
                    break;

                case 2: // Precache Code
                    PrecacheTab.OnGUISettingsTab();
                    break;

                case 3: // Ent Code
                    switch ( tabEnt )
                    {
                        case 0: // Script Code
                            ScriptEntTab.OnGUISettingsTab();
                            break;

                        case 1: // Sound Code
                            SoundEntTab.OnGUISettingsTab();
                            break;

                        case 2: // Spawn Code
                            SpawnEntTab.OnGUISettingsTab();
                        break;
                    }
                break;
            }
        }

        private static void ObjectCount()
        {
            string info = tab == 2 ? "Models Precached Count" : "Entity Count";
            GUILayout.BeginHorizontal();

                if ( GenerationIsActive )
                {
                    GUILayout.Label( $" // Generating code...", EditorStyles.boldLabel );
                }
                else
                {
                    SetCorrectColor( EntityCount );
                    GUILayout.Label( $" // {info}: {EntityCount} | {SetCorrectEntityLabel( EntityCount )}", EditorStyles.boldLabel );
                    GUI.contentColor = Color.white;
                }

                if ( SendingObjects )
                {
                    if ( LiveMap.ApexProcessIsActive() )
                    {
                        GUILayout.Label( $"|| Sending {SendedEntityCount} Objects to the game", EditorStyles.boldLabel );
                    }
                    else GUILayout.Label( $"|| Game not found !", EditorStyles.boldLabel );
                }

            GUILayout.EndHorizontal();
        }

        private static void ExportButton( string text = "Export Code", string tooltip = "Export current code" )
        {
            if ( GUILayout.Button( new GUIContent( text, tooltip ), GUILayout.Width( 100 ) ) ) ExportFunction();
        }

        private static void AdditionalCodeButton( string text = "Additional Code", string tooltip = "Open Additional Code Window" )
        {
            if ( GUILayout.Button( new GUIContent( text, tooltip ), GUILayout.Width( 120 ) ) ) AdditionalCodeWindow.Init();
        }

        private static void SettingsMenuButton( string text = "Settings", string tooltip = "Show settings" )
        {
            if ( GUILayout.Button( new GUIContent( text, tooltip ), GUILayout.Width( 100 ) ) ) ShowSettingsMenu = !ShowSettingsMenu;
        }

        private static void CodeOutput()
        {
            GUIStyle style = new GUIStyle( GUI.skin.textArea );
            style.fontSize = 12;
            style.wordWrap = false;

            scroll = EditorGUILayout.BeginScrollView( scroll );
                GUILayout.TextArea( code, style, GUILayout.ExpandHeight( true ) );
            EditorGUILayout.EndScrollView();
        }


        //  ██╗   ██╗██╗     ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗   ██╗
        //  ██║   ██║██║     ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝╚██╗ ██╔╝
        //  ██║   ██║██║     ██║   ██║   ██║   ██║██║     ██║   ██║    ╚████╔╝ 
        //  ██║   ██║██║     ██║   ██║   ██║   ██║██║     ██║   ██║     ╚██╔╝  
        //  ╚██████╔╝██║     ╚██████╔╝   ██║   ██║███████╗██║   ██║      ██║   
        //   ╚═════╝ ╚═╝      ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝      ╚═╝   

        private static void ShortCut()
        {
            Event currentEvent = Event.current;

            if ( currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.R ) Refresh();
        } 

        private static void GetEditorWindowSize()
        {
            EditorWindow editorWindow = windowInstance;
            if ( editorWindow != null )
            {
                windowSize = new Vector2( editorWindow.position.width, editorWindow.position.height );
            }
        }

        private static async void GenerateCorrectCode()
        {
            code = "";

            GenerationIsActive = true;

            switch ( tab )
            {
                case 0: // Squirrel Code
                    code += await ScriptTab.GenerateCode();
                    break;

                case 1: // DataTable Code
                    code += await DataTableTab.GenerateCode();
                    break;

                case 2: // Precache Code
                    code += await PrecacheTab.GenerateCode();
                    break;

                case 3: // Ent Code
                    switch ( tabEnt )
                    {
                        case 0: // Script Code
                            code += await ScriptEntTab.GenerateCode();
                            break;

                        case 1: // Sound Code
                            code += await SoundEntTab.GenerateCode();
                            break;
                        case 2:  // Spawn Code
                            code += await SpawnEntTab.GenerateCode();
                        break;
                    }
                break;
            }

            GenerationIsActive = false;
        }

        private static void GetFunctionName()
        {
            switch ( tab )
            {
                case 0: // Squirrel Code
                    functionName = Helper.GetSceneName();
                    break;

                case 1: // DataTable Code
                    functionName = $"remap_datatable";
                    break;

                case 2: // Precache Code
                    functionName = $"{Helper.GetSceneName()}_Precache";
                    break;

                case 3: // Ent Code
                    switch ( tabEnt )
                    {
                        case 0: // Script Code
                            functionName = $"mp_rr_remap_script";
                            break;

                        case 1: // Sound Code
                            functionName = $"mp_rr_remap_snd";
                            break;
                        case 2:  // Spawn Code
                            functionName = $"mp_rr_remap_spawn";
                        break;
                    }
                break;
            }
        }

        private static string SetCorrectEntityLabel( int count )
        {
            if( count < greenPropCount )
                return "Status: Safe";

            else if( ( count < yellowPropCount ) ) 
                return "Status: Safe";

            else return "Status: Warning! Game could crash!";
        }

        private static void SetCorrectColor( int count )
        {
            if( count < greenPropCount )
                GUI.contentColor = Color.green;

            else if( count < yellowPropCount ) 
                GUI.contentColor = Color.yellow;

            else GUI.contentColor = Color.red;
        }


        internal static bool ShowFunctionEnable()
        {
            return MenuInit.IsEnable( SquirrelMenuShowFunction );
        }

        internal static bool InBlockAdditionalCodeEnable()
        {
            return MenuInit.IsEnable( SquirrelMenuShowInBlockAdditionalCode );
        }

        internal static bool SelectionEnable()
        {
            return MenuInit.IsEnable( SelectionMenu );
        }

        internal static bool AutoSendEnable()
        {
            return MenuInit.IsEnable( LiveCodeMenuAutoSend );
        }
    }
}