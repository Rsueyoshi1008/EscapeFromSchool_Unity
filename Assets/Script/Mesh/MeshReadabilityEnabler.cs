using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
public class MeshReadabilityEnabler : MonoBehaviour
{ 
    [SerializeField] private List<GameObject> changeMeshObjects;
    public void Initialize()
    {
        foreach (GameObject changeObject in changeMeshObjects)
        {
            MeshFilter meshFilter = changeObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    string path = AssetDatabase.GetAssetPath(mesh);
                    ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer != null)
                    {
                        importer.isReadable = true; // Read/Write Enabledを有効にする
                        importer.SaveAndReimport(); // 再インポート
                        Debug.Log("Mesh変更");
                    }
                }
            }
        }
        Debug.Log("All door meshes have been set to Read/Write Enabled.");
    }
    
}
