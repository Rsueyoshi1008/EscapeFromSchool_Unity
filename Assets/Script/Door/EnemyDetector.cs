using UnityEngine;
using UnityEngine.Events;
using MVRP.Students.Models;
namespace MVRP.Doors.Models
{
    public class EnemyDetector : MonoBehaviour
    {
        public UnityAction onEnemyEnter,rotationDoor;
        void Start()
        {
            //
        }
        void Update()
        {
            //
        }
        private void OnTriggerEnter(Collider other)
        {
            //接触
            string objectTag = other.gameObject.tag;
            if(objectTag == "Student")
            {
                rotationDoor?.Invoke();
                //  検知した敵のスクリプトにアクセスし、移動を一時的に止める
                StudentsTraffic _studentsTraffic = other.GetComponent<StudentsTraffic>();
                _studentsTraffic.movementFlag = true;
                Debug.Log("生徒を検知");
            }
        }
    }
}