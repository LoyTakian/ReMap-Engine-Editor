using UnityEngine;

public class Build
{
    public enum BuildType {
        Map, 
        Ent, 
        Precache,
        DataTable
    };

    public static string Buttons()
    {
        GameObject[] ButtonObjects = GameObject.FindGameObjectsWithTag("Button");
        if (ButtonObjects.Length < 1)
            return "";
        
        string code = "    //Buttons \n";

        foreach (GameObject go in ButtonObjects)
        {
            ButtonScripting script = go.GetComponent<ButtonScripting>();
            code += $"    AddCallback_OnUseEntity( CreateFRButton({Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, \"{script.UseText}\"), void function(entity panel, entity user, int input)" + "\n    {\n" + script.OnUseCallback + "\n    })" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string Jumpads()
    {
        GameObject[] JumppadObjects = GameObject.FindGameObjectsWithTag("Jumppad");
        if (JumppadObjects.Length < 1)
            return "";

        string code = "    //Jumppads \n";

        foreach (GameObject go in JumppadObjects)
        {
            PropScript script = go.GetComponent<PropScript>();
            code += $"    JumpPad_CreatedCallback( MapEditor_CreateProp( $\"mdl/props/octane_jump_pad/octane_jump_pad.rmdl\", {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, {script.allowMantle.ToString().ToLower()}, {script.fadeDistance}, {script.realmID}, {go.transform.localScale.x.ToString().Replace(",", ".")} ) )" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string BubbleShields()
    {
        GameObject[] BubbleShieldObjects = GameObject.FindGameObjectsWithTag("BubbleShield");
        if (BubbleShieldObjects.Length < 1)
            return "";

        string code = "    //BubbleShields \n";

        foreach (GameObject go in BubbleShieldObjects)
        {
            string model = go.name.Split(char.Parse(" "))[0].Replace("#", "/") + ".rmdl";
            BubbleScript script = go.GetComponent<BubbleScript>();
            string shieldColor = script.shieldColor.r + " " + script.shieldColor.g + " " + script.shieldColor.b;
                
            code += $"    MapEditor_CreateBubbleShieldWithSettings( {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, {go.transform.localScale.x.ToString().Replace(",", ".")}, \"{shieldColor}\", $\"{model}\" )" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string WeaponRacks()
    {
        GameObject[] WeaponRackObjects = GameObject.FindGameObjectsWithTag("WeaponRack");
        if (WeaponRackObjects.Length < 1)
            return "";

        string code = "    //Weapon Racks \n";

        foreach (GameObject go in WeaponRackObjects)
        {
            WeaponRackScript script = go.GetComponent<WeaponRackScript>();
            code += $"    MapEditor_CreateRespawnableWeaponRack( {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, \"{go.name.Replace("custom_weaponrack_", "mp_weapon_")}\", {script.respawnTime} )" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string LootBins()
    {
        GameObject[] LootBinObjects = GameObject.FindGameObjectsWithTag("LootBin");
        if (LootBinObjects.Length < 1)
            return "";

        string code = "    //LootBins \n";

        foreach (GameObject go in LootBinObjects)
        {
            LootBinScript script = go.GetComponent<LootBinScript>();
            code += $"    MapEditor_CreateLootBin( {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, {script.lootbinSkin} )" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string ZipLines()
    {
        GameObject[] ZipLineObjects = GameObject.FindGameObjectsWithTag("ZipLine");
        if (ZipLineObjects.Length < 1)
            return "";

        string code = "    //ZipLines \n";

        foreach (GameObject go in ZipLineObjects)
        {
            string ziplinestart = "";
            string ziplineend = "";

            foreach (Transform child in go.transform)
            {
                if (child.name == "zipline_start")
                    ziplinestart = Helper.BuildOrigin(child.gameObject);
                else if (child.name == "zipline_end")
                    ziplineend = Helper.BuildOrigin(child.gameObject);
            }

            code += $"    CreateZipline( {ziplinestart + Helper.ShouldAddStartingOrg()}, {ziplineend + Helper.ShouldAddStartingOrg()} )" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string LinkedZipLines()
    {
        GameObject[] LinkedZipLineObjects = GameObject.FindGameObjectsWithTag("LinkedZipline");
        
        if(LinkedZipLineObjects.Length < 1)
            return "";

        string code = "    //LinkedZipLines \n";

        foreach (GameObject go in LinkedZipLineObjects)
        {
            bool first = true;
            string nodes = "[ ";

            LinkedZiplineScript script = go.GetComponent<LinkedZiplineScript>();

            foreach (Transform child in go.transform)
            {
                if (!first)
                    nodes += ", ";

                nodes += Helper.BuildOrigin(child.gameObject);

                first = false;
            }

            string smoothType = "GetAllPointsOnBezier";
            if (!script.smoothType)
                smoothType = "GetBezierOfPath";

            nodes += " ]";

            code += @"    MapEditor_CreateLinkedZipline( ";
            if (script.enableSmoothing) code += $"{smoothType}( ";
            code += nodes;
            if (script.enableSmoothing) code += $", {script.smoothAmount}";
            code += " )";
            if (script.enableSmoothing) code += " )";
            code += "\n";
        }

        code += "\n";

        return code;
    }

    public static string VerticalZipLines()
    {
        GameObject[] VerticalZipLineObjects = GameObject.FindGameObjectsWithTag("VerticalZipLine");

        if (VerticalZipLineObjects.Length < 1)
            return "";

        string code = "    //VerticalZipLines \n";

        foreach (GameObject go in VerticalZipLineObjects)
        {
            DrawVerticalZipline ziplineScript = go.GetComponent<DrawVerticalZipline>();
            if (ziplineScript == null)
                continue;

            int preservevelocity = 0;
            int dropToBottom = 0;
            string restpoint = "false";
            int pushoffindirectionx = 0;
            string ismoving = "false";
            int detachEndOnSpawn = 0;
            int detachEndOnUse = 0;

            if (ziplineScript.preserveVelocity) preservevelocity = 1;
            if (ziplineScript.dropToBottom) dropToBottom = 1;
            if (ziplineScript.restPoint) restpoint = "true";
            if (ziplineScript.pushOffInDirectionX) pushoffindirectionx = 1;
            if (ziplineScript.isMoving) ismoving = "true";
            if (ziplineScript.detachEndOnSpawn) detachEndOnSpawn = 1;
            if (ziplineScript.detachEndOnUse) detachEndOnUse = 1;

            code += $"    MapEditor_CreateZiplineFromUnity( {Helper.BuildOrigin(ziplineScript.rope_start.gameObject) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(ziplineScript.rope_start.gameObject)}, {Helper.BuildOrigin(ziplineScript.rope_end.gameObject) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(ziplineScript.rope_start.gameObject)}, true, {ziplineScript.fadeDistance.ToString().Replace(",", ".")}, {ziplineScript.scale.ToString().Replace(",", ".")}, {ziplineScript.width.ToString().Replace(",", ".")}, {ziplineScript.speedScale.ToString().Replace(",", ".")}, {ziplineScript.lengthScale.ToString().Replace(",", ".")}, {preservevelocity}, {dropToBottom}, {ziplineScript.autoDetachStart.ToString().Replace(",", ".")}, {ziplineScript.autoDetachEnd.ToString().Replace(",", ".")}, {restpoint}, {pushoffindirectionx}, {ismoving}, {detachEndOnSpawn}, {detachEndOnUse} )" + "\n";

        }

        code += "\n";

        return code;
    }

    public static string NonVerticalZipLines()
    {
        GameObject[] NonVerticalZipLineObjects = GameObject.FindGameObjectsWithTag("NonVerticalZipLine");

        if (NonVerticalZipLineObjects.Length < 1)
            return "";

        string code = "    //NonVerticalZipLines \n";

        foreach (GameObject go in NonVerticalZipLineObjects)
        {
            DrawNonVerticalZipline ziplineScript = go.GetComponent<DrawNonVerticalZipline>();
            if (ziplineScript == null)
                continue;

            int preservevelocity = 0;
            int dropToBottom = 0;
            string restpoint = "false";
            int pushoffindirectionx = 0;
            string ismoving = "false";
            int detachEndOnSpawn = 0;
            int detachEndOnUse = 0;

            if (ziplineScript.preserveVelocity) preservevelocity = 1;
            if (ziplineScript.dropToBottom) dropToBottom = 1;
            if (ziplineScript.restPoint) restpoint = "true";
            if (ziplineScript.pushOffInDirectionX) pushoffindirectionx = 1;
            if (ziplineScript.isMoving) ismoving = "true";
            if (ziplineScript.detachEndOnSpawn) detachEndOnSpawn = 1;
            if (ziplineScript.detachEndOnUse) detachEndOnUse = 1;

            code += $"    MapEditor_CreateZiplineFromUnity( {Helper.BuildOrigin(ziplineScript.rope_start.gameObject) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(ziplineScript.rope_start.gameObject)}, {Helper.BuildOrigin(ziplineScript.rope_end.gameObject) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(ziplineScript.rope_start.gameObject)}, true, {ziplineScript.fadeDistance.ToString().Replace(",", ".")}, {ziplineScript.scale.ToString().Replace(",", ".")}, {ziplineScript.width.ToString().Replace(",", ".")}, {ziplineScript.speedScale.ToString().Replace(",", ".")}, {ziplineScript.lengthScale.ToString().Replace(",", ".")}, {preservevelocity}, {dropToBottom}, {ziplineScript.autoDetachStart.ToString().Replace(",", ".")}, {ziplineScript.autoDetachEnd.ToString().Replace(",", ".")}, {restpoint}, {pushoffindirectionx}, {ismoving}, {detachEndOnSpawn}, {detachEndOnUse} )" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string SingleDoors()
    {
        GameObject[] SingleDoorObjects = GameObject.FindGameObjectsWithTag("SingleDoor");
        if (SingleDoorObjects.Length < 1)
            return "";

        string code = "    //Single Doors \n";

        foreach (GameObject go in SingleDoorObjects) {
            DoorScript script = go.GetComponent<DoorScript>();
            code += $"    MapEditor_SpawnDoor( {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, eMapEditorDoorType.Single, {script.goldDoor.ToString().ToLower()} )" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string DoubleDoors()
    {
        GameObject[] DoubleDoorObjects = GameObject.FindGameObjectsWithTag("DoubleDoor");
        if (DoubleDoorObjects.Length < 1)
            return "";

        string code = "    //Double Doors \n";

        foreach (GameObject go in DoubleDoorObjects) {
            DoorScript script = go.GetComponent<DoorScript>();
            code += $"    MapEditor_SpawnDoor( {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, eMapEditorDoorType.Double, {script.goldDoor.ToString().ToLower()} )" + "\n";
        }

        code += "\n";

        return code;
    }

    public static string VertDoors()
    {
        GameObject[] VertDoorObjects = GameObject.FindGameObjectsWithTag("VerticalDoor");
        if (VertDoorObjects.Length < 1)
            return "";

        string code = "    //Vertical Doors \n";

        foreach (GameObject go in VertDoorObjects)
            code += $"    MapEditor_SpawnDoor( {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, eMapEditorDoorType.Vertical)" + "\n";

        code += "\n";

        return code;
    }

    public static string HorizontalDoors()
    {
        GameObject[] HorzDoorObjects = GameObject.FindGameObjectsWithTag("HorzDoor");
        if (HorzDoorObjects.Length < 1)
            return "";

        string code = "    //Horizontal Doors \n";

        foreach (GameObject go in HorzDoorObjects)
            code += $"    MapEditor_SpawnDoor( {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, eMapEditorDoorType.Horizontal)" + "\n";

        code += "\n";

        return code;
    }

    public static string Props(BuildType type = BuildType.Map)
    {
        GameObject[] PropObjects = GameObject.FindGameObjectsWithTag("Prop");
        if (PropObjects.Length < 1)
            return "";

        string code = "";

        switch(type) {
            case BuildType.Map:
                code += "    //Props \n";
                break;
            case BuildType.DataTable:
                code += "\"type\",\"origin\",\"angles\",\"scale\",\"fade\",\"mantle\",\"visible\",\"mdl\",\"collection\"" + "\n";
                break;
        }

        foreach (GameObject go in PropObjects)
        {
            string model = go.name.Split(char.Parse(" "))[0].Replace("#", "/") + ".rmdl";
            PropScript script = go.GetComponent<PropScript>();

            switch (type) {
                case BuildType.Ent:
                    code += BuildScriptEntItem(go);
                    continue;
                case BuildType.DataTable:
                    code += BuildDataTableItem(go);
                    continue;
                case BuildType.Precache:
                    code += $"    PrecacheModel( $\"{model}\" )" + "\n";
                    continue;
                case BuildType.Map:
                    code += $"    MapEditor_CreateProp( $\"{model}\", {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, {script.allowMantle.ToString().ToLower()}, {script.fadeDistance}, {script.realmID}, {go.transform.localScale.x.ToString().Replace(",", ".")} )" + "\n";
                    continue;
            }
        }

        switch(type) {
            case BuildType.Map:
                code += "\n";
                break;
            case BuildType.DataTable:
                code += "\"string\",\"vector\",\"vector\",\"float\",\"float\",\"bool\",\"bool\",\"asset\",\"string\"";
                break;
        }

        return code;
    }

    public static string Triggers()
    {
        GameObject[] TriggerObjects = GameObject.FindGameObjectsWithTag("Trigger");
        if (TriggerObjects.Length < 1)
            return "";

        string code = "    //Triggers \n";

        int triggerid = 0;
        foreach (GameObject go in TriggerObjects)
        {
            TriggerScripting script = go.GetComponent<TriggerScripting>();
            code += $"    entity trigger" + triggerid + $" = MapEditor_CreateTrigger( {Helper.BuildOrigin(go) + Helper.ShouldAddStartingOrg()}, {Helper.BuildAngles(go)}, {go.transform.localScale.x.ToString().Replace(",", ".")}, {go.transform.localScale.y.ToString().Replace(",", ".")}, {script.Debug.ToString().ToLower()} )" + "\n";

            if (script.EnterCallback != "")
                code += $"    trigger{triggerid}.SetEnterCallback( void function(entity trigger , entity ent)" + "{\n" + script.EnterCallback + "\n    })" + "\n";

            if (script.LeaveCallback != "")
                code += $"    trigger{triggerid}.SetLeaveCallback( void function(entity trigger , entity ent)" + "{\n" + script.LeaveCallback + "\n    })" + "\n";

            code += $"    DispatchSpawn( trigger{triggerid} )" + "\n";
            triggerid++;
        }

        return code;
    }

    public static string BuildDataTableItem(GameObject go)
    {
        string model = go.name.Split(char.Parse(" "))[0].Replace("#", "/") + ".rmdl";
        PropScript script = go.GetComponent<PropScript>();

        string type = "\"dynamic_prop\",";
        string origin = "\"" + Helper.BuildOrigin(go).Replace(" ", "") + "\",";
        string angles = "\"" + Helper.BuildAngles(go).Replace(" ", "") + "\",";
        string scale = go.transform.localScale.x.ToString().Replace(",", ".") + ",";
        string fade = script.fadeDistance.ToString() + ",";
        string mantle = script.allowMantle.ToString().ToLower() + ",";
        string visible = "true,";
        string mdl = "\"" + model + "\",";
        string collection = "\"\"";

        if (go.transform.parent != null) {
            GameObject parent = go.transform.parent.gameObject;
            collection = "\"" + parent.name.Replace("\r", "").Replace("\n", "") + "\"";
        }

        return type + origin + angles + scale + fade + mantle + visible + mdl + collection + "\n";
    }

    public static string BuildScriptEntItem(GameObject go)
    {
        string model = go.name.Split(char.Parse(" "))[0].Replace("#", "/") + ".rmdl";
        PropScript script = go.GetComponent<PropScript>();
        
        string buildent = @"{
""StartDisabled"" ""0""
""spawnflags"" ""0""
""fadedist"" """ + script.fadeDistance + @"""
""collide_titan"" ""1""
""collide_ai"" ""1""
""scale"" """ + go.transform.localScale.x.ToString().Replace(",", ".") + @"""
""angles"" """ + Helper.BuildAngles(go)  + @"""
""origin"" """ + Helper.BuildOrigin(go) + @"""
""targetname"" ""MapEditorProp""
""solid"" ""6""
""model"" """ +  model + @"""
""ClientSide"" ""0""
""classname"" ""prop_dynamic""
}
";
        return buildent;
    }
}