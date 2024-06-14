using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MVRP.Item.Models
{
    public class ItemSpawn : MonoBehaviour
    {
        // アイテムごとのスポーンポイントを管理する辞書
        private Dictionary<string, List<Transform>> itemSpawnPoints = new Dictionary<string, List<Transform>>();
        //  取得するアイテム名
        [SerializeField] private List<string> spawnItemObjectName = new List<string>();
        // アイテムごとのスポーンポイントフォルダ
        [SerializeField] private List<GameObject> spawnPointParentObjects;
        //  アイテムオブジェクトを格納
        private List<GameObject> prefabObject;
        //  生成したオブジェクトの管理オブジェクト
        [SerializeField] private GameObject spawnItemObjectManager;
        //  イベント
        public Func<List<string>, List<GameObject>> _getItemObject;
        private bool itemSpawned = false;
        private int previousRandom = -1;
        // コルーチンの定義
        private IEnumerator GetItemObjectAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);
            prefabObject = _getItemObject?.Invoke(spawnItemObjectName);
        }
        public void Initialize()
        {
            itemSpawned = false;
        }
        void Start()
        {
            // 1秒後にアイテムオブジェクトを取得するコルーチンを開始
            //  ディレイをかけないとイベントの紐づけが完了する前に呼ばれちゃう
            StartCoroutine(GetItemObjectAfterDelay(1f));
            //  アイテムのSpawn地点の設定
            InitializeSpawnPoints();
        }
        void Update()
        {
            if (prefabObject != null && !itemSpawned)
            {
                //  アイテムの生成
                SpawnItemIfNameExists("EscapeKey");
                SpawnItemIfNameExists("TransparencyItem");
                itemSpawned = true;
            }
            
        }
        void InitializeSpawnPoints()
        {
            //  スポーン地点の管理オブジェクトが入っているか
            if (spawnPointParentObjects != null && spawnPointParentObjects.Count > 0)
            {
                // それぞれのアイテム名ごとに初期化
                foreach (var itemName in spawnItemObjectName)
                {
                    // スポーンポイントリストを初期化
                    if (!itemSpawnPoints.ContainsKey(itemName))
                    {
                        itemSpawnPoints[itemName] = new List<Transform>();
                    }
                }
                //  スポーン地点の親オブジェクトの数だけ回す
                for (int i = 0; i < spawnPointParentObjects.Count; i++)
                {
                    var parentObject = spawnPointParentObjects[i];
                    if (parentObject != null)
                    {
                        // 対応するアイテム名を取得
                        if (i < spawnItemObjectName.Count)
                        {
                            string itemName = spawnItemObjectName[i];

                            // スポーン地点の親オブジェクト内の全ての子オブジェクトをスポーンポイントとして追加
                            int count = parentObject.transform.childCount;
                            for (int j = 0; j < count; j++)
                            {
                                Transform child = parentObject.transform.GetChild(j);
                                itemSpawnPoints[itemName].Add(child);
                                Debug.Log("child  /  " + child.position);
                            }

                            Debug.Log("アイテム " + itemName + " / " + itemSpawnPoints[itemName].Count + " 追加");
                        }
                        else
                        {
                            Debug.LogWarning("対応するアイテム名が不足しています。");
                        }
                    }
                    else
                    {
                        // 親オブジェクトがnullであれば警告を出力
                        Debug.LogWarning("A parentObject in spawnPointParentObjects is null.");
                    }
                }
                
            }
            else
            {
                // 目標地点管理オブジェクトがなかった場合
                Debug.LogError("ItemSpawnPointFolders is empty or null, navmesh does not work. (Scene object " + gameObject.name + ").");
            }
        }
        // 指定したアイテムのスポーンポイントを取得
        public List<Transform> GetSpawnPoints(string itemName)
        {
            if (itemSpawnPoints.ContainsKey(itemName))
            {
                return itemSpawnPoints[itemName];
            }
            else
            {
                Debug.LogWarning("No spawn points found for item: " + itemName);
                return new List<Transform>();
            }
        }
        public void SpawnItemIfNameExists(string itemName)
        {
            bool isSpawn = false;
            foreach (GameObject item in prefabObject)//  アイテムがあるかの確認
            {
                if (item.name == itemName)
                {
                    isSpawn = true;
                }
            }
            if(isSpawn == true)//   該当するアイテムが存在したときに動く
            {
                ResetItemPosition(itemName);
                Debug.Log("アイテム生成中");
                GameObject escapeItem = null;
                int tmpPreviousRandom = -1;
                foreach (GameObject item in prefabObject)//  脱出用のアイテムオブジェクト取得
                {
                    if (item.name == itemName)
                    {
                        escapeItem = item;
                        tmpPreviousRandom = previousRandom;
                    }
                }

                List<Transform> spawnEscapePoints = GetSpawnPoints(itemName);
                Debug.Log(spawnEscapePoints[0].position);
                if (spawnEscapePoints.Count == 0)// 生成地点がなかった場合何もしない
                {
                    return;
                }
                else
                {
                    int random = RandomPosition(spawnEscapePoints.Count,tmpPreviousRandom);
                    tmpPreviousRandom = random;
                    Vector3 spawnPosition = spawnEscapePoints[random].position;// 生成場所の取得
                    Quaternion spawnRotation = escapeItem.gameObject.transform.rotation;//  プレハブのものを参照
                    GameObject newObject = Instantiate(escapeItem, spawnPosition, spawnRotation,spawnItemObjectManager.transform);//    parentObjectを親としてオブジェクトを生成
                    //  位置の初期化
                    newObject.transform.position = spawnPosition;
                    newObject.transform.rotation = spawnRotation;
                    Debug.Log("アイテム生成完了  /" + "spawnPosition  = " + spawnEscapePoints[0].position + "  /ItemName " +itemName);
                    Debug.Log("Random " + random);
                }
                foreach (GameObject item in prefabObject)// 乱数の記録
                {
                    if (item.name == itemName)
                    {
                        previousRandom = tmpPreviousRandom;
                    }
                }
            }
            else
            {
                Debug.Log("not found item  =" + itemName);
            }
        }
        
        private int RandomPosition(int range, int previousRandom)
        {
            int random;
            do
            {
                random = UnityEngine.Random.Range(0, range); //  二回目以降のアイテム生成で同じ場所に生成されないようにしてる
            } while (random == previousRandom);
            return random;
        }
        //  指定したアイテムの位置を移動させる
        private void ResetItemPosition(string itemName)
        {
            List<Transform> spawnEscapePoints = GetSpawnPoints(itemName);
            // 脱出アイテムを削除
            foreach (Transform child in spawnEscapePoints)
            {
                child.position = new Vector3(0f,0f,0f);
            }
        }
    }
}

