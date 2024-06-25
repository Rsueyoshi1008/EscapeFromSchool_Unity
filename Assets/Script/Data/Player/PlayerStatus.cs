using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Status", menuName = "CreatePlayerStatus")]
public class PlayerStatus : ScriptableObject
{
    public string type; // 種類
    public string value; // 情報
}