using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
[CreateAssetMenu(fileName = "Items", menuName = "CreateItem")]
public class MainItem : ScriptableObject
{
    public string objectName; //オブジェクトの名前
    public string itemName; //画面に表示するアイテムの名前
    public Sprite itemImage; //アイテムの画像
    public bool unlock; //アイテムの開放条件
    public GameObject gameObject;   //アイテムの本体
    public float effectiveTime; //アイテムの効果時間
    public int maxSpawnCount;  //生成したい上限数
    [HideInInspector] public int previousRandom;  //同じ場所にアイテムを生成しないために乱数を記録する
    [HideInInspector] public bool isSpawn;    //生成されているかを保持
    [HideInInspector] public int spawnCount;  //生成された数
    
}