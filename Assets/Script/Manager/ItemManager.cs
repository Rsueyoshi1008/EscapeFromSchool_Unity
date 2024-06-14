using System.Collections.Generic;
using MVRP.Items.Models;
using MVRP.Item.Models;
using UnityEngine;
using UnityEngine.Events;
namespace MVRP.Item.Managers
{
    public class ItemManager : MonoBehaviour
    {
        private ItemDataBase itemDataBase;
        [SerializeField] private ItemSpawn _itemSpawn;
        
        public UnityAction<string,Sprite> _viewItem;
        public UnityAction releaseEscape;
        
        private string getItemName;
        
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
            //_itemSpawn.EscapeItemSpawn();
        }
        void Start()
        {
            //  テスト用
            if(itemDataBase == null)
            {
                itemDataBase = Resources.Load<ItemDataBase>("DataBase/ItemDataBase");
            }
        }
        //階段扉のDoorControllerを取得し、鍵を獲得したらその階のDoorControllerをActiveにする
        void Update()
        {
            
            if(getItemName == "EscapeKey")//  脱出扉の開放条件クリア
            {
                releaseEscape?.Invoke();
            }
            foreach(MainItem status in itemDataBase.items)//    取得したアイテムの情報の情報
            {
                if(status.objectName != "")//   空欄じゃなかったら
                {
                    if(status.objectName == getItemName)
                    {
                        _viewItem?.Invoke(status.itemName,status.itemImage);//    PlayerViewにアイテム名の表示
                        getItemName = "null";
                        
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