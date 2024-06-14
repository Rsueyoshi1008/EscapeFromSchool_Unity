using UnityEditor;
using UnityEngine;

public class MeshReadWriteEnabler : EditorWindow
{
    [MenuItem("Tools/Enable Read/Write for Meshes")]
    static void EnableReadWrite()
    {
        /*
        // プロジェクト内の全てのメッシュアセットを取得
        string[] guids = AssetDatabase.FindAssets("t:Mesh");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                importer.isReadable = false; // Read/Write Enabledを有効にする
                importer.SaveAndReimport(); // 再インポート
            }
        }
        Debug.Log("All meshes have been set to Read/Write Enabled.");
        */
    }
}