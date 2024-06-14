using UnityEngine;
namespace MVRP.Doors.Models
{
    public class EnemyOnlyDoorController : MonoBehaviour
    {
        public bool idDoorOpen;
        float startEulerAnglesY;//扉の最初の角度を格納
        float maxRotationAngle = 90f;//    ドアが開く上限
        float minRotationAngle = 1f;//  ドアが閉まる上限

        bool obstacleEnabled = false;// 一度だけ処理を行うフラグ

        // Update is called once per frame
        void Update()
        {
            
            if (idDoorOpen == true)
            {
                OpeningAndClosingDoors(50f);
            }
            else OpeningAndClosingDoors(-50f);


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
                    obstacleEnabled = true;
                }
            }

        }
        public void DoorRotation()
        {
            idDoorOpen = !idDoorOpen;
            
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


