using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using ImportExport.Shared;
using static ImportExport.Shared.SharedFunction;
using Build;
using static Build.Build;
using static CodeViewsWindow.CodeViewsWindow;

public enum StartingOriginType
{
    SquirrelFunction = 0,
    Function = 1
}

public enum StringType
{
    ObjectRef = 0,
    TagName = 1,
    Name = 2
}

public enum ObjectType
{
    // Order of importance
    Prop,
    ZipLine,
    LinkedZipline,
    VerticalZipLine,
    NonVerticalZipLine,
    SingleDoor,
    DoubleDoor,
    HorzDoor,
    VerticalDoor,
    Button,
    Jumppad,
    LootBin,
    WeaponRack,
    Trigger,
    BubbleShield,
    NewLocPair,
    SpawnPoint,
    TextInfoPanel,
    FuncWindowHint,
    Sound
}

public class Helper
{
    public static int maxBuildLength = 75000;
    public static bool UseStartingOffset = false;
    public static bool UseStartingOffsetTemp = false;
    public static bool ShowStartingOffset = true;
    public static bool ShowStartingOffsetTemp = true;

    private static readonly Dictionary< ObjectType, ObjectTypeData > _objectTypeData = new Dictionary< ObjectType, ObjectTypeData >
    {
        { ObjectType.Prop,               new ObjectTypeData( new string[] { "mdl",                          "Prop",               "Prop"                 }, typeof( PropScript ),             typeof( PropClassData ) ) },
        { ObjectType.BubbleShield,       new ObjectTypeData( new string[] { "mdl#fx#bb_shield",             "BubbleShield",       "Bubble Shield"        }, typeof( BubbleScript ),           typeof( BubbleShieldClassData ) ) },
        { ObjectType.Button,             new ObjectTypeData( new string[] { "custom_button",                "Button",             "Button"               }, typeof( ButtonScripting ),        typeof( ButtonClassData ) ) },
        { ObjectType.DoubleDoor,         new ObjectTypeData( new string[] { "custom_double_door",           "DoubleDoor",         "Double Door"          }, typeof( DoorScript ),             typeof( DoubleDoorClassData ) ) },
        { ObjectType.FuncWindowHint,     new ObjectTypeData( new string[] { "custom_window_hint",           "FuncWindowHint",     "Window Hint"          }, typeof( WindowHintScript ),       typeof( FuncWindowHintClassData ) ) },
        { ObjectType.HorzDoor,           new ObjectTypeData( new string[] { "custom_sliding_door",          "HorzDoor",           "Horizontal Door"      }, typeof( HorzDoorScript ),         typeof( HorzDoorClassData ) ) },
        { ObjectType.Jumppad,            new ObjectTypeData( new string[] { "custom_jumppad",               "Jumppad",            "Jump Pad"             }, typeof( PropScript ),             typeof( JumppadClassData ) ) },
        { ObjectType.LinkedZipline,      new ObjectTypeData( new string[] { "custom_linked_zipline",        "LinkedZipline",      "Linked Zipline"       }, typeof( LinkedZiplineScript ),    typeof( LinkedZipLinesClassData ) ) },
        { ObjectType.LootBin,            new ObjectTypeData( new string[] { "custom_lootbin",               "LootBin",            "Loot Bin"             }, typeof( LootBinScript ),          typeof( LootBinClassData ) ) },
        { ObjectType.SingleDoor,         new ObjectTypeData( new string[] { "custom_single_door",           "SingleDoor",         "Single Door"          }, typeof( DoorScript ),             typeof( SingleDoorClassData ) ) },
        { ObjectType.Sound,              new ObjectTypeData( new string[] { "custom_sound",                 "Sound",              "Sound"                }, typeof( SoundScript ),            typeof( SoundClassData ) ) },
        { ObjectType.NewLocPair,         new ObjectTypeData( new string[] { "custom_new_loc_pair",          "NewLocPair",         "New Loc Pair"         }, typeof( NewLocPairScript ),       typeof( NewLocPairClassData ) ) },
        { ObjectType.SpawnPoint,         new ObjectTypeData( new string[] { "custom_info_spawnpoint_human", "SpawnPoint",         "Spawn Point"          }, typeof( SpawnPointScript ),       typeof( SpawnPointClassData ) ) },
        { ObjectType.TextInfoPanel,      new ObjectTypeData( new string[] { "custom_text_info_panel",       "TextInfoPanel",      "Text Info Panel"      }, typeof( TextInfoPanelScript ),    typeof( TextInfoPanelClassData ) ) },
        { ObjectType.Trigger,            new ObjectTypeData( new string[] { "trigger_cylinder",             "Trigger",            "Trigger"              }, typeof( TriggerScripting ),       typeof( TriggerClassData ) ) },
        { ObjectType.VerticalDoor,       new ObjectTypeData( new string[] { "custom_vertical_door",         "VerticalDoor",       "Vertical Door"        }, typeof( VerticalDoorScript ),     typeof( VerticalDoorClassData ) ) },
        { ObjectType.VerticalZipLine,    new ObjectTypeData( new string[] { "_vertical_zipline",            "VerticalZipLine",    "Vertical ZipLine"     }, typeof( DrawVerticalZipline ),    typeof( VerticalZipLineClassData ) ) },
        { ObjectType.NonVerticalZipLine, new ObjectTypeData( new string[] { "_non_vertical_zipline",        "NonVerticalZipLine", "Non Vertical ZipLine" }, typeof( DrawNonVerticalZipline ), typeof( NonVerticalZipLineClassData ) ) },
        { ObjectType.WeaponRack,         new ObjectTypeData( new string[] { "custom_weaponrack",            "WeaponRack",         "Weapon Rack"          }, typeof( WeaponRackScript ),       typeof( WeaponRackClassData ) ) },
        { ObjectType.ZipLine,            new ObjectTypeData( new string[] { "custom_zipline",               "ZipLine",            "ZipLine"              }, typeof( DrawZipline ),            typeof( ZipLineClassData ) ) }
    };

    public static Dictionary<string, string> ObjectToTag = ObjectToTagDictionaryInit();

    public enum ExportType
    {
        WholeScriptOffset,
        MapOnlyOffset,
        WholeScript,
        MapOnly
    }

    public struct NewDataTable {
        public string Type;
        public Vector3 Origin;
        public Vector3 Angles;
        public float Scale;
        public string FadeDistance;
        public string canMantle;
        public string isVisible;
        public string Model;
        public string Collection;
    }

    /// <summary>
    /// Should add starting origin to object location
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string ShouldAddStartingOrg( StartingOriginType type = StartingOriginType.Function, float x = 0, float y = 0, float z = 0 )
    {
        if( !UseStartingOffset || !ShowStartingOffset )
            return "";

        if( type == StartingOriginType.Function )
            return " + startingorg";

        string vector = $"< {ReplaceComma( x )}, {ReplaceComma( y )}, {ReplaceComma( z )} >";

        return $"    //Starting Origin, Change this to a origin in a map \n    vector startingorg = {vector}" + "\n\n";
    }

    /// <summary>
    /// Builds correct angles from gameobject
    /// </summary>
    /// <param name="go">Prop Object</param>
    /// <returns></returns>
    public static string BuildAngles( GameObject go, bool isEntFile = false )
    {
        string x = (-WrapAngle(go.transform.eulerAngles.x)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string y = (-WrapAngle(go.transform.eulerAngles.y)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string z = (WrapAngle(go.transform.eulerAngles.z)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );

        string angles = $"< {x}, {y}, {z} >";

        if( isEntFile )
            angles = $"{x} {y} {z}";

        return angles;
    }

    public static string BuildAnglesVector( Vector3 vec, bool isEntFile = false )
    {
        string x = (-WrapAngle(vec.x)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string y = (-WrapAngle(vec.y)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string z = (WrapAngle(vec.z)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );

        string angles = $"< {x}, {y}, {z} >";

        if( isEntFile )
            angles = $"{x} {y} {z}";

        return angles;
    }

    public static string BuildRightVector( Vector3 vec, bool isEntFile = false )
    {
        string x = (WrapAngle(vec.z)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string y = (WrapAngle(vec.x)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string z = (-WrapAngle(vec.y)).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );

        string angles = $"< {x}, {y}, {z} >";

        if( isEntFile )
            angles = $"{x} {y} {z}";

        return angles;
    }

    /// <summary>
    /// Wraps Angles that are above 180
    /// </summary>
    /// <param name="angle">Angle to wrap</param>
    /// <returns></returns>
    public static float WrapAngle( float angle )
    {
        angle %= 360;

        if( angle > 180 )
            return angle - 360;
 
        return angle;
    }

    /// <summary>
    /// Builds correct ingame origin from gameobject
    /// </summary>
    /// <param name="go">Prop Object</param>
    /// <returns></returns>
    public static string BuildOrigin( GameObject go, bool isEntFile = false, bool returnWithOffset = false )
    {
        float xOffset = UseStartingOffset && returnWithOffset ? StartingOffset.x : 0;
        float yOffset = UseStartingOffset && returnWithOffset ? StartingOffset.y : 0;
        float zOffset = UseStartingOffset && returnWithOffset ? StartingOffset.z : 0;

        string x = (-go.transform.position.z + xOffset).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string y = (go.transform.position.x + yOffset).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string z = (go.transform.position.y + zOffset).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );

        string origin = $"< {x}, {y}, {z} >";

        if( isEntFile )
            origin = $"{x} {y} {z}";

        return origin;
    }

    /// <summary>
    /// Builds correct ingame origin from vector3
    /// </summary>
    /// <param name="go">Prop Object</param>
    /// <returns></returns>
    public static string BuildOriginVector( Vector3 vec, bool isEntFile = false, bool returnWithOffset = false )
    {
        float xOffset = UseStartingOffset && returnWithOffset ? 0 : StartingOffset.x;
        float yOffset = UseStartingOffset && returnWithOffset ? 0 : StartingOffset.y;
        float zOffset = UseStartingOffset && returnWithOffset ? 0 : StartingOffset.z;

        string x = (-vec.z + xOffset).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string y = (vec.x + yOffset).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );
        string z = (vec.y + zOffset).ToString( "F4" ).TrimEnd( '0' ).Replace( ',', '.' ).TrimEnd( '.' );

        string origin = $"< {x}, {y}, {z} >";

        if( isEntFile )
            origin = $"{x} {y} {z}";

        return origin;
    }

    /// <summary>
    /// Tags Custom Prefabs so users cant wrongly tag a item
    /// </summary>
    public static void FixPropTags()
    {
        //Retag All Objects
        foreach ( GameObject go in UnityInfo.GetAllGameObjectInScene() )
        {
            go.tag = "Untagged";

            foreach (string key in ObjectToTag.Keys)
                if (go.name.Contains(key))
                    go.tag = ObjectToTag[key];
        }
    }

    public static NewDataTable BuildDataTable(string item)
    {
        string[] items = item.Replace("\"", "").Split(char.Parse(","));

        NewDataTable dt = new NewDataTable();
        dt.Type = items[0];
        dt.Origin = new Vector3(float.Parse(items[2]), float.Parse(items[3].Replace(">", "")), -(float.Parse(items[1].Replace("<", ""))));
        dt.Angles = new Vector3(-(float.Parse(items[4].Replace("<", ""))), -(float.Parse(items[5])), float.Parse(items[6].Replace(">", "")));
        dt.Scale = float.Parse(items[7]);
        dt.FadeDistance = items[8];
        dt.canMantle = items[9];
        dt.isVisible = items[10];
        dt.Model = items[11].Replace("/", "#").Replace(".rmdl", "").Replace("\"", "").Replace("\n", "").Replace("\r", "");
        dt.Collection = items[12].Replace("\"", "");

        return dt;
    }

    public static void CreateDataTableItem(NewDataTable dt, UnityEngine.Object loadedPrefabResource)
    {
        GameObject obj = PrefabUtility.InstantiatePrefab(loadedPrefabResource as GameObject) as GameObject;
        obj.transform.position = dt.Origin;
        obj.transform.eulerAngles = dt.Angles;
        obj.name = dt.Model;
        obj.gameObject.transform.localScale = new Vector3(dt.Scale, dt.Scale, dt.Scale);
        obj.SetActive(dt.isVisible == "true");

        PropScript script = obj.GetComponent<PropScript>();
        script.FadeDistance = float.Parse(dt.FadeDistance);
        script.AllowMantle = dt.canMantle == "true";

        if (dt.Collection == "")
            return;

        GameObject parent = GameObject.Find(dt.Collection);
        if (parent != null)
            obj.gameObject.transform.parent = parent.transform;
    }

    public static List<String> BuildCollectionList(string[] items)
    {
        List<String> collectionList = new List<String>();
        foreach (string item in items)
        {
            string[] itemsplit = item.Replace("\"", "").Split(char.Parse(","));

            if (itemsplit.Length < 12)
                continue;

            string collection = itemsplit[12].Replace("\"", "");

            if (collection == "")
                continue;

            if (!collectionList.Contains(collection))
                collectionList.Add(collection);
        }

        return collectionList;
    }

    /// <summary>
    /// Build Map Code
    /// </summary>
    /// <returns>Map Code as string</returns>
    public static string BuildMapCode( BuildType buildType = BuildType.Script, bool Selection = false )
    {
        // Order of importance
        string code = "";
        if( GetBoolFromGenerateObjects( ObjectType.Prop ) )               code += BuildObjectsWithEnum( ObjectType.Prop, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.ZipLine ) )            code += BuildObjectsWithEnum( ObjectType.ZipLine, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.LinkedZipline ) )      code += BuildObjectsWithEnum( ObjectType.LinkedZipline, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.VerticalZipLine ) )    code += BuildObjectsWithEnum( ObjectType.VerticalZipLine, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.NonVerticalZipLine ) ) code += BuildObjectsWithEnum( ObjectType.NonVerticalZipLine, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.SingleDoor ) )         code += BuildObjectsWithEnum( ObjectType.SingleDoor, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.DoubleDoor ) )         code += BuildObjectsWithEnum( ObjectType.DoubleDoor, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.HorzDoor ) )           code += BuildObjectsWithEnum( ObjectType.HorzDoor, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.VerticalDoor ) )       code += BuildObjectsWithEnum( ObjectType.VerticalDoor, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.Button ) )             code += BuildObjectsWithEnum( ObjectType.Button, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.Jumppad ) )            code += BuildObjectsWithEnum( ObjectType.Jumppad, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.LootBin ) )            code += BuildObjectsWithEnum( ObjectType.LootBin, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.WeaponRack ) )         code += BuildObjectsWithEnum( ObjectType.WeaponRack, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.Trigger ) )            code += BuildObjectsWithEnum( ObjectType.Trigger, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.BubbleShield ) )       code += BuildObjectsWithEnum( ObjectType.BubbleShield, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.SpawnPoint ) )         code += BuildObjectsWithEnum( ObjectType.SpawnPoint, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.NewLocPair ) )         code += BuildObjectsWithEnum( ObjectType.NewLocPair, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.TextInfoPanel ) )      code += BuildObjectsWithEnum( ObjectType.TextInfoPanel, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.FuncWindowHint ) )     code += BuildObjectsWithEnum( ObjectType.FuncWindowHint, buildType, Selection );
        if( GetBoolFromGenerateObjects( ObjectType.Sound ) )              code += BuildObjectsWithEnum( ObjectType.Sound, buildType, Selection );

        return code;
    }

    public static void ApplyComponentScriptData< T >( T target, T source ) where T : Component
    {
        Type type = typeof( T );
        FieldInfo[] fields = type.GetFields( BindingFlags.Public | BindingFlags.Instance );

        foreach ( FieldInfo field in fields )
        {
            object value = field.GetValue( source );
            field.SetValue( target, value );
        }
    }

    public static string GetRandomGUIDForEnt()
    {
        return Guid.NewGuid().ToString().Replace( "-", "" ).Substring( 0, 16 );
    }

    public static GameObject[] GetObjArrayWithEnum( ObjectType objectType )
    {
        return GameObject.FindGameObjectsWithTag( GetObjTagNameWithEnum( objectType ) );
    }

    public static string GetObjRefWithEnum( ObjectType objectType )
    {
        return Internal_GetStringByEnum( objectType, StringType.ObjectRef );
    }

    public static string GetObjTagNameWithEnum( ObjectType objectType )
    {
        return Internal_GetStringByEnum( objectType, StringType.TagName );
    }

    public static string GetObjNameWithEnum( ObjectType objectType )
    {
        return Internal_GetStringByEnum( objectType, StringType.Name );
    }

    private static string Internal_GetStringByEnum( ObjectType objectType, StringType stringType )
    {
        if ( _objectTypeData.TryGetValue( objectType, out ObjectTypeData objectTypeData ) && objectTypeData != null )
        {
            return objectTypeData.StringData[ ( int ) stringType ];
        }

        throw new ArgumentOutOfRangeException( nameof( objectType ), objectType, "This ObjectType does not exist." );
    }

    public static Component GetComponentByEnum( GameObject obj, ObjectType objectType )
    {
        if ( _objectTypeData.TryGetValue( objectType, out ObjectTypeData objectTypeData ) && objectTypeData != null )
        {
            return obj.GetComponent( objectTypeData.ComponentType );
        }

        return null;
    }

    public static Type GetImportExportClassByEnum( ObjectType objectType )
    {
        if ( _objectTypeData.TryGetValue( objectType, out ObjectTypeData objectTypeData ) && objectTypeData != null )
        {
            return objectTypeData.ImportExportClass;
        }

        return null;
    }

    public static ObjectType? GetObjectTypeByObjName( string searchTerm )
    {
        foreach ( ObjectType objectType in Enum.GetValues( typeof( ObjectType ) ) )
        {
            if ( Helper.GetObjNameWithEnum( objectType ) == searchTerm ) return objectType;
        }

        return null;
    }

    private class ObjectTypeData
    {
        public string[] StringData { get; }
        public System.Type ComponentType { get; }
        public Type ImportExportClass { get; }

        public ObjectTypeData( string[] stringData, System.Type componentType, Type importExportClass )
        {
            StringData = stringData;
            ComponentType = componentType;
            ImportExportClass = importExportClass;
        }
    }

    private static Dictionary< string, string > ObjectToTagDictionaryInit()
    {
        Dictionary< string, string > dictionary = new Dictionary< string, string >();

        foreach ( ObjectType objectType in Enum.GetValues( typeof( ObjectType ) ) )
        {
            dictionary.Add( GetObjRefWithEnum( objectType ), GetObjTagNameWithEnum( objectType ) );
        }

        return dictionary;
    }

    public static Dictionary< string, bool > ObjectGenerateDictionaryInit()
    {
        Dictionary< string, bool > dictionary = new Dictionary< string, bool >();

        foreach ( ObjectType objectType in Enum.GetValues( typeof( ObjectType ) ) )
        {
            dictionary.Add( GetObjNameWithEnum( objectType ), true );
        }

        return dictionary;
    }

    public static bool GetBoolFromGenerateObjects( ObjectType objectType )
    {
        return GenerateObjects[ GetObjNameWithEnum( objectType ) ];
    }

    public static void ForceSetBoolToGenerateObjects( ObjectType[] array, bool value )
    {
        foreach ( ObjectType objectType in array )
        {
            GenerateObjects[ GetObjNameWithEnum( objectType ) ] = value;
            GenerateObjectsFunctionTemp[ GetObjNameWithEnum( objectType ) ] = value;
        }
    }

    /// <summary>
    /// Forces objects not to appear in code, if forceShow is true, this return the opposite of the array specifier
    /// </summary>
    public static void ForceHideBoolToGenerateObjects( ObjectType[] array, bool forceShow = false )
    {
        List< ObjectType > objectTypeArray = new List< ObjectType >();
        if ( forceShow )
        {
            foreach ( ObjectType objectType in Enum.GetValues( typeof( ObjectType ) ) )
            {
                if ( !array.Contains( objectType ) ) objectTypeArray.Add( objectType );
            }
        } else objectTypeArray = array.ToList();

        CodeViewsWindow.CodeViewsWindow.GenerateIgnore = objectTypeArray.ToArray();
    }

    public static GameObject[] GetSelectedObjectWithEnum( ObjectType objectType )
    {
        GameObject[] SelectedObject =
        Selection.gameObjects.Where( obj => obj.CompareTag( Helper.GetObjTagNameWithEnum( objectType ) ) )
        .SelectMany( obj => obj.GetComponentsInChildren< Transform >( true ) )
        .Where( child => child.gameObject.CompareTag( Helper.GetObjTagNameWithEnum( objectType ) ) )
        .Select( child => child.gameObject )
        .Concat( Selection.gameObjects.Where( obj => obj.transform.childCount > 0 )
        .SelectMany( obj => obj.GetComponentsInChildren< Transform >( true ) )
        .Where( child => child.gameObject.CompareTag( Helper.GetObjTagNameWithEnum( objectType ) ) )
        .Select( child => child.gameObject ) )
        .Distinct()
        .ToArray();

        return SelectedObject;
    }

    public static string ReplaceComma( float value )
    {
        return value.ToString().Replace( ",", "." );
    }

    public static string BoolToLower( bool value )
    {
        return value.ToString().ToLower();
    }

    public static string GetSquirrelSceneNameFunction( bool ext = true )
    {
        string extention = ext ? "()" : "";
        return $"void function {SceneManager.GetActiveScene().name.Replace(" ", "_")}{extention}";
    }

    public static string GetSceneName()
    {
        return $"{SceneManager.GetActiveScene().name.Replace(" ", "_")}";
    }

    public static string ReMapCredit()
    {
        string credit = "";
        credit += $"    // Generated with Unity ReMap Editor {UnityInfo.ReMapVersion}\n";
        credit += $"    // Made with love by AyeZee#6969 & Julefox#0050 :)\n";
        PageBreak( ref credit );
        return credit;
    }
}