using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDataBase", menuName = "CreatePlayerDataBase")]
public class PlayerDataBase : ScriptableObject
{
    public List<PlayerStatus> playerStatus = new List<PlayerStatus>();
}

