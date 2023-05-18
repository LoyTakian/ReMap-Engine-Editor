
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
    public class JsonExport
    {
        [ MenuItem( "ReMap/Export/Json", false, 51 ) ]
        public static async void ExportJson()
        {
            Helper.FixPropTags();

            var path = EditorUtility.SaveFilePanel( "Json Export", "", "mapexport.json", "json" );

            if ( path.Length == 0 ) return;

            EditorUtility.DisplayProgressBar( "Starting Export", "" , 0 );

            ResetJsonData();

            foreach ( ObjectType objectType in Helper.GetAllObjectType() )
            {
                await ExecuteJson( objectType, ExecuteType.Export );
            }

            ReMapConsole.Log( "[Json Export] Writing to file: " + path, ReMapConsole.LogType.Warning );
            string json = JsonUtility.ToJson( jsonData );
            System.IO.File.WriteAllText( path, json );

            ReMapConsole.Log( "[Json Export] Finished.", ReMapConsole.LogType.Success );

            EditorUtility.ClearProgressBar();
        }

        [ MenuItem( "ReMap/Export Selection/Json", false, 51 ) ]
        public static async void ExportSelectionJson()
        {
            Helper.FixPropTags();

            var path = EditorUtility.SaveFilePanel( "Json Export", "", "mapexport.json", "json" );

            if ( path.Length == 0 ) return;

            EditorUtility.DisplayProgressBar( "Starting Export", "" , 0 );

            ResetJsonData();

            foreach ( ObjectType objectType in Helper.GetAllObjectType() )
            {
                await ExecuteJson( objectType, ExecuteType.Export, ExportType.Selection );
            }

            ReMapConsole.Log( "[Json Export] Writing to file: " + path, ReMapConsole.LogType.Warning );
            string json = JsonUtility.ToJson( jsonData );
            System.IO.File.WriteAllText( path, json );

            ReMapConsole.Log( "[Json Export] Finished.", ReMapConsole.LogType.Success );

            EditorUtility.ClearProgressBar();
        }


        internal static async Task ExportObjectsWithEnum< T >( ObjectType objectType, List< T > listType, ExportType exportType = ExportType.All ) where T : GlobalClassData
        {
            int i = 0; int j = 1; GameObject[] objectsData;

            switch ( exportType )
            {
                case ExportType.All: objectsData = Helper.GetObjArrayWithEnum( objectType ); break;
                case ExportType.Selection: objectsData = Helper.GetSelectedObjectWithEnum( objectType ); break;

                default: return;
            }

            int objectsCount = objectsData.Length;
            string objType = Helper.GetObjNameWithEnum( objectType );
            string objName;

            foreach( GameObject obj in objectsData )
            {
                objName = obj.name;

                if ( Helper.GetComponentByEnum( obj, objectType ) == null )
                {
                    ReMapConsole.Log( $"[Json Export] Missing Component on: " + objName, ReMapConsole.LogType.Error );
                    continue;
                }

                string exporting = ""; string objPath = FindPathString( obj );

                if ( string.IsNullOrEmpty( objPath ) )
                {
                    exporting = objName;
                } else exporting = $"{objPath}/{objName}";

                ReMapConsole.Log( "[Json Export] Exporting: " + objName, ReMapConsole.LogType.Info );
                EditorUtility.DisplayProgressBar( $"Exporting {objType} {j}/{objectsCount}", $"Exporting: {exporting}", ( i + 1 ) / ( float )objectsCount );

                T classData = Activator.CreateInstance( typeof( T ) ) as T;

                switch ( classData )
                {
                    case PropClassData data: // Props
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case ZipLineClassData data: // Ziplines
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case LinkedZipLinesClassData data: // Linked Ziplines
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case VerticalZipLineClassData data: // Vertical Ziplines
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case NonVerticalZipLineClassData data: // Non Vertical ZipLines
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case SingleDoorClassData data: // Single Doors
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case DoubleDoorClassData data: // Double Doors
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case HorzDoorClassData data: // Horizontal Doors
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case VerticalDoorClassData data: // Vertical Doors
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case JumpTowerClassData data: // Jump Towers
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case ButtonClassData data: // Bouttons
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case JumppadClassData data: // Jumppads
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case LootBinClassData data: // Loot Bins
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case WeaponRackClassData data: // Weapon Racks
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case TriggerClassData data: // Triggers
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case BubbleShieldClassData data: // Bubbles Shield
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case SpawnPointClassData data: // Spawn Points
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case NewLocPairClassData data: // New Loc Pairs
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case TextInfoPanelClassData data: // Text Info Panels
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case FuncWindowHintClassData data: // Window Hints
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case SoundClassData data: // Sounds
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;
                    case CameraPathClassData data: // Camera Paths
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;

                    case UOPlayerSpawnClassData data: // Unity Only Player Spawn
                        ProcessExportClassData( data, obj, objPath, objectType );
                        break;

                    default: break;
                }

                if ( IsValidPath( objPath ) ) listType.Add( classData );

                await Task.Delay( TimeSpan.FromSeconds( 0.001 ) ); i++; j++;
            }
        }

        private static void ProcessExportClassData< T >( T classData, GameObject obj, string objPath, ObjectType objectType ) where T : GlobalClassData
        {
            classData.PathString = objPath;
            classData.Path = FindPath( obj );
            classData.TransformData = GetSetTransformData( obj, classData.TransformData );
            GetSetScriptData( obj, classData, objectType, GetSetData.Get );
        }

        /// <summary>
        /// Instantiate a new JsonData class
        /// </summary>
        private static void ResetJsonData()
        {
            jsonData = new JsonData();
            jsonData.Version = UnityInfo.JsonVersion;
            jsonData.Props = new List< PropClassData >();
            jsonData.Ziplines = new List< ZipLineClassData >();
            jsonData.LinkedZiplines = new List< LinkedZipLinesClassData >();
            jsonData.VerticalZipLines = new List< VerticalZipLineClassData >();
            jsonData.NonVerticalZipLines = new List< NonVerticalZipLineClassData >();
            jsonData.SingleDoors = new List< SingleDoorClassData >();
            jsonData.DoubleDoors = new List< DoubleDoorClassData >();
            jsonData.HorzDoors = new List< HorzDoorClassData >();
            jsonData.VerticalDoors = new List< VerticalDoorClassData >();
            jsonData.JumpTowers = new List< JumpTowerClassData >();
            jsonData.Buttons = new List< ButtonClassData >();
            jsonData.Jumppads = new List< JumppadClassData >();
            jsonData.LootBins = new List< LootBinClassData >();
            jsonData.WeaponRacks = new List< WeaponRackClassData >();
            jsonData.Triggers = new List< TriggerClassData >();
            jsonData.BubbleShields = new List< BubbleShieldClassData >();
            jsonData.SpawnPoints = new List< SpawnPointClassData >();
            jsonData.NewLocPairs = new List< NewLocPairClassData >();
            jsonData.TextInfoPanels = new List< TextInfoPanelClassData >();
            jsonData.FuncWindowHints = new List< FuncWindowHintClassData >();
            jsonData.Sounds = new List< SoundClassData >();
            jsonData.CameraPaths = new List< CameraPathClassData >();
            jsonData.PlayerSpawns = new List< UOPlayerSpawnClassData >();
        }
    }
}
