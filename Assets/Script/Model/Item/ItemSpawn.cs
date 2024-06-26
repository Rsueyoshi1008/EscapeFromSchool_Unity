using System;
using UniRx;
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
        // UniRxのSubjectを使用してリクエストとレスポンスを管理
        public IObservable<List<GameObject>> GetItemObjectResponse => _getItemObjectResponse.AsObservable();
        private readonly Subject<List<GameObject>> _getItemObjectResponse = new Subject<List<GameObject>>();
        public IObservable<List<string>> GetItemObjectRequest => _getItemObjectRequest.AsObservable();
        private readonly Subject<List<string>> _getItemObjectRequest = new Subject<List<string>>();

        // UniRxのSubjectを使用してイベントを管理
        private readonly Subject<(string, int)> _setPreviousRandomSubject = new Subject<(string, int)>();
        private readonly Subject<string> _setIsSpawnSubject = new Subject<string>();
        private readonly Subject<(string, int)> _setSpawnCountSubject = new Subject<(string, int)>();

        public IObservable<(string, int)> SetPreviousRandom => _setPreviousRandomSubject.AsObservable();
        public IObservable<string> SetIsSpawn => _setIsSpawnSubject.AsObservable();
        public IObservable<(string, int)> SetSpawnCount => _setSpawnCountSubject.AsObservable();

        public Func<string, bool> getIsSpawn;
        public Func<string, int> getPreviousRandom;
        public Func<string, Vector2> getMaxSpawnCountAndSpawnCount;

        private bool itemSpawned = false;
        // コルーチンの定義
        private IEnumerator GetItemObjectAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);
            _getItemObjectRequest.OnNext(spawnItemObjectName);
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
            StartCoroutine(GetItemObjectAfterDelay(1f));
            // アイテムのSpawn地点の設定
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
            float spawnCount = maxSpawnCountAndSpawnCount.y;
            //   アイテムが生成可能かを確認する
            //   可能な時はtrueを返す
            if(CheckItemSpawnEligibility(itemName))
            {
                Debug.Log("アイテム生成開始");
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
                    Debug.Log("スポーン位置なし");
                    return;
                }
                else
                {
                    Debug.Log("アイテムの生成その4");
                    if(spawnCount <= max)
                    {
                        if(spawnCount != 0)
                        {
                            spawnCount--;
                        }
                        for(int i = (int)spawnCount; i < max; i++)//  生成上限までアイテムを生成する
                        {
                            // 新しいアイテムを生成
                            GameObject newItem = SpawnNewItem(itemToSpawn,itemName);
                            spawnedItems[itemName] = newItem;// 生成したアイテムを記録
                            
                        }
                    }
                    
                }
                _setIsSpawnSubject.OnNext(itemName);
            }
            else
            {
                Debug.LogError("アイテム生成チェックが通らない!");
                // ゲームの実行を停止する
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
            
        }
        public void SetPrefabObject(List<GameObject> objects)
        {
            prefabObject = objects;
        }
        public void PublishItemObjectResponse(List<GameObject> response)
        {
            _getItemObjectResponse.OnNext(response);
        }
        //  スポーン地点をランダムに選んでそこに移動する
        public void UpdateItemPosition(string itemName)
        {
            if (!spawnedItems.ContainsKey(itemName) || spawnedItems[itemName] == null)
            {
                // 位置を更新しようとしているオブジェクトがないときは何もしない
                return;
            }
            int tmpPreviousRandom = getPreviousRandom?.Invoke(itemName) ?? -1;
            List<Transform> spawnPoints = GetSpawnPoints(itemName);
            int randomIndex = RandomPosition(spawnPoints.Count,tmpPreviousRandom);
            Vector3 newSpawnPosition = spawnPoints[randomIndex].position;
            spawnedItems[itemName].transform.position = newSpawnPosition; // 位置を更新
            _setPreviousRandomSubject.OnNext((itemName, randomIndex));//   乱数の記録
        }
        private GameObject SpawnNewItem(GameObject item, string itemName)
        {
            int tmpPreviousRandom = getPreviousRandom?.Invoke(itemName) ?? -1;
            List<Transform> spawnPoints = GetSpawnPoints(itemName);
            int randomIndex = RandomPosition(spawnPoints.Count,tmpPreviousRandom);
            Vector3 spawnPosition = spawnPoints[randomIndex].position;
            Quaternion spawnRotation = item.transform.rotation;
            GameObject newItem = Instantiate(item, spawnPosition, spawnRotation, spawnItemObjectManager.transform);
            _setPreviousRandomSubject.OnNext((itemName, randomIndex));//   乱数の記録
            _setSpawnCountSubject.OnNext((itemName, 1));//  生成した数を追加
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
