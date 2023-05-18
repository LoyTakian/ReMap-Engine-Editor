
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

// Internal
using ImportExport.Shared;
using static ImportExport.Shared.SharedFunction;
using static ImportExport.Json.JsonShared;

namespace ImportExport.Json
{
    public class JsonImport
    {
        [ MenuItem( "ReMap/Import/Json", false, 51 ) ]
        public static async void ImportJson()
        {
            var path = EditorUtility.OpenFilePanel( "Json Import", "", "json" );

            if ( path.Length == 0 ) return;

            EditorUtility.DisplayProgressBar( "Starting Import", "Reading File..." , 0 );
            ReMapConsole.Log( "[Json Import] Reading file: " + path, ReMapConsole.LogType.Warning );

            string json = System.IO.File.ReadAllText( path );
            jsonData = JsonUtility.FromJson< JsonData >( json );

            if ( !CheckJsonVersion( jsonData ) )
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            // Sort by alphabetical name
            foreach ( ObjectType objectType in Helper.GetAllObjectType() )
            {
                await ExecuteJson( objectType, ExecuteType.SortList );
            }

            foreach ( ObjectType objectType in Helper.GetAllObjectType() )
            {
                await ExecuteJson( objectType, ExecuteType.Import );
            }

            ReMapConsole.Log( "[Json Import] Finished", ReMapConsole.LogType.Success );

            EditorUtility.ClearProgressBar();
        }

        internal static async Task ImportObjectsWithEnum< T >( ObjectType objectType, List< T > listType ) where T : GlobalClassData
        {
            int i = 0; int j = 1;

            int objectsCount = listType.Count;

            foreach( T objData in listType )
            {
                ProcessImportClassData( objData, DetermineDataName( objData, objectType ), objectType, i, j, objectsCount );

                await Task.Delay( TimeSpan.FromSeconds( 0.001 ) ); i++; j++;
            }
        }

        private static GameObject ProcessImportClassData< T >( T objData, string objName, ObjectType objectType, int i, int j, int objectsCount ) where T : GlobalClassData
        {
            GameObject obj = null; string importing = "";

            if ( string.IsNullOrEmpty( objData.PathString ) )
            {
                importing = objName;
            } else importing = $"{objData.PathString}/{objName}";

            EditorUtility.DisplayProgressBar( $"Importing {Helper.GetObjNameWithEnum( objectType )} {j}/{objectsCount}", $"Importing: {importing}", ( i + 1 ) / ( float )objectsCount );
            ReMapConsole.Log( "[Json Import] Importing: " + objName, ReMapConsole.LogType.Info );

            switch ( objectType )
            {
                case ObjectType.LinkedZipline:
                    obj = new GameObject( Helper.GetObjRefWithEnum( objectType ) );
                    break;

                case ObjectType.CameraPath:
                    obj = new GameObject( Helper.GetObjRefWithEnum( objectType ) );
                    break;

                default:
                    obj = Helper.CreateGameObject( "", objName, PathType.Name );
                break;
            }

            if ( !Helper.IsValid( obj ) ) return null;

            GetSetTransformData( obj, objData.TransformData );
            GetSetScriptData( obj, objData, objectType, GetSetData.Set );
            CreatePath( objData.Path, objData.PathString, obj );

            return obj;
        }

        private static string DetermineDataName< T >( T objData, ObjectType objectType ) where T : GlobalClassData
        {
            switch ( objData )
            {
                case PropClassData data: return data.Name;
                case VerticalZipLineClassData data: return data.Name;
                case NonVerticalZipLineClassData data: return data.Name;
                case WeaponRackClassData data: return data.Name;
                case BubbleShieldClassData data: return data.Name;
            }

            return Helper.GetObjRefWithEnum( objectType );
        }

        private static bool CheckJsonVersion( JsonData jsonData )
        {
            string Version = UnityInfo.JsonVersion;
            if ( jsonData.Version != Version )
            {
                string fileVersion = jsonData.Version == null ? "Unknown Version" : jsonData.Version;

                UnityInfo.Printt( $"This json file is outdated ( {fileVersion} / {Version} )" );

                return false;
            }

            return true;
        }
    }
}
