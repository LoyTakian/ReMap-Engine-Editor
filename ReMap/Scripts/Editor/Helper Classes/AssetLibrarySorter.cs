using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class AssetLibrarySorter
{
    public static void LibrarySorter()
    {
        int totalFiles = 0;
        Array[] totalArrayMap = new Array[20];
        string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Assets/ReMap/Scripts/Editor/Helper Classes", "TextFile"), "*.txt");

        foreach (string file in files)
        {
            int lineCount = File.ReadAllLines(file).Length;

            string[] arrayMap = new string[lineCount];

            string mapPath = file;
            string mapName = Path.GetFileNameWithoutExtension(file);

            int row = 0;

            using (StreamReader reader = new StreamReader(mapPath))
            {
                string modelPath;
                while ((modelPath = reader.ReadLine()) != null)
                {
                    arrayMap[row] = modelPath;
                    row++;
                }
            }

            totalArrayMap[totalFiles] = arrayMap;
            totalFiles++;

            if ( !Directory.Exists( Path.Combine(Directory.GetCurrentDirectory(), "Assets/Prefabs", mapName ) ) )
            Directory.CreateDirectory( Path.Combine(Directory.GetCurrentDirectory(), "Assets/Prefabs", mapName ) );

            string modelPath_; string modelName;

            foreach ( string models in arrayMap )
            {
                modelPath_ = models;
                modelName = Path.GetFileNameWithoutExtension(modelPath_);

                if ( File.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"Assets\ReMap\Lods - Dont use these\Models", modelName + "_LOD0.fbx") ) )
                {
                    if ( !File.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Prefabs\" + mapName, modelPath_.Replace(".rmdl", "").Replace("/", "#") + ".prefab") ) )
                        File.Copy(Path.Combine(Directory.GetCurrentDirectory(), @"Assets\ReMap\Lods - Dont use these\", "EmptyPrefab.prefab"), Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Prefabs\" + mapName, modelPath_.Replace(".rmdl", "").Replace("/", "#") + ".prefab") );

                        //GameObject prefab = Resources.Load(Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Prefabs\" + mapName, modelPath_.Replace(".rmdl", "").Replace("/", "#"))) as GameObject;
                        //GameObject parent = UnityEngine.Object.Instantiate(prefab);

                        //prefab.AddComponent<PropScript>();

                    // TESTS || I will do the code clean up after

                    //GameObject model = (GameObject)Resources.Load(Path.Combine(Directory.GetCurrentDirectory(), @"Assets\ReMap\Lods - Dont use these\Models\", modelName + "_LOD0.fbx"));

                    
                    //if ( model != null ){
                    //GameObject instance = /* (GameObject)PrefabUtility.Object. */UnityEngine.Object.Instantiate(model);}

                    //PrefabUtility.SaveAsPrefabAsset(emptyPrefab, Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Prefabs\" + mapName, modelPath_.Replace(".rmdl", "").Replace("/", "#") + ".prefab" ) );

                    //UnityEngine.Object.Destroy(emptyPrefab);
                }
            }
        }
    }
}