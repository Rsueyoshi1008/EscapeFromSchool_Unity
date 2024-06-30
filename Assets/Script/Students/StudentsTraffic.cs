using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MVRP.Students.Models
{
    public class StudentsTraffic : MonoBehaviour
    {
        //  ナビメッシュ
        [SerializeField] private UnityEngine.AI.NavMeshAgent nav;
        //  目標視点を管理してるオブジェクト
        [SerializeField] private GameObject destinationFolder;
        //   生徒のステータスを格納
        [SerializeField] private StudentsDataBase studentsDataBase;
        //  アニメーション
        [SerializeField] private Animator anim;
        //  オーディオ
        [SerializeField] private AudioSource audioSource;

        //足音はインスペクターで指定
        [SerializeField] private AudioClip footsteps;

        //  最高速度と最低速度
        private Vector2 minMaxSpeed = new Vector2();
        //  高低差によってオーディオの周波数調整    //
        [SerializeField] private Transform player;
        [SerializeField] private float maxVolumeDistance; // 音量が最大になる距離
        [SerializeField] private float heightFactor; // 高さによる音量減衰の係数
        [SerializeField] private float maxCutoffFrequency; // 高さの差がないときの最大カットオフ周波数
        [SerializeField] private float minCutoffFrequency; // 高さの差が最大のときの最小カットオフ周波数
        private AudioLowPassFilter lowPassFilter;
        //  イベント  //
        public UnityAction hideStudentCanvas;
        public UnityAction resetHitEvent;
        //  アニメーション
        float animationCount = 0;
        int playerState = 0; //0=entry, 1=stay
        bool refreshDestination = false;
        bool dice;
        float pauseTime = 1;
        float timeCount;
        //目標地点
        int targetPoint;
        public bool movementFlag = false;

        //Playerの位置
        Transform playerTransform;

        //  ミニマップに表示されるオブジェクト
        [SerializeField] private GameObject miniMapIcon;

        List<Transform> wayPoints = new List<Transform>();
        // コルーチンの定義
        private IEnumerator EnemyMoveAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);

            nav.destination = playerTransform.position;
            nav.speed = 3;
        }
        private IEnumerator ReSTartMoveAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);
            movementFlag = false;
            nav.speed = RandomSpeed();
            targetPoint = RandomPoint();
            nav.destination = wayPoints[targetPoint].position;

        }
        public void initialize()
        {
            nav.speed = RandomSpeed();
            targetPoint = RandomPoint();
            refreshDestination = true;
        }
        void Start()
        {

            timeCount = pauseTime;
            if (studentsDataBase == null)//    参照されていないときゲーム終了
            {
                Debug.LogError("StudentsDataBase is not found!");
                // ゲームの実行を停止する
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
            GetStatus();
            if (destinationFolder != null)
            {
                int count = destinationFolder.transform.childCount;//   目標地点の数を取得
                for (int i = 0; i < count; i++)
                {
                    wayPoints.Add(destinationFolder.transform.GetChild(i));
                }
            }
            else
            {
                //  目標地点管理オブジェクトがなかった場合
                print("DestinationFolder is empty, navmesh does not work. (Scene object " + transform.gameObject.name.ToString() + ").");
            }
            nav.speed = RandomSpeed();
            targetPoint = RandomPoint();
            lowPassFilter = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
            refreshDestination = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (wayPoints.Count == 0)// 目標地点がなかった場合何もしない
            {
                return;
            }
            else
            {
                if (movementFlag != true)
                {
                    //  ポイントとの距離の計算
                    float dist = Vector3.Distance(wayPoints[targetPoint].position, transform.position);
                    if (dist < 4f)// 目標地点に近づいたら
                    {
                        //arrived
                        if (!dice)
                        {
                            playerState = Random.Range(0, 2);
                            dice = true;
                        }
                        if (playerState == 1)
                        {
                            //  ディレイの役割
                            timeCount -= Time.deltaTime;
                            if (timeCount < 0)
                            {
                                timeCount = pauseTime;
                                dice = false;
                                playerState = 0;
                            }
                        }
                        else
                        {
                            //  新しい目標地点を決める
                            if (dice) dice = false;
                            targetPoint = RandomPoint();
                            nav.speed = RandomSpeed();
                            refreshDestination = true;
                        }
                    }
                }

                //Debug.Log(playerState);
                if (refreshDestination)
                {
                    //  新しい目標地点への距離計算
                    nav.destination = wayPoints[targetPoint].position;
                    refreshDestination = false;

                }
            }

            //  アニメーション
            anim.SetFloat("Walk", nav.velocity.magnitude);
            animationCount += Time.deltaTime;
            if(animationCount >= 0.5f)
            {
                if(nav.speed != 0)
                {
                    PlaybackFootsteps();
                    animationCount = 0.0f;
                }
            }
            //  Playerと同じ階層に近づくと音がだんだん大きくなる
            AdjustFootstepVolume();
            AdjustFootstepFilter();

            //  Playerと同じ階層にいたときにミニマップに敵を表示する
            ChangeObjectLayer();
        }
        public float RandomSpeed()
        {
            return Random.Range(minMaxSpeed.x, minMaxSpeed.y);
        }
        public int RandomPoint()
        {
            int rPoint = -1;
            if (wayPoints.Count > 0)
            {
                rPoint = Random.Range(0, wayPoints.Count);
            }
            return rPoint;
        }
        public void GetStatus()
        {
            foreach (StudentStatus status in studentsDataBase.studentStatus)
            {
                //  オブジェクト名と一致したらデータの取得
                if (status.studentName == gameObject.name)
                {
                    minMaxSpeed = new Vector2(status.maxSpeed, status.minSpeed);

                }

            }

        }
        void AdjustFootstepVolume()
        {
            if (player == null || audioSource == null) return;

            Vector3 playerPosition = player.position;
            Vector3 enemyPosition = transform.position;

            float distance = Vector3.Distance(new Vector3(playerPosition.x, 0, playerPosition.z), new Vector3(enemyPosition.x, 0, enemyPosition.z));
            float heightDifference = Mathf.Abs(playerPosition.y - enemyPosition.y);

            // 距離に基づいて基本音量を計算
            float volume = Mathf.Clamp01(1 - (distance / maxVolumeDistance));

            // 高さの差に基づいて音量を減衰
            volume *= Mathf.Clamp01(1 - (heightDifference * heightFactor));
            audioSource.volume = volume;
        }
        void ChangeObjectLayer()
        {
            if (player == null || audioSource == null) return;

            Vector3 playerPosition = player.position;
            Vector3 enemyPosition = transform.position;

            float heightDifference = Mathf.Abs(playerPosition.y - enemyPosition.y);
            if(heightDifference <= 4)
            {
                miniMapIcon.layer = LayerMask.NameToLayer("Player");
            }
            else miniMapIcon.layer = LayerMask.NameToLayer("Default");
        }
        void AdjustFootstepFilter()
        {
            if (player == null || lowPassFilter == null) return;

            Vector3 playerPosition = player.position;
            Vector3 enemyPosition = transform.position;
            float heightDifference = Mathf.Abs(playerPosition.y - enemyPosition.y);

            // 高さの差に基づいてカットオフ周波数を調整
            float cutoffFrequency = Mathf.Lerp(maxCutoffFrequency, minCutoffFrequency, heightDifference * heightFactor);
            lowPassFilter.cutoffFrequency = cutoffFrequency;
        }
        public void PlaybackFootsteps()
        {
            //足音を再生
            //足音のループ再生はインスペクターで指定
            audioSource.PlayOneShot(footsteps);
        }
        public void MovementRestrictions(Transform _playerTransform)
        {
            //  移動の一時停止
            movementFlag = true;
            nav.speed = 0;

            playerTransform = _playerTransform;
            // 秒後にテキストを非表示にするコルーチンを開始
            StartCoroutine(EnemyMoveAfterDelay(1f));
            //Debug.Log("Change" + "movementFlag" + movementFlag);
        }
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag == "Player")
            {
                hideStudentCanvas?.Invoke();
                resetHitEvent?.Invoke();
                StartCoroutine(ReSTartMoveAfterDelay(2f));//    二秒後に目的地の再設定
            }
        }
    }
}

