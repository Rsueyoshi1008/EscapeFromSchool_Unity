using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using UnityEngine.Events;
namespace MVRP.Item.Managers
{
    public class ItemManager : MonoBehaviour
    {
        // アイテムごとのスポーンポイントを管理する辞書
        private Dictionary<string,float> itemsEffectiveTime = new Dictionary<string, float>();
        private ItemDataBase itemDataBase;
        //  イベント
        public UnityAction<string,Sprite> _viewItem;
        public UnityAction releaseEscape;
        public Subject<Tuple<int, float>> onRevealEvent = new Subject<Tuple<int, float>>();
        public UnityAction<bool> viewEscapeKey;
        public UnityAction<string> spawnItem;
        public UnityAction<float> ViewEffectiveTime;
        
        private string getItemName;
        // コルーチンの定義
        private IEnumerator GetItemObjectAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);
            viewEscapeKey?.Invoke(false);
        }
        public void Initialize()
        {
            itemDataBase = Resources.Load<ItemDataBase>("DataBase/ItemDataBase");
            //データを読み込めなかった時のエラー処理
            if(itemDataBase == null)
            {
                Debug.LogError("ItemDataBase is not found!");
                // ゲームの実行を停止する
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
            //  PlayerViewのテキストを初期化
            _viewItem?.Invoke("",null);
            onRevealEvent.OnNext(Tuple.Create(1,0f));//   敵が透ける効果をリセット
        }
        void Start()
        {
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                // アイテムの効果時間を初期化
                if (!itemsEffectiveTime.ContainsKey(status.itemName))
                {
                    itemsEffectiveTime[status.objectName] = status.effectiveTime;
                }

            }
        }
        //階段扉のDoorControllerを取得し、鍵を獲得したらその階のDoorControllerをActiveにする
        void Update()
        {
            
            if(getItemName == "EscapeKey")//  脱出扉の開放条件クリア
            {
                releaseEscape?.Invoke();
            }
            if(getItemName == "TransparencyItem")
            {
                if(itemsEffectiveTime.ContainsKey("TransparencyItem"))
                {
                    onRevealEvent.OnNext(Tuple.Create(1,itemsEffectiveTime["TransparencyItem"]));
                    ViewEffectiveTime?.Invoke(itemsEffectiveTime["TransparencyItem"]);
                    viewEscapeKey?.Invoke(true);
                    spawnItem?.Invoke("TransparencyItem");
                    SetSpawnCount("TransparencyItem", -1);
                    StartCoroutine(GetItemObjectAfterDelay(itemsEffectiveTime["TransparencyItem"]));
                }
            }
            if(getItemName == "PerfectItem")
            {
                onRevealEvent.OnNext(Tuple.Create(1,itemsEffectiveTime["PerfectItem"]));
                ViewEffectiveTime?.Invoke(itemsEffectiveTime["PerfectItem"]);
                viewEscapeKey?.Invoke(true);
                StartCoroutine(GetItemObjectAfterDelay(itemsEffectiveTime["PerfectItem"]));
            }
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                if(status.objectName != "")//   空欄じゃなかったら
                {
                    if(status.objectName == getItemName)//    PlayerViewにアイテム名の表示
                    {
                        _viewItem?.Invoke(status.itemName,status.itemImage);
                        status.isSpawn = false;
                        getItemName = "null";
                        
                    }
                    
                }
            }
        }
        public bool GetIsSpawn(string itemName)
        {
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                if(status.objectName != "")//   空欄じゃなかったら
                {
                    if(status.objectName == itemName)//    PlayerViewにアイテム名の表示
                    {
                        return status.isSpawn;
                    }
                    
                }
            }
            return true;
        }
        public void SetIsSpawn(string itemName)
        {
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                if(status.objectName != "")//   空欄じゃなかったら
                {
                    if(status.objectName == itemName)
                    {
                        status.isSpawn = true;
                    }
                    
                }
            }
        }
        public Vector2 MaxSpawnCountAndSpawnCount(string itemName)
        {
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                if(status.objectName != "")//   空欄じゃなかったら
                {
                    if(status.objectName == itemName)
                    {
                        return new Vector2(status.maxSpawnCount,status.spawnCount);
                    }
                    
                }
            }
            return new Vector2(0,0);
        }
        public void SetSpawnCount(string itemName, int count)
        {
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                if(status.objectName != "")//   空欄じゃなかったら
                {
                    if(status.objectName == itemName)
                    {
                        status.spawnCount += count;
                    }
                    
                }
            }
        }
        public int GetPreviousRandom(string itemName)
        {
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                if(status.objectName != "")//   空欄じゃなかったら
                {
                    if(status.objectName == itemName)//    PlayerViewにアイテム名の表示
                    {
                        return status.previousRandom;
                    }
                    
                }
            }
            return -1;
        }
        public void SetPreviousRandom(string itemName, int random)
        {
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                if(status.objectName != "")//   空欄じゃなかったら
                {
                    if(status.objectName == itemName)//    PlayerViewにアイテム名の表示
                    {
                        status.previousRandom = random;
                    }
                    
                }
            }
        }
        
        public void GetName(string _itemName)//Playerが獲得したアイテムの名前を取得
        {
            getItemName = _itemName;
        }
        public string ItemSearch(string spriteName)
        {
            string itemName = "";
            foreach(MainItem status in itemDataBase.items)
            {
                if(status.itemImage != null)//   画像が入っていたら
                {
                    if(status.itemImage.name == spriteName)
                    {
                        itemName = status.itemName;
                    }
                    
                }
                
            }
            return itemName;
        }
        public List<GameObject> GetItemObject(List<string> itemNames)
        {
            List<GameObject> items = new List<GameObject>();
            foreach(string itemName in itemNames)
            {
                foreach(MainItem status in itemDataBase.items)//    取得したいアイテムを検索し、ItemObjectを返す
                {
                    if(status.itemImage != null && status.objectName == itemName)//   画面に表示する画像が入っていてオブジェクト名が一致していたら
                    {
                        status.gameObject.name = itemName;//    オブジェクト名を変更する
                        items.Add(status.gameObject);
                    }
                }
            }
            return items;
        }
    }
}