using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace MVRP.Item.Models
{
    public class ItemSpawn : MonoBehaviour
    {
        // アイテムごとのスポーンポイントを管理する辞書
        private Dictionary<string, List<Transform>> itemSpawnPoints = new Dictionary<string, List<Transform>>();
        private Dictionary<string, GameObject> spawnedItems = new Dictionary<string, GameObject>();
        private Dictionary<string, bool> isSpawn = new Dictionary<string, bool>();
        //  取得するアイテム名
        [SerializeField] private List<string> spawnItemObjectName = new List<string>();
        // アイテムごとのスポーンポイントフォルダ
        [SerializeField] private List<GameObject> spawnPointParentObjects;
        //  アイテムオブジェクトを格納
        private List<GameObject> prefabObject;
        //  生成したオブジェクトの管理オブジェクト
        [SerializeField] private GameObject spawnItemObjectManager;
        //  イベント
        public Func<List<string>, List<GameObject>> getItemObject;
        public Func<string, bool> getIsSpawn;
        public Func<string, int> getPreviousRandom;
        public Func<string, Vector2> getMaxSpawnCountAndSpawnCount;
        public UnityAction<string, int> setPreviousRandom;
        public UnityAction<string> setIsSpawn;
        public UnityAction<string,int> setSpawnCount;
        private bool itemSpawned = false;
        // コルーチンの定義
        private IEnumerator GetItemObjectAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);
            prefabObject = getItemObject?.Invoke(spawnItemObjectName);
        }
        public void Initialize()
        {
            itemSpawned = false;
            // それぞれのアイテム名ごとに初期化
            foreach (var itemName in spawnItemObjectName)
            {
                // スポーンポイントリストを初期化
                if (!isSpawn.ContainsKey(itemName))
                {
                    isSpawn[itemName] = true;
                }
            }
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
                SpawnItemIfNameExists("PerfectItem");
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
                            }

                            
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
            Vector2 maxSpawnCountAndSpawnCount = getMaxSpawnCountAndSpawnCount?.Invoke(itemName) ?? new Vector2 (0,0);
            float max = maxSpawnCountAndSpawnCount.x;
            //   アイテムが生成可能かを確認する
            //   可能な時はtrueを返す
            if(CheckItemSpawnEligibility(itemName))
            {
                GameObject itemToSpawn = null;
                foreach (GameObject item in prefabObject)//  脱出用のアイテムオブジェクト取得
                {
                    if (item.name == itemName)
                    {
                        itemToSpawn = item;
                    }
                }

                List<Transform> spawnEscapePoints = GetSpawnPoints(itemName);// 生成位置の取得
                if (spawnEscapePoints.Count == 0)// 生成地点がなかった場合何もしない
                {
                    return;
                }
                else
                {
                    for(int i = 0; i < max; i++)//  生成上限までアイテムを生成する
                    {
                        // 新しいアイテムを生成
                        GameObject newItem = SpawnNewItem(itemToSpawn,itemName);
                        spawnedItems[itemName] = newItem;// 生成したアイテムを記録
                    }
                }
                setIsSpawn?.Invoke(itemName);
            }
            
        }
        //  スポーン地点をランダムに選んでそこに移動する
        public void UpdateItemPosition(string itemName)
        {
            Debug.Log("UpdatePosition");
            int tmpPreviousRandom = getPreviousRandom?.Invoke(itemName) ?? -1;
            List<Transform> spawnPoints = GetSpawnPoints(itemName);
            int randomIndex = RandomPosition(spawnPoints.Count,tmpPreviousRandom);
            Vector3 newSpawnPosition = spawnPoints[randomIndex].position;
            spawnedItems[itemName].transform.position = newSpawnPosition; // 位置を更新
            setPreviousRandom?.Invoke(itemName, randomIndex);//   乱数の記録
        }
        private GameObject SpawnNewItem(GameObject item, string itemName)
        {
            int tmpPreviousRandom = getPreviousRandom?.Invoke(itemName) ?? -1;
            List<Transform> spawnPoints = GetSpawnPoints(itemName);
            int randomIndex = RandomPosition(spawnPoints.Count,tmpPreviousRandom);
            Vector3 spawnPosition = spawnPoints[randomIndex].position;
            Quaternion spawnRotation = item.transform.rotation;
            GameObject newItem = Instantiate(item, spawnPosition, spawnRotation, spawnItemObjectManager.transform);
            setPreviousRandom?.Invoke(itemName, randomIndex);//   乱数の記録
            setSpawnCount?.Invoke(itemName, 1);//  生成した数を追加
            return newItem;
        }
        private bool CheckItemSpawnEligibility(string itemName)
        {
            //  生成可能数と生成した数を同時取得
            Vector2 maxSpawnCountAndSpawnCount = getMaxSpawnCountAndSpawnCount?.Invoke(itemName) ?? new Vector2 (0,0);
            float maxSpawn = maxSpawnCountAndSpawnCount.x;
            float spawnCount = maxSpawnCountAndSpawnCount.y;
            bool isSpawn = false;
            bool spawn = getIsSpawn?.Invoke(itemName) ?? false;
            foreach (GameObject item in prefabObject)//  アイテムがあるかの確認
            {
                if (item.name == itemName)
                {
                    isSpawn = true;
                }
            }
            //   生成最大数より生成した数が多くなったらreturn
            if(maxSpawn <= spawnCount && spawn == true && isSpawn == false)
            {
                return false;
            }
            else return true;
            
        }
        private int RandomPosition(int range, int previousRandom)
        {
            if (range <= 1) return 0; // rangeが1以下の場合は常に0を返す
            int random;
            do
            {
                random = UnityEngine.Random.Range(0, range); //  二回目以降のアイテム生成で同じ場所に生成されないようにしてる
            } while (random == previousRandom);
            return random;
        }
    }
}

