using UnityEngine;
using TMPro;
using UnityEngine.AI;
namespace MVRP.Doors.Models
{
    public class DoorController : MonoBehaviour
    {
        //  outLineスクリプトの制御
        [SerializeField] private Outline outline;
        bool isOutLine = false;
        //  扉移動の際のNavMeshを更新するためのやつ
        public NavMeshObstacle navMeshObstacle;
        //  扉の回転に関与するやつ
        public bool idDoorOpen;
        float startEulerAnglesY;//扉の最初の角度を格納
        float maxRotationAngle = 90f;//    ドアが開く上限
        float minRotationAngle = 1f;//  ドアが閉まる上限
        bool escapeDoor = false;
        bool obstacleEnabled = false;// 一度だけ処理を行うフラグ
        public void initialize()
        {
            if (gameObject.name == "EscapeDoor")
            {
                //脱出用の扉の処理
                escapeDoor = true;
            }
            idDoorOpen = false;
        }
        void Start()
        {
            //扉の最初の角度を格納
            startEulerAnglesY = transform.rotation.eulerAngles.y;
            if (gameObject.name == "EscapeDoor")
            {
                //脱出用の扉の処理
                escapeDoor = true;
            }

        }
        // Update is called once per frame
        void Update()
        {
            
            if (escapeDoor == false)//   脱出扉だけ回転処理を停止
            {
                if(isOutLine == true)
                {
                    outline.enabled = true;
                }
                else outline.enabled = false;
                if (idDoorOpen == true)
                {
                    OpeningAndClosingDoors(50f);
                }
                else OpeningAndClosingDoors(-50f);
                // 扉が開いた/閉じたときのナビメッシュ更新
                if (navMeshObstacle.enabled == true)
                {
                    if (!obstacleEnabled)
                    {
                        navMeshObstacle.enabled = false;
                    }
                }
                
            }
            

        }
        public void OpeningAndClosingDoors(float rotationSpeed)
        {
            float currentRotationY = transform.rotation.eulerAngles.y;

            // 回転角度を0-360度範囲に正規化
            float normalizedCurrentRotation = NormalizeAngle(currentRotationY);
            float normalizedStartRotation = NormalizeAngle(startEulerAnglesY);

            // ドアが開く方向の処理
            if (rotationSpeed > 0)
            {
                if (AngleDifference(normalizedCurrentRotation, normalizedStartRotation) < maxRotationAngle)
                {
                    transform.rotation *= Quaternion.Euler(0, Time.deltaTime * rotationSpeed, 0);
                    obstacleEnabled = false;//  回転を始めたときにフラグの初期化
                }
                if (!obstacleEnabled && AngleDifference(normalizedCurrentRotation, normalizedStartRotation) >= maxRotationAngle)
                {
                    navMeshObstacle.enabled = true;
                    obstacleEnabled = true;
                }
            }
            // ドアが閉じる方向の処理
            else if (rotationSpeed < 0)
            {
                if (AngleDifference(normalizedCurrentRotation, normalizedStartRotation) > minRotationAngle)
                {
                    transform.rotation *= Quaternion.Euler(0, Time.deltaTime * rotationSpeed, 0);
                    obstacleEnabled = false;
                }
                if (!obstacleEnabled && AngleDifference(normalizedCurrentRotation, normalizedStartRotation) <= minRotationAngle)
                {
                    navMeshObstacle.enabled = true;
                    obstacleEnabled = true;
                }
            }

        }
        public void GetIsOutLine(bool _isOutLine)// アウトラインの表示制御
        {
            isOutLine = _isOutLine;
        }
        public void ReleaseEscape()
        {
            escapeDoor = false;//   脱出扉も回転処理を受けるようにする
        }
        public void DoorRotation(bool _idDoorOpen)
        {
            idDoorOpen = _idDoorOpen;
        }
        private float NormalizeAngle(float angle)
        {
            angle = angle % 360;
            if (angle < 0)
                angle += 360;
            return angle;
        }
        private float AngleDifference(float angle1, float angle2)
        {
            float diff = angle1 - angle2;
            if (diff < 0)
                diff += 360;
            return diff;
        }
    }
}


