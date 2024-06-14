using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StudentsDataBase", menuName = "CreateStudentsDataBase")]
public class StudentsDataBase : ScriptableObject
{
    public List<StudentStatus> studentStatus = new List<StudentStatus>();
}