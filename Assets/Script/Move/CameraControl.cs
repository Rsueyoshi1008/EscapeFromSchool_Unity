using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
namespace MVRP.Cameras.Models
{
    public class CameraControl : MonoBehaviour
    { 
        private Transform enemyTransform;
        public UnityAction<Vector3> cameraEulerAngles;
        float yaw = 0.0f;
        float pitch = 0.0f;
        float sensitivity;// マウスのセンシ
        bool isCameraMovementPaused = false;//  敵に見つかったかの監視
        private UniversalAdditionalCameraData cameraData;
        // コルーチンの定義
        private IEnumerator SetUPRDataAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);
            cameraData.SetRenderer(default); // Rendererのindexを指定する
        }
        void Start()
        {
            cameraData = GetComponent<Camera>().GetComponent<UniversalAdditionalCameraData>();
        }
        
        void Update()
        {
            if(isCameraMovementPaused != true)
            {
                if(Cursor.visible == false)//   カーソル表示時にカメラ操作の停止
                {
                    // マウスの入力を取得する
                    float mouseX = Input.GetAxis("Mouse X") * sensitivity;
                    float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

                    // Y軸の回転量を計算する
                    yaw += mouseX;
                    // X軸の回転量を計算する（上下方向の回転を制限する）
                    pitch -= mouseY;

                    pitch = Mathf.Clamp(pitch, -90f, 90f); // 上下方向の回転を制限する

                    // カメラの回転を適用する
                    transform.eulerAngles = new Vector3(pitch, yaw, 0f);

                    // カメラが向いている方向を取得しPlayerを回転させる
                    cameraEulerAngles?.Invoke(transform.eulerAngles);
                }
            }
            else
            {
                // キャンバスがプレイヤーの方向を向くように回転させる
                transform.LookAt(enemyTransform);
                // UIが正しい方向を向くようにY軸のみを回転させる
                Vector3 rotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(0, rotation.y, 0);
            }
        }
        public void SetURPData(int rendererIndex,float EffectiveTime)
        {
            cameraData.SetRenderer(rendererIndex); // Rendererのindexを指定する
            StartCoroutine(SetUPRDataAfterDelay(EffectiveTime));
        }
        //  設定したマウスセンシの同期  //
        public void SyncMouseSensitivity(float _mouseSensitivity)
        {
            sensitivity = _mouseSensitivity;
            
        }
        //  カーソル表示フラグの取得    //
        public void AimCameraAtEnemy(Transform _enemyTransform)
        {
            isCameraMovementPaused = true;
            enemyTransform = _enemyTransform;
        }
        public void ReleaseCameraLock()
        {
            isCameraMovementPaused = false;
            Debug.Log("固定解除");
        }
    }
}

