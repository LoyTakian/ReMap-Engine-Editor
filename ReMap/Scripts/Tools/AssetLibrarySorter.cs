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
        string currentDirectory = Directory.GetCurrentDirectory();
        string[] files = Directory.GetFiles(Path.Combine(currentDirectory, @"Assets\ReMap\Scripts\Tools", "rpakModelFile"), "*.txt");
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

            if ( !Directory.Exists( Path.Combine(currentDirectory, @"Assets\Prefabs", mapName ) ) )
            Directory.CreateDirectory( Path.Combine(currentDirectory, @"Assets\Prefabs", mapName ) );

            string modelName ; GameObject prefabToAdd; GameObject objectToAdd; GameObject prefabInstance; GameObject objectInstance;

            foreach ( string modelsPath in arrayMap )
            {
                modelName = Path.GetFileNameWithoutExtension(modelsPath);

                if ( File.Exists(Path.Combine(currentDirectory, @"Assets\ReMap\Lods - Dont use these\Models", modelName + "_LOD0.fbx") ) )
                {
                    if ( !File.Exists(Path.Combine(currentDirectory, @"Assets\Prefabs\" + mapName, modelsPath.Replace(".rmdl", ".prefab").Replace("/", "#")) ) )
                    {
                        prefabToAdd = AssetDatabase.LoadAssetAtPath(@"Assets\ReMap\Lods - Dont use these\EmptyPrefab.prefab", typeof(UnityEngine.Object) ) as GameObject;
                        objectToAdd = AssetDatabase.LoadAssetAtPath( @"Assets\ReMap\Lods - Dont use these\Models" + @"\" + modelName + "_LOD0.fbx", typeof(UnityEngine.Object) )as GameObject;
                            if ( prefabToAdd == null || objectToAdd == null ) return;
                        prefabInstance = UnityEngine.Object.Instantiate(prefabToAdd) as GameObject;
                        objectInstance = UnityEngine.Object.Instantiate(objectToAdd) as GameObject;
                            if ( prefabInstance == null || objectInstance == null ) return;

                        objectInstance.transform.SetParent(prefabInstance.transform);

                        PrefabUtility.SaveAsPrefabAsset( prefabInstance, Path.Combine(currentDirectory, @"Assets\Prefabs\" + mapName, modelsPath.Replace(".rmdl", ".prefab").Replace("/", "#")) );
                    }
                        //GameObject prefab = AssetDatabase.LoadAssetAtPath(@"Assets\Prefabs\" + mapName + @"\" + modelsPath.Replace(".rmdl", ".prefab").Replace("/", "#"), typeof(UnityEngine.Object) ) as GameObject;
                        //GameObject objectToAdd = AssetDatabase.LoadAssetAtPath( @"Assets\ReMap\Lods - Dont use these\Models" + @"\" + modelName + "_LOD0.fbx", typeof(UnityEngine.Object) )as GameObject;
                        //if (prefab == null) return;
                        //if (objectToAdd == null) return;
                        //GameObject prefabInstance = UnityEngine.Object.Instantiate(prefab) as GameObject;
                        //GameObject instance = UnityEngine.Object.Instantiate(objectToAdd) as GameObject;
                        //instance.transform.SetParent(prefabInstance.transform);

                        //PrefabUtility.ApplyAddedGameObject( instance, @"Assets\Prefabs\" + mapName + @"\" + modelsPath.Replace(".rmdl", ".prefab").Replace("/", "#"), InteractionMode.AutomatedAction );
                        //
                        //instance.transform.SetParent(prefabInstance.transform);
                        //PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.AutomatedAction);

                        //UnityEngine.Object.Destroy(instance);

                        //prefab.AddComponent<PropScript>();

                        // TESTS || I will do the code clean up after

                        //GameObject model = (GameObject)Resources.Load(Path.Combine(currentDirectory, @"Assets\ReMap\Lods - Dont use these\Models\", modelName + "_LOD0.fbx"));

                    
                        //if ( model != null ){
                        //GameObject instance = /* (GameObject)PrefabUtility.Object. */UnityEngine.Object.Instantiate(model);}

                        //PrefabUtility.SaveAsPrefabAsset(emptyPrefab, Path.Combine(currentDirectory, @"Assets\Prefabs\" + mapName, modelsPath.Replace(".rmdl", "").Replace("/", "#") + ".prefab" ) );

                        //UnityEngine.Object.Destroy(emptyPrefab);
                }
            }
        }
    }
}