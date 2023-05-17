using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UnityInfo
{
    public static string ReMapVersion = "Version 1.0.1";
    public static string JsonVersion = "1.0.3";

    // Path Utility
    public static string currentDirectoryPath =        Directory.GetCurrentDirectory().Replace("\\","/");
    public static string relativePathLods =            $"Assets/ReMap/Lods - Dont use these";
    public static string relativePathLodsUtility =     $"{relativePathLods}/Utility";
    public static string relativePathEmptyPrefab =     $"{relativePathLodsUtility}/EmptyPrefab.prefab";
    public static string relativePathCubePrefab =      $"{relativePathLodsUtility}/Cube.prefab";
    public static string relativePathModel =           $"{relativePathLods}/Models";
    public static string relativePathMaterials =       $"{relativePathLods}/Materials";
    public static string relativePathPrefabs =         $"Assets/Prefabs";
    public static string relativePathAdditionalCode =  $"Assets/ReMap/Resources/AdditionalCode/additionalCode.json";
    public static string relativePathRpakManager =     $"Assets/ReMap/Resources/RpakManager";
    public static string relativePathRpakManagerList = $"{relativePathRpakManager}/rpakManagerList.json";
    public static string relativePathJsonOffset =      $"{relativePathRpakManager}/prefabOffsetList.json";
    public static string relativePathR5RPlayerInfo =    "\\platform\\scripts\\player_info.txt";
    public static string relativePathR5RScripts =       "\\platform\\scripts\\vscripts\\mp\\levels\\mp_rr_remap.nut";


    /// <summary>
    /// Gets total GameObject in scene
    /// </summary>
    /// <returns></returns>
    public static GameObject[] GetAllGameObjectInScene()
    {
        return UnityEngine.Object.FindObjectsOfType< GameObject >();
    }

    /// <summary>
    /// Gets Total Count of all objects in scene
    /// </summary>
    /// <returns></returns>
    public static int GetAllCount()
    {
        int objectCount = 0;

        foreach (GameObject go in GetAllGameObjectInScene())
        {
            foreach ( string key in Helper.ObjectToTag.Keys )
            {
                if ( go.name.Contains( key ) ) objectCount++;
            }
        }

        return objectCount;
    }

    /// <summary>
    /// Gets total count of a specific object in scene
    /// </summary>
    /// <returns></returns>
    public static int GetSpecificObjectCount( ObjectType objectType )
    {
        GameObject[] PropObjects = GameObject.FindGameObjectsWithTag( Helper.GetObjTagNameWithEnum( objectType ) );

        if ( objectType == ObjectType.ZipLine || objectType == ObjectType.LinkedZipline || objectType == ObjectType.VerticalZipLine || objectType == ObjectType.NonVerticalZipLine )
            return PropObjects.Length * 2;

        return PropObjects.Length;
    }

    /// <summary>
    /// Get all the models name in the active scene
    /// </summary>
    /// <returns></returns>
    public static string[] GetModelsListInScene()
    {
        List<string> modelsInScene = new List<string>();

        foreach ( GameObject go in GetAllGameObjectInScene() )
        {
            if ( go.name.Contains( "mdl#" ) && !modelsInScene.Contains( go.name ) )
                modelsInScene.Add( go.name );
        }

        modelsInScene.Sort();

        return modelsInScene.ToArray();
    }

    /// <summary>
    /// Returns the model name as a prefab
    /// </summary>
    /// <returns></returns>
    public static string GetUnityModelName( string modelName, bool extension = false )
    {
        string ext = extension ? ".prefab" : "";
        modelName = modelName.Replace( '#', '/' ).Replace( ".rmdl", "" ).Replace( ".prefab", "" );
        return modelName.Substring( modelName.IndexOf( "mdl/" ) ).Replace( '/', '#' ) + ext;
    }

    /// <summary>
    /// Returns the model name as a Apex path
    /// </summary>
    /// <returns></returns>
    public static string GetApexModelName( string modelName, bool extension = false )
    {
        string ext = extension ? ".rmdl" : "";
        modelName = modelName.Replace( '#', '/' ).Replace( ".rmdl", "" ).Replace( ".prefab", "" );
        if ( modelName.IndexOf( "mdl/" ) == -1 ) modelName = "mdl/" + modelName;
        return modelName.Substring( modelName.IndexOf( "mdl/" ) ) + ext;
    }

    /// <summary>
    /// Printt a string in editor console
    /// </summary>
    /// <returns></returns>
    public static void Printt( string str )
    {
        UnityEngine.Debug.Log( str );
    }

    public static string GetObjName( GameObject obj )
    {
        return obj.name.Split( char.Parse( " " ) )[0];
    }

    public static void SortListByKey< T, TKey >( List< T > list, Func< T, TKey > keySelector ) where TKey : IComparable
    {
        list.Sort( ( x, y ) => keySelector( x ).CompareTo( keySelector( y ) ) );
    }

    public static UnityEngine.Object FindPrefabFromName( string name )
    {
        // Hack so that the models named at the end with "(number)" still work
        if( name.Contains( " " ) ) name = name.Split( " " )[0];

        //Find Model GUID in Assets
        string[] results = AssetDatabase.FindAssets( name );
        if ( results.Length == 0 ) return null;

        //Get model path from guid and load it
        UnityEngine.Object loadedPrefabResource = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( results[0] ), typeof( UnityEngine.Object ) ) as GameObject;
        return loadedPrefabResource;
    }

    /// <summary>
    /// Example
    /// </summary>
    /// <returns></returns>
    //public static int GetCount()
    //{
    //    GameObject[] Objects = GameObject.FindGameObjectsWithTag("");
    //    return Objects.Length;
    //}
}
