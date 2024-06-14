using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Status", menuName = "CreatePlayerStatus")]
public class PlayerStatus : ScriptableObject
{
    public string type; // 種類
    public string value; // 情報
    public Sprite sprite; // 画像(追加)
    

    // public PlayerStatus(PlayerStatus status)
    // {
    //     this.type = status.type;
    //     this.value = status.value;
    //     this.sprite = status.sprite; // 画像(追加)
    // }
    
    
}