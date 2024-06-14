using UnityEngine;
using UniRx;
namespace MVRP.Items.Models
{
    public class ItemController : MonoBehaviour
    {
        private ItemDataBase itemDataBase;
        private string itemName;
        void Start()
        {
            itemDataBase = Resources.Load<ItemDataBase>("DataBase");
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
        }
        void Update()
        {
            if(itemName == "EscapeKey")
            {
                
            }
        }
        public void GetName(string _itemName)//Playerが獲得したアイテムの名前を取得
        {
            itemName = _itemName;
        }
    }
}