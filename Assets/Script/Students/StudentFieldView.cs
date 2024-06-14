using UnityEngine;
using UnityEngine.Events;
using MVRP.Students.Views;
using System.Collections;
namespace MVRP.Students.Models
{
    public class StudentFieldView : MonoBehaviour
    {
        //  Unityイベント
        public UnityAction onField;
        public UnityAction<Transform> enemyTransformAction;
        //  Rayを出す位置
        [SerializeField] private Transform lookAt;
        private bool hasProcessedEvents = false;
        private Vector3 playerPosition = new Vector3(0, 0, 0);
        private Collider triggerCollider;
        //  ColliderにPlayerが残り続けるバグの暫定処理
        private IEnumerator HideTextAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);

            triggerCollider.enabled = true;
        }
        void Start()
        {
            triggerCollider = GetComponent<Collider>();
            triggerCollider.enabled = false;
            StartCoroutine(HideTextAfterDelay(1f));
        }
        // 視界となるゲームオブジェクトに滞在している GameObject だけ検査する
        public void OnTriggerStay(Collider other)
        {
            foreach (Transform child in other.transform)
            {
                // 子オブジェクトにアクセス
                if (child.gameObject.name == "OnColliderCheck")
                {
                    playerPosition = child.gameObject.transform.position;// Playerの中心点
                    playerPosition.y = transform.position.y;//  ｙは考慮しない
                }

            }
            if (other.gameObject.tag == "Player")
            {
                //Playerと敵との距離
                Vector3 posDelta = playerPosition - transform.position;

                if (Physics.Raycast(lookAt.transform.position, posDelta, out RaycastHit hit)) //Rayを使用してtargetに当たっているか判別
                {
                    if (hit.collider.gameObject.tag == "Player" && !hasProcessedEvents)
                    {
                        // トリガーエリア外に一時的に移動
                        PublishEvents(hit.collider.gameObject.transform);
                        RotateToFacePlayer(hit.collider.gameObject.transform);
                        hasProcessedEvents = true;
                    }
                }
                Debug.DrawRay(lookAt.transform.position, posDelta, Color.blue, 2);

            }


        }
        public void PublishEvents(Transform _playerTransform)
        {
            //  Studentクラスへのアクセス・キャンバスの表示と移動を止める
            StudentView studentView = GetComponentInParent<StudentView>();
            StudentsTraffic studentsTraffic = GetComponentInParent<StudentsTraffic>();
            studentView.IndicationCanvas();
            //  PlayerのTransformを渡す
            studentsTraffic.MovementRestrictions(_playerTransform);
            onField?.Invoke();//    発見イベントの発行
            enemyTransformAction?.Invoke(transform);//  Playerのカメラを発見した生徒にカメラを固定する
        }
        public void RotateToFacePlayer(Transform _playerTransform)
        {
            // キャンバスがプレイヤーの方向を向くように回転させる
            transform.LookAt(_playerTransform);
            // UIが正しい方向を向くようにY軸のみを回転させる
            Vector3 rotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, rotation.y, 0);
        }
        public void ReSetHitEvent()
        {
            hasProcessedEvents = false;
        }
    }
}

