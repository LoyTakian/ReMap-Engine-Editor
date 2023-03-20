using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public enum GetSetData
{
    Get = 0,
    Set = 1
}

public class ImportExportJsonTest
{
    static JsonData jsonData = new JsonData();

    static string[] protectedModels = { "_vertical_zipline", "_non_vertical_zipline" };

    //  ██╗███╗   ███╗██████╗  ██████╗ ██████╗ ████████╗
    //  ██║████╗ ████║██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
    //  ██║██╔████╔██║██████╔╝██║   ██║██████╔╝   ██║   
    //  ██║██║╚██╔╝██║██╔═══╝ ██║   ██║██╔══██╗   ██║   
    //  ██║██║ ╚═╝ ██║██║     ╚██████╔╝██║  ██║   ██║   
    //  ╚═╝╚═╝     ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝   
    #if ReMapDev
    [ MenuItem( "ReMap Dev Tools/Import/Json", false, 51 ) ]
    public static async void ImportJson()
    {
        var path = EditorUtility.OpenFilePanel("Json Import", "", "json");

        if (path.Length == 0) return;

        EditorUtility.DisplayProgressBar( "Starting Import", "Reading File..." , 0 );
        ReMapConsole.Log( "[Json Import] Reading file: " + path, ReMapConsole.LogType.Warning );

        string json = System.IO.File.ReadAllText( path );
        JsonData jsonData = JsonUtility.FromJson< JsonData >( json );

        // Sort by alphabetical name
        ImportExportJson.SortListByKey( jsonData.Props, x => x.PathString );
        ImportExportJson.SortListByKey( jsonData.Ziplines, x => x.PathString );

        await ImportObjectsWithEnum( ObjectType.Prop, jsonData.Props );
        await ImportObjectsWithEnum( ObjectType.ZipLine, jsonData.Ziplines );

        ReMapConsole.Log("[Json Import] Finished", ReMapConsole.LogType.Success);

        EditorUtility.ClearProgressBar();
    }
    #endif
    private static async Task ImportObjectsWithEnum<T>( ObjectType objectType, List<T> listType ) where T : class
    {
        int i = 0; int j = 1;

        int objectsCount = listType.Count;
        string objType = Helper.GetObjNameWithEnum( objectType );

        T classData = Activator.CreateInstance( typeof( T ) ) as T; GameObject obj;

        foreach( T objData in listType )
        {
            switch ( classData )
            {
                case PropClassData data: // Props
                    data = ( PropClassData )( object ) objData;

                    obj = TryInstantiatePrefab( data.name, data.PathString, objType, i, j, objectsCount );
                    if ( obj == null ) continue;

                    GetSetTransformData( obj, data.TransformData );
                    GetSetScriptData( obj, data, objectType, GetSetData.Set );
                    CreatePath( data.Path, data.PathString, obj );
                    break;

                case ZipLineClassData data: // Ziplines
                    data = ( ZipLineClassData )( object ) objData;

                    obj = TryInstantiatePrefab( "custom_zipline", data.PathString, objType, i, j, objectsCount );
                    if ( obj == null ) continue;

                    GetSetTransformData( obj, data.TransformData );
                    GetSetScriptData( obj, data, objectType, GetSetData.Set );
                    CreatePath( data.Path, data.PathString, obj );
                    break;

                default: break;
            }

            await Task.Delay(TimeSpan.FromSeconds(0.001)); i++; j++;
        }
    }


    //  ███████╗██╗  ██╗██████╗  ██████╗ ██████╗ ████████╗
    //  ██╔════╝╚██╗██╔╝██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
    //  █████╗   ╚███╔╝ ██████╔╝██║   ██║██████╔╝   ██║   
    //  ██╔══╝   ██╔██╗ ██╔═══╝ ██║   ██║██╔══██╗   ██║   
    //  ███████╗██╔╝ ██╗██║     ╚██████╔╝██║  ██║   ██║   
    //  ╚══════╝╚═╝  ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝   
    #if ReMapDev
    [ MenuItem( "ReMap Dev Tools/Export/Json", false, 51 ) ]
    public static async void ExportJson()
    {
        Helper.FixPropTags();

        var path = EditorUtility.SaveFilePanel( "Json Export", "", "mapexport.json", "json" );

        if (path.Length == 0) return;

        EditorUtility.DisplayProgressBar( "Starting Export", "" , 0 );

        ResetJsonData();
        await ExportObjectsWithEnum( ObjectType.Prop, jsonData.Props );
        await ExportObjectsWithEnum( ObjectType.ZipLine, jsonData.Ziplines );

        ReMapConsole.Log( "[Json Export] Writing to file: " + path, ReMapConsole.LogType.Warning );
        string json = JsonUtility.ToJson( jsonData );
        System.IO.File.WriteAllText( path, json );

        ReMapConsole.Log( "[Json Export] Finished.", ReMapConsole.LogType.Success );

        EditorUtility.ClearProgressBar();
    }
    #endif

    private static async Task ExportObjectsWithEnum<T>( ObjectType objectType, List<T> listType ) where T : class
    {
        int i = 0; int j = 1;

        GameObject[] objectsData = Helper.GetObjArrayWithEnum( objectType );

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

            ReMapConsole.Log("[Json Export] Exporting: " + objName, ReMapConsole.LogType.Info);
            EditorUtility.DisplayProgressBar( $"Exporting {objType} {j}/{objectsCount}", $"Exporting: {exporting}", (i + 1) / (float)objectsCount );

            T classData = Activator.CreateInstance( typeof( T ) ) as T;

            switch ( classData )
            {
                case PropClassData data: // Props
                    data.PathString = objPath;
                    data.Path = FindPath( obj );
                    data.TransformData = GetSetTransformData( obj, data.TransformData );
                    GetSetScriptData( obj, data, objectType, GetSetData.Get );
                    break;

                case ZipLineClassData data: // Ziplines
                    data.PathString = objPath;
                    data.Path = FindPath( obj );
                    data.TransformData = GetSetTransformData( obj, data.TransformData );
                    GetSetScriptData( obj, data, objectType, GetSetData.Get );
                    break;

                default: break;
            }

            if ( IsValidPath( objPath ) ) listType.Add( classData );

            await Task.Delay(TimeSpan.FromSeconds(0.001)); i++; j++;
        }
    }


    //  ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗   ██╗
    //  ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝╚██╗ ██╔╝
    //  ██║   ██║   ██║   ██║██║     ██║   ██║    ╚████╔╝ 
    //  ██║   ██║   ██║   ██║██║     ██║   ██║     ╚██╔╝  
    //  ╚██████╔╝   ██║   ██║███████╗██║   ██║      ██║   
    //   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝      ╚═╝   

    private static void ResetJsonData()
    {
        jsonData = new JsonData();
        jsonData.Props = new List<PropClassData>();
        jsonData.Ziplines = new List<ZipLineClassData>();
    }

    private static TransformData GetSetTransformData( GameObject obj, TransformData data = null )
    {
        if ( data == null ) // if data is null, get the transformation data
        {
            data = new TransformData();
            data.position = obj.transform.position;
            data.eulerAngles = obj.transform.eulerAngles;
            data.localScale = obj.transform.localScale;

            return data;
        }
        else // otherwise, define the transformation data provided
        {
            obj.transform.position = data.position;
            obj.transform.eulerAngles = data.eulerAngles;
            obj.transform.localScale = data.localScale;

            return null;
        }
    }

    private static void GetSetScriptData< T >( GameObject obj, T scriptData, ObjectType dataType, GetSetData getSet ) where T : class
    {
        T classData = Activator.CreateInstance( typeof( T ) ) as T;

        if ( getSet == GetSetData.Get )
        {
            switch ( classData )
            {
                case PropClassData data: // Props
                    data = ( PropClassData )( object ) scriptData;
                    PropScript propScript = ( PropScript ) Helper.GetComponentByEnum( obj, dataType );
                    data.name = GetObjName( obj );
                    data.allowMantle = propScript.allowMantle;
                    data.fadeDistance = propScript.fadeDistance;
                    data.realmID = propScript.realmID;
                    data.parameters = propScript.parameters;
                    data.customParameters = propScript.customParameters;
                    break;

                case ZipLineClassData data: // Ziplines
                    data = ( ZipLineClassData )( object ) scriptData;
                    DrawZipline drawZipline = ( DrawZipline ) Helper.GetComponentByEnum( obj, dataType );
                    data.showZipline = drawZipline.showZipline;
                    data.showZiplineDistance = drawZipline.showZiplineDistance;
                    data.zipline_start = drawZipline.zipline_start.position;
                    data.zipline_end = drawZipline.zipline_end.position;
                    break;

                default: break;
            }
        }
        else
        {
            switch ( classData )
            {
                case PropClassData data: // Props
                    data = ( PropClassData )( object ) scriptData;
                    PropScript propScript = ( PropScript ) Helper.GetComponentByEnum( obj, dataType );
                    propScript.allowMantle = data.allowMantle;
                    propScript.fadeDistance = data.fadeDistance;
                    propScript.realmID = data.realmID;
                    propScript.parameters = data.parameters;
                    propScript.customParameters = data.customParameters;
                    break;

                case ZipLineClassData data: // Ziplines
                    data = ( ZipLineClassData )( object ) scriptData;
                    DrawZipline drawZipline = ( DrawZipline ) Helper.GetComponentByEnum( obj, dataType );
                    drawZipline.showZipline = data.showZipline;
                    drawZipline.showZiplineDistance = data.showZiplineDistance;
                    drawZipline.zipline_start.position = data.zipline_start;
                    drawZipline.zipline_end.position = data.zipline_end;
                    break;

                default: break;
            }
        }
    }

    /*
    Todo:

    LinkedZipline
    VerticalZipLine
    NonVerticalZipLine
    SingleDoor
    DoubleDoor
    HorzDoor
    VerticalDoor
    Button
    Jumppad
    LootBin
    WeaponRack
    Trigger
    BubbleShield
    SpawnPoint
    TextInfoPanel
    FuncWindowHint
    Sound
    */

    private static List< PathClass > FindPath( GameObject obj )
    {
        List< GameObject > parents = new List< GameObject >();
        List< PathClass > pathList = new List< PathClass >();
        GameObject currentParent = obj;

        // find all parented game objects
        while ( currentParent.transform.parent != null )
        {
            if ( currentParent != obj ) parents.Add( currentParent );
            currentParent = currentParent.transform.parent.gameObject;
        }

        if ( currentParent != obj ) parents.Add(currentParent);

        foreach ( GameObject parent in parents )
        {
            PathClass path = new PathClass();
            path.FolderName = parent.name;
            path.Position = parent.transform.position;
            path.Rotation = parent.transform.eulerAngles;

            pathList.Add( path );
        }

        pathList.Reverse();

        return pathList;
    }

    private static string FindPathString( GameObject obj )
    {
        List< GameObject > parents = new List< GameObject >();
        string path = "";
        GameObject currentParent = obj;

        // find all parented game objects
        while (currentParent.transform.parent != null)
        {
            if ( currentParent != obj ) parents.Add(currentParent);
            currentParent = currentParent.transform.parent.gameObject;
        }

        if ( currentParent != obj ) parents.Add(currentParent);

        parents.Reverse();

        foreach (GameObject parent in parents)
        {
            if ( string.IsNullOrEmpty( path ) )
            {
                path = $"{parent.name}";
            }
            else path = $"{path}/{parent.name}";
        }

        return path;
    }

    private static void CreatePath( List<PathClass> pathList, string pathString, GameObject obj )
    {
        if ( string.IsNullOrEmpty( pathString ) ) return;

        GameObject folder = null; string path = "";

        foreach ( PathClass pathClass in pathList )
        {
            if ( string.IsNullOrEmpty( path ) )
            {
                path = $"{pathClass.FolderName}";
            }
            else path = $"{path}/{pathClass.FolderName}";

            GameObject newFolder = GameObject.Find( path );

            if ( newFolder == null ) newFolder = new GameObject( pathClass.FolderName );

            newFolder.transform.position = pathClass.Position;
            newFolder.transform.eulerAngles = pathClass.Rotation;

            if ( folder != null ) newFolder.transform.SetParent(folder.transform);

            folder = newFolder;
        }

        if ( folder != null ) obj.transform.parent = folder.transform;
    }

    private static bool IsValidPath( string path )
    {
        foreach ( string protectedModel in protectedModels )
        {
            if ( path.Contains( protectedModel ) )
                return false;
        }

        return true;
    }

    private static string GetObjName( GameObject obj )
    {
        return obj.name.Split(char.Parse(" "))[0];
    }

    private static GameObject TryInstantiatePrefab( string objName, string objPath, string objType, int i, int j, int objectsCount )
    {
        string importing = "";

        if ( string.IsNullOrEmpty( objPath ) )
        {
            importing = objName;
        } else importing = $"{objPath}/{objName}";

        EditorUtility.DisplayProgressBar( $"Importing {objType} {j}/{objectsCount}", $"Importing: {importing}", (i + 1) / (float)objectsCount );
        ReMapConsole.Log("[Json Import] Importing: " + objName, ReMapConsole.LogType.Info);

        UnityEngine.Object loadedPrefabResource = ImportExportJson.FindPrefabFromName( objName );
        if ( loadedPrefabResource == null )
        {
            ReMapConsole.Log($"[Json Import] Couldnt find prefab with name of: {objName}" , ReMapConsole.LogType.Error);
            return null;
        }

        return PrefabUtility.InstantiatePrefab( loadedPrefabResource as GameObject ) as GameObject;
    }
}
