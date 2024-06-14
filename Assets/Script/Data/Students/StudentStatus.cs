using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Status", menuName = "CreateStudentStatus")]
public class StudentStatus : ScriptableObject
{
    public string studentName;//    生徒の名前
    public float maxSpeed;//    maxの速度
    public float minSpeed;//    minの速度
    public Vector3 scale;// 生徒のサイズ
}