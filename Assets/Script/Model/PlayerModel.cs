using UnityEngine;
using System;
using UniRx;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MVRP.Doors.Models;
namespace MVRP.Player.Models
{

    public sealed class PlayerModel : MonoBehaviour
    {
        // Unityのコンポーネント
        private Rigidbody rb;
        [SerializeField] private PlayerDataBase playerDataBase;//   Playerのステータスを格納
        [SerializeField] private Animator anim;
        [SerializeField] private Transform groundCheck;//   地面との接触判定
        //  オーディオ  //
        [SerializeField] private AudioSource getItemAudioSource;

        //  UniRx
        public IReadOnlyReactiveProperty<string> GetItemName => _getItemName;
        private readonly StringReactiveProperty _getItemName = new StringReactiveProperty("");
        public IReadOnlyReactiveProperty<float> GetPlayerStamina => _getPlayerStamina;
        private readonly FloatReactiveProperty _getPlayerStamina = new FloatReactiveProperty();
        public ReactiveProperty<bool> IsCursor => _isCursor;// カーソル表示の監視
        private readonly BoolReactiveProperty _isCursor = new BoolReactiveProperty(false);
        /*
        public IReadOnlyReactiveProperty<string> ChangeScene => _changeScene;
        private readonly StringReactiveProperty _changeScene = new StringReactiveProperty("");
        public IReadOnlyReactiveProperty<string> ReItemSpawnEvent => _reItemSpawnEvent;//  リスポーンイベント
        private readonly StringReactiveProperty _reItemSpawnEvent = new StringReactiveProperty("");
        public IReadOnlyReactiveProperty<float> SnycPlayerMaxDashStamina => _snycPlayerMaxDashStamina;//   Playerのスタミナの同期
        private readonly FloatReactiveProperty _snycPlayerMaxDashStamina = new FloatReactiveProperty();
        public ReactiveProperty<bool> EventKey => _eventKey;// カーソル表示の監視
        private readonly BoolReactiveProperty _eventKey = new BoolReactiveProperty(false);
        */
        
        //  アニメーション  //
        bool _isRunAnimation;//  走るアニメーション
        float _moveSpeedAnimation;// 走りから歩きの移行

        //  ステータス  //
        float moveSpeed;//   Playerの今の移動速度
        float dashStamina;//   ダッシュのスタミナ
        float maxDashStamina;//    スタミナの最大値
        float jumpPower;// ジャンプ力
        float dashMoveSpeed;//  走った時の移動速度
        float walkMoveSpeed;//   歩きの移動速度
        float slowdownMoveSpeed;//  スタミナが切れて遅くなった時の移動速度
        bool isDownDash = false;    //ダッシュの状態
        //  当たり判定  //
        bool isCameraMovementPaused = false;
        bool isWallHit = false;
        bool isInDoor = false;
        float groundDistance = 0.1f;//  キャラクターが地面と接触しているかどうかを確認する
        bool isRestrictedMovement;//    チュートリアルが終わるまで動くのを制限
        //  階段の処理  //
        [SerializeField] private float maxStepHeight;
        bool isStair = false;
        private Vector3 velocity;
        //  Ray //
        [SerializeField] private float movementRayDistance;// 移動用のRayを飛ばす距離
        [SerializeField] private Transform rayLaunchPosition;// RaySphereの発射位置
        [SerializeField] private Transform StairRayPosition;// 階段を検知するRayの発射位置
        [SerializeField] private float StairRayDistance;// 階段を検知するRayを飛ばす距離
        RaycastHit inputHit;//   壁との衝突処理用のRay構造体
        RaycastHit cameraHit;// カメラが向いてる方向に飛ばすRay構造体
        //  イベント  //
        public UnityAction<string> changeScene;
        public UnityAction<bool> eventKey;
        public UnityAction<string> reItemSpawnEvent;//  リスポーンイベント
        public UnityAction reCameraEvent;//  リスポーンイベント
        public UnityAction cameraRawImageEvent;// ViewのRawImageをfalseにする
        public UnityAction<float> snycPlayerMaxDashStamina;//   Playerのスタミナの同期
        //  現在アクティブなアウトラインを追跡するための変数
        Outline currentOutline = null;

        // リスポーン関連   //
        [SerializeField] private Transform reStartPosition;
        bool reStart = false;
        DoorController _doorController; //壁に衝突したときの処理遅延


        //  テスト用
        public UnityAction<string> testChangeScene;
        public void Initialize()
        {
            transform.position = reStartPosition.position;//    初期値に設定
            if (playerDataBase == null)//    参照されていないときゲーム終了
            {
                Debug.LogError("PlayerDataBase is not found!");
                // ゲームの実行を停止する
                    #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    Application.Quit();
                #endif
            }
            //  PlayerStatusの取得
            GetStatus();
            //ダッシュのスタミナを最大値に初期化
            dashStamina = maxDashStamina;
            isRestrictedMovement = true;
            reStart = false;
        }
        void Start()
        {
            rb = transform.GetComponent<Rigidbody>();
            transform.position = reStartPosition.position;//    初期値に設定
        }

        
        void Update()
        {
            
            // キャラクターのレイヤーマスクを作成
            int layerMask = ~LayerMask.GetMask("Player");
            int outLineLayer = LayerMask.NameToLayer("OutLine");

            // レイキャストを使って地面との接触を検出
            bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, layerMask);
            if (cameraHit.collider != null)//    cameraHit.colliderがnullじゃないことの確認
            {
                if (cameraHit.collider.gameObject.layer == outLineLayer)//   outLineLayerを検知したらoutLineを表示
                {
                    Outline outline = cameraHit.collider.gameObject.GetComponentInChildren<Outline>();
                    if (outline != null)
                    {
                        // 新しいオブジェクトにアクティブなアウトラインを設定
                        if (currentOutline != outline)
                        {
                            if (currentOutline != null)
                            {
                                // 以前のアウトラインを無効にする
                                currentOutline.enabled = false;
                            }
                            currentOutline = outline;
                            
                        }
                        //  OutLineのアクティブ化
                        outline.enabled = true;
                        //_eventKey.Value = true;
                        eventKey?.Invoke(true);
                    }
                }
                else
                {
                    // Rayが何もヒットしなかった場合、現在のアウトラインを無効にする
                    if (currentOutline != null)
                    {
                        currentOutline.enabled = false;
                        currentOutline = null;
                        //_eventKey.Value =  false;
                        eventKey?.Invoke(false);
                    }
                }
                if (cameraHit.collider.gameObject.tag == "Item")
                {
                    //アイテム取得(String)
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        string itemName = cameraHit.collider.gameObject.name.Replace("(Clone)", "");
                        // (clone)を削除
                        itemName = itemName.Replace("(clone)", "").Trim();
                        _getItemName.Value = itemName;//    取得したアイテム名を格納
                        //_eventKey.Value =  false;
                        eventKey?.Invoke(false);
                        getItemAudioSource.Play();
                        Destroy(cameraHit.collider.gameObject);//   獲得したアイテムを削除する
                    }
                }

            }

            //  Test用のイベント
            
            if(Input.GetKeyDown(KeyCode.F))
            {
                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                testChangeScene?.Invoke("Clear");
                
                //_getItemName.Value = "PerfectItem";
            }
            

            if (isDownDash == false)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (isWallHit == false && isStair == false)// すり抜けするオブジェクトに触れていないときダッシュ可能
                    {
                        StartDash();
                        _isRunAnimation = true;
                    }
                    else
                    {
                        StartWalk();
                        _isRunAnimation = false;
                    }

                }
                else
                {
                    StartWalk();
                    _isRunAnimation = false;
                }
            }
            else CoolDownDash();
            _getPlayerStamina.Value = dashStamina;//    スタミナをViewのスライダーに反映
            if (isInDoor == true)// 扉に近づいて尚且つEキーを入力したとき
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    //  扉を開ける処理
                    _doorController.idDoorOpen = !_doorController.idDoorOpen;
                }
            }
            if (isGrounded == true)
            {
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        Jump();
                    }
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }

            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //  カーソルの表示、非表示を行う    //
                ChangeCursorVisible();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && Input.GetKey(KeyCode.G))
            {
                //  ビルド時にゲームを終了させる
                Application.Quit();
            }
            
        }
        
        void FixedUpdate()
        {
            if(reStart == false)//  Initializeで位置を初期化してもどこかでtransformが変わってるからここでも初期化
            {
                transform.position = reStartPosition.position;
                reStart = true;
            }
            float inputHorizontal = Input.GetAxis("Horizontal");
            float inputVertical = Input.GetAxis("Vertical");
            // 入力方向をベクトルに変換
            Vector3 inputDirection = new Vector3(inputHorizontal, 0f, inputVertical).normalized;

            // カメラの向きに合わせて入力ベクトルを変換
            inputDirection = Camera.main.transform.TransformDirection(inputDirection);

            // 壁との衝突処理用のRayを生成
            Ray ray = new Ray(rayLaunchPosition.position, inputDirection);
            //  階段検知のRayを作成
            Ray stairRay = new Ray(StairRayPosition.position, inputDirection);
            Debug.DrawRay(stairRay.origin, stairRay.direction * StairRayDistance, Color.red, 1);

            if (isRestrictedMovement != true && isCameraMovementPaused != true)
            {
                if (Physics.Raycast(ray, out inputHit, movementRayDistance))
                {
                    if (inputHit.collider.gameObject.tag == "Wall")//  壁に当たった時
                    {
                        if (inputVertical > 0)// 前に入力しているとき
                        {
                            isWallHit = true;
                            // 法線方向に対してプレイヤーをスライドさせる方向を計算
                            Vector3 slideDirection = Vector3.ProjectOnPlane(transform.forward, inputHit.normal).normalized;
                            Vector3 slideMovement = slideDirection * moveSpeed * Time.fixedDeltaTime;
                            rb.MovePosition(transform.position + slideMovement);
                            
                        }
                        else if (inputVertical < 0)//    後ろに入力しているとき
                        {
                            isWallHit = true;
                            // 法線方向に対してプレイヤーをスライドさせる方向を計算
                            Vector3 slideDirection = Vector3.ProjectOnPlane(transform.forward, inputHit.normal).normalized;
                            Vector3 slideMovement = -slideDirection * moveSpeed * Time.fixedDeltaTime;
                            rb.MovePosition(transform.position + slideMovement);
                        }
                        else if (inputHorizontal != 0) //    横入力しているときはスライドしない
                        {
                            return;
                        }
                    }
                    else Move(inputHorizontal, inputVertical);
                }
                else//  オブジェクトに当たっていない時
                {
                    isWallHit = false;
                    Move(inputHorizontal, inputVertical);
                }
                if(inputHorizontal == 0f && inputVertical == 0f)//  移動キーを離したときにすぐに停止する
                {
                    Move(0f,0f);
                }
                if(Physics.Raycast(stairRay, out inputHit, StairRayDistance))// 階段を検知するためのRay
                {
                    if(inputHit.collider.gameObject.tag == "Stair" && isStair == true)
                    {
                        StairMove();
                    }
                }
            }
        }
        //  Playerのステータス取得
        public void GetStatus()
        {
            foreach (PlayerStatus status in playerDataBase.playerStatus)
            {
                if (status.type == "JumpPower")
                {
                    if (float.TryParse(status.value, out jumpPower))//   stringからfloatに変換成功したかを確かめるメソッド
                    {

                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
                if (status.type == "MaxDashStamina")
                {
                    if (float.TryParse(status.value, out maxDashStamina))//   stringからfloatに変換成功したかを確かめるメソッド
                    {

                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
                if (status.type == "DashMoveSpeed")
                {
                    if (float.TryParse(status.value, out dashMoveSpeed))//   stringからfloatに変換成功したかを確かめるメソッド
                    {

                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
                if (status.type == "WalkMoveSpeed")
                {
                    if (float.TryParse(status.value, out walkMoveSpeed))//   stringからfloatに変換成功したかを確かめるメソッド
                    {

                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
                if (status.type == "SlowdownMoveSpeed")
                {
                    if (float.TryParse(status.value, out slowdownMoveSpeed))//   stringからfloatに変換成功したかを確かめるメソッド
                    {

                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
            }
        }


        public void SetItemName()// アイテム名の初期化
        {
            _getItemName.Value = "";
        }
        //  移動処理
        public void Move(float _inputHorizontal, float _inputVertical)
        {

            // 入力の正規化
            Vector3 inputDirection = new Vector3(_inputHorizontal, 0.0f, _inputVertical).normalized;

            // プレイヤーの向きに合わせて入力の変換
            inputDirection = transform.TransformDirection(inputDirection);

            // 速度のスケーリング
            velocity = inputDirection * moveSpeed;

            rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
        }
        // 階段を登る処理
        private void StairMove()
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, calculateJumpForce(maxStepHeight), rb.linearVelocity.z);
        }
        //  階段を上るのに必要な計算
        private float calculateJumpForce(float height)
        {
            return Mathf.Sqrt(2 * height * Physics.gravity.magnitude) * 1.2f;
        }
        public void Jump()
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
        public void Rotation(Vector3 cameraTransform)
        {
            // キャラクタの回転を設定
            transform.eulerAngles = new Vector3(0, cameraTransform.y, 0);
        }
        //  Playerのダッシュ処理
        public void StartDash()
        {
            if (dashStamina >= 0)
            {
                moveSpeed = dashMoveSpeed;//  ダッシュ処理
                dashStamina -= Time.deltaTime;//    スタミナ消費

            }
            else
            {
                StartWalk();
                isDownDash = true;//  スタミナ切れまで走ったら
            }
        }
        //  Playerの歩き処理
        public void StartWalk()
        {
            moveSpeed = walkMoveSpeed;//   ダッシュ終了処理
            if (dashStamina <= maxDashStamina)
            {
                dashStamina += Time.deltaTime;//    スタミナ回復
            }
        }
        //  スタミナ切れの処理
        public void CoolDownDash()
        {
            moveSpeed = slowdownMoveSpeed;//   減速
            if (dashStamina <= maxDashStamina)
            {
                dashStamina += Time.deltaTime;//    スタミナ最大回復まで待機
            }
            else isDownDash = false;//  回復したらダッシュ可能
        }
        public void GetMaxStamina()
        {
            //_snycPlayerMaxDashStamina.Value = maxDashStamina;
            snycPlayerMaxDashStamina?.Invoke(maxDashStamina);
        }

        public void ChangeCursorVisible()
        {
            if (Cursor.visible == false)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _isCursor.Value = Cursor.visible;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _isCursor.Value = Cursor.visible;
            }
        }
        //  カメラの方向を取得
        public void GetCameraEulerAngles(Vector3 cameraTransform)
        {
            Rotation(cameraTransform);
        }
        //  カメラから発射されているRayで取得した情報
        public void GetRayCastObjectName(RaycastHit _hit)
        {
            cameraHit = _hit;
        }
        public void PlayerMoveAtEnemy()
        {
            isCameraMovementPaused = true;
        }
        public void LockMovement()
        {
            isRestrictedMovement = false;
        }
        /// <summary>
        /// 衝突イベント
        /// </summary>
        private void OnCollisionEnter(Collision other)
        {
            //  敵から発見状態で敵に触れて時にリスポーンをする
            if (other.gameObject.tag == "Student" && isCameraMovementPaused == true)
            {
                //_reItemSpawnEvent.Value = "EscapeKey";
                reItemSpawnEvent?.Invoke("EscapeKey");
                reCameraEvent?.Invoke();
                transform.position = reStartPosition.position;
                isCameraMovementPaused = false;
                isStair = false;//  階段のダッシュ不可フラグを初期化
            }
        }
        private void OnTriggerEnter(Collider collision)
        {
            // ドアの開閉を可能にする
            if (collision.gameObject.tag == "Door")
            {
                isInDoor = true;
                _doorController = collision.gameObject.GetComponent<DoorController>();
                _doorController.GetIsOutLine(true);
                // Viewに推すボタンの表示をする
                //_eventKey.Value = true;
                eventKey?.Invoke(true);

            }
            // SceneChange
            if (collision.gameObject.tag == "Clear")
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                //_changeScene.Value = "Clear";
                changeScene?.Invoke("Clear");
            }
            //  階段エリアにいるときだけ走るのを禁止にする
            if(collision.gameObject.tag == "InStair")
            {
                isStair = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            // ドアの開閉を不能にする
            if (other.gameObject.tag == "Door")
            {
                isInDoor = false;
                _doorController.GetIsOutLine(false);
                //_eventKey.Value = false;
                eventKey?.Invoke(false);
            }
            //  走れるようにする
            if(other.gameObject.tag == "InStair")
            {
                isStair = false;
            }
        }
        
    }
}

