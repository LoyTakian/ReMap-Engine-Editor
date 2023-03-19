using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public enum StringType
{
    ObjectRef = 0,
    TagName = 1,
    Name = 2
}

public enum ObjectType
{
    LootBin,
    ZipLine,
    VerticalZipLine,
    NonVerticalZipLine,
    LinkedZipline,
    Jumppad,
    SingleDoor,
    DoubleDoor,
    VerticalDoor,
    HorzDoor,
    WeaponRack,
    Button,
    Trigger,
    Prop,
    BubbleShield,
    Sound,
    SpawnPoint,
    TextInfoPanel
}

public class Helper
{
    public static int maxBuildLength = 75000;
    public static int greenPropCount = 1500;
    public static int yellowPropCount = 3000;
    public static bool Is_Using_Starting_Offset = false;
    public static bool DisableStartingOffsetString = false;

    // Gen Settings
    public static bool GenerateProps = true;
    public static bool GenerateButtons = true;
    public static bool GenerateJumppads = true;
    public static bool GenerateBubbleShields = true;
    public static bool GenerateDoors = true;
    public static bool GenerateLootBins = true;
    public static bool GenerateZipLines = true;
    public static bool GenerateWeaponRacks = true;
    public static bool GenerateTriggers = true;
    public static bool GenerateTextInfoPanel = true;

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
        public string fadeDistance;
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
    public static string ShouldAddStartingOrg(int type = 0)
    {
        if(!Is_Using_Starting_Offset)
            return "";

        if(type == 0)
            return " + startingorg";

        if(DisableStartingOffsetString)
            return "";

        return "    //Starting Origin, Change this to a origin in a map \n    vector startingorg = <0,0,0>" + "\n\n";
    }

    /// <summary>
    /// Builds correct angles from gameobject
    /// </summary>
    /// <param name="go">Prop Object</param>
    /// <returns></returns>
    public static string BuildAngles(GameObject go, bool isEntFile = false)
    {
        string x = (-WrapAngle(go.transform.eulerAngles.x)).ToString("F4").Replace(",", ".");
        string y = (-WrapAngle(go.transform.eulerAngles.y)).ToString("F4").Replace(",", ".");
        string z = (WrapAngle(go.transform.eulerAngles.z)).ToString("F4").Replace(",", ".");

        if ( x.Contains( ".0000" ) ) x = x.Replace( ".0000", "" );
        if ( y.Contains( ".0000" ) ) y = y.Replace( ".0000", "" );
        if ( z.Contains( ".0000" ) ) z = z.Replace( ".0000", "" );
                    
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
    public static float WrapAngle(float angle)
    {
        angle%=360;
        if(angle >180)
            return angle - 360;
 
        return angle;
    }

    /// <summary>
    /// Builds correct ingame origin from gameobject
    /// </summary>
    /// <param name="go">Prop Object</param>
    /// <returns></returns>
    public static string BuildOrigin(GameObject go, bool isEntFile = false)
    {
        float xOffset = 0;
        float yOffset = 0;
        float zOffset = 0;

        if (CodeViews.UseOriginOffset)
        {
            xOffset = CodeViews.OriginOffset.x;
            yOffset = CodeViews.OriginOffset.y;
            zOffset = CodeViews.OriginOffset.z;
        }

        string x = (-go.transform.position.z + zOffset).ToString("F4").Replace(",", ".");
        string y = (go.transform.position.x + xOffset).ToString("F4").Replace(",", ".");
        string z = (go.transform.position.y + yOffset).ToString("F4").Replace(",", ".");

        if ( x.Contains( ".0000" ) ) x = x.Replace( ".0000", "" );
        if ( y.Contains( ".0000" ) ) y = y.Replace( ".0000", "" );
        if ( z.Contains( ".0000" ) ) z = z.Replace( ".0000", "" );

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
    public static string BuildOriginVector(Vector3 vec, bool isEntFile = false)
    {
        float xOffset = 0;
        float yOffset = 0;
        float zOffset = 0;

        if (CodeViews.UseOriginOffset)
        {
            xOffset = CodeViews.OriginOffset.x;
            yOffset = CodeViews.OriginOffset.y;
            zOffset = CodeViews.OriginOffset.z;
        }

        string x = (-vec.z + zOffset).ToString("F4").Replace(",", ".");
        string y = (vec.x + xOffset).ToString("F4").Replace(",", ".");
        string z = (vec.y + yOffset).ToString("F4").Replace(",", ".");

        if ( x.Contains( ".0000" ) ) x = x.Replace( ".0000", "" );
        if ( y.Contains( ".0000" ) ) y = y.Replace( ".0000", "" );
        if ( z.Contains( ".0000" ) ) z = z.Replace( ".0000", "" );

        string origin = $"< {x}, {y}, {z} >";

        if( isEntFile )
            origin = $"( {x} {y} {z} )";

        return origin;
    }

    /// <summary>
    /// Tags Custom Prefabs so users cant wrongly tag a item
    /// </summary>
    public static void FixPropTags()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        //Retag All Objects
        foreach (GameObject go in allObjects) {
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
        dt.fadeDistance = items[8];
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
        script.fadeDistance = float.Parse(dt.fadeDistance);
        script.allowMantle = dt.canMantle == "true";

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
    /// Builds Map Code
    /// </summary>
    /// <returns>built map code string</returns>
    public static string BuildMapCode(bool buttons = true, bool jumppads = true, bool bubbleshields = true, bool weaponracks = true, bool lootbins = true, bool ziplines = true, bool doors = true, bool props = true, bool triggers = true, bool infopanel = true)
    {
        string code = "";
        if(buttons) code += Build.Buttons();
        if(jumppads) code += Build.Jumpads();
        if(bubbleshields) code += Build.BubbleShields();
        if(weaponracks) code += Build.WeaponRacks();
        if(lootbins) code += Build.LootBins();
        if(ziplines) code += Build.ZipLines();
        if(ziplines) code += Build.LinkedZipLines();
        if(ziplines) code += Build.VerticalZipLines();
        if(ziplines) code += Build.NonVerticalZipLines();
        if(doors) code += Build.SingleDoors();
        if(doors) code += Build.DoubleDoors();
        if(doors) code += Build.VertDoors();
        if(doors) code += Build.HorizontalDoors();
        if(props) code += Build.Props( null, Build.BuildType.Map );
        if(triggers) code += Build.Triggers();
        if(infopanel) code += Build.TextInfoPanel();
        return code;
    }

    public static void ApplyComponentScriptData<T>(T target, T source) where T : Component
    {
        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(source);
            field.SetValue(target, value);
        }
    }

    /*
    public static void ApplyComponentScriptDataFromJson<T>(T target, T source) where T : Component
    {
        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(source);
            field.SetValue(target, value);
        }
    }
    */

    public static string GetRandomGUIDForEnt()
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
    }

    public static GameObject[] GetObjArrayWithEnum( ObjectType objectType )
    {
        return GameObject.FindGameObjectsWithTag( GetObjTagNameWithEnum( objectType ) );
    }

    public static string GetObjRefWithEnum( ObjectType objectType )
    {
        return GetEnumString( objectType, StringType.ObjectRef );
    }

    public static string GetObjTagNameWithEnum( ObjectType objectType )
    {
        return GetEnumString( objectType, StringType.Name );
    }

    public static string GetObjNameWithEnum( ObjectType objectType )
    {
        return GetEnumString( objectType, StringType.Name );
    }

    public static string GetEnumString(ObjectType objectType, StringType stringType)
    {
        int i = (int)stringType;

        switch (objectType)
        {
            case ObjectType.LootBin:            return new string[] { "custom_lootbin",        "LootBin",            "Loot Bin"             }[i];
            case ObjectType.ZipLine:            return new string[] { "custom_zipline",        "ZipLine",            "ZipLine"              }[i];
            case ObjectType.VerticalZipLine:    return new string[] { "_vertical_zipline",     "VerticalZipLine",    "Vertical ZipLine"     }[i];
            case ObjectType.NonVerticalZipLine: return new string[] { "_non_vertical_zipline", "NonVerticalZipLine", "Non Vertical ZipLine" }[i];
            case ObjectType.LinkedZipline:      return new string[] { "custom_jumppad",        "LinkedZipline",      "Linked Zipline"       }[i];
            case ObjectType.Jumppad:            return new string[] { "custom_linked_zipline", "Jumppad",            "Jump Pad"             }[i];
            case ObjectType.SingleDoor:         return new string[] { "custom_single_door",    "SingleDoor",         "Single Door"          }[i];
            case ObjectType.DoubleDoor:         return new string[] { "custom_double_door",    "DoubleDoor",         "Double Door"          }[i];
            case ObjectType.VerticalDoor:       return new string[] { "custom_vertical_door",  "VerticalDoor",       "Vertical Door"        }[i];
            case ObjectType.HorzDoor:           return new string[] { "custom_sliding_door",   "HorzDoor",           "Horizontal Door"      }[i];
            case ObjectType.WeaponRack:         return new string[] { "custom_weaponrack",     "WeaponRack",         "Weapon Rack"          }[i];
            case ObjectType.Button:             return new string[] { "custom_button",         "Button",             "Button"               }[i];
            case ObjectType.Trigger:            return new string[] { "trigger_cylinder",      "Trigger",            "Trigger"              }[i];
            case ObjectType.Prop:               return new string[] { "mdl",                   "Prop",               "Prop"                 }[i];
            case ObjectType.BubbleShield:       return new string[] { "mdl#fx#bb_shield",      "BubbleShield",       "Bubble Shield"        }[i];
            case ObjectType.Sound:              return new string[] { "custom_sound",          "Sound",              "Sound"                }[i];
            case ObjectType.SpawnPoint:         return new string[] { "info_spawnpoint_human", "SpawnPoint",         "Spawn Point"          }[i];
            case ObjectType.TextInfoPanel:      return new string[] { "custom_TextInfoPanel",  "TextInfoPanel",      "Text Info Panel"      }[i];
 
            default: throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
        }
    }

    private static Dictionary<string, string> ObjectToTagDictionaryInit()
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        foreach ( ObjectType objectType in Enum.GetValues( typeof( ObjectType ) ) )
        {
            dictionary.Add( GetObjRefWithEnum( objectType ), GetObjTagNameWithEnum( objectType ) );
        }

        return dictionary;
    }

    public static string Credits = @"
//Made with Unity Map Editor
//By AyeZee#6969 & Julefox#0050
";
}