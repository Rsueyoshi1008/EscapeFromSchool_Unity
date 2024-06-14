using UnityEngine;
using UniRx;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MVRP.Doors.Models;
using Unity.VisualScripting;
namespace MVRP.Player.Models
{
    
    public sealed class PlayerModel : MonoBehaviour
    {
        // Unityのコンポーネント
        private Rigidbody rb;
        [SerializeField] private PlayerDataBase playerDataBase;//   Playerのステータスを格納
        [SerializeField] private Animator anim;
        [SerializeField] private Transform groundCheck;//   地面との接触判定
        [SerializeField] private Transform rayLaunchPosition;// RaySphereの発射位置
        //  UniRx
        public IReadOnlyReactiveProperty<string> GetItemName => _getItemName;
        private readonly StringReactiveProperty _getItemName = new StringReactiveProperty("");

        public ReactiveProperty<bool> IsCursor => _isCursor;// カーソル表示の監視
        private readonly BoolReactiveProperty _isCursor = new BoolReactiveProperty(false);
        public IReadOnlyReactiveProperty<float> GetPlayerStamina => _getPlayerStamina;
        private readonly FloatReactiveProperty _getPlayerStamina = new FloatReactiveProperty();
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
        
        //  Ray //
        public float movementRayDistance;// 移動用のRayを飛ばす距離
        RaycastHit inputHit;//   壁との衝突処理用のRay構造体
        RaycastHit cameraHit;// カメラが向いてる方向に飛ばすRay構造体
        //  イベント  //
        public UnityAction<string> changeScene;
        public UnityAction<bool> eventKey;
        public UnityAction<string> reItemSpawnEvent;//  リスポーンイベント
        public UnityAction reCameraEvent;//  リスポーンイベント
        public UnityAction itemRawImageEvent;// ViewのRawImageをfalseにする
        public UnityAction<float> snycPlayerMaxDashStamina;//   Playerのスタミナの同期
        //  現在アクティブなアウトラインを追跡するための変数
        Outline currentOutline = null;

        // リスポーン関連   //
        [SerializeField] private Transform reStartPosition;
        DoorController _doorController; //壁に衝突したときの処理遅延
        //  テスト用
        public UnityAction<string> testChangeScene;
        [SerializeField] private AudioSource getItemAudioSource;
        public void Initialize()
        {
            transform.position = reStartPosition.position;//    初期値に設定
            if(playerDataBase == null)//    参照されていないときゲーム終了
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
        }
        void Start()
        {
            rb = transform.GetComponent<Rigidbody> ();
        }

        
        void Update()
        {
            // キャラクターのレイヤーマスクを作成
            int layerMask = ~LayerMask.GetMask("Player");
            int outLineLayer = LayerMask.NameToLayer("OutLine");

            // レイキャストを使って地面との接触を検出
            bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, layerMask);
            if(cameraHit.collider != null)//    cameraHit.colliderがnullじゃないことの確認
            {
                if(cameraHit.collider.gameObject.layer == outLineLayer)//   outLineLayerを検知したらoutLineを表示
                {
                    Outline outline = cameraHit.collider.gameObject.GetComponent<Outline>();
                    
                    if (outline != null)
                    {
                        // 新しいオブジェクトにアクティブなアウトラインを設定
                        if(currentOutline != outline)
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
                        
                    }
                }
                else
                {
                    // Rayが何もヒットしなかった場合、現在のアウトラインを無効にする
                    if (currentOutline != null)
                    {
                        currentOutline.enabled = false;
                        currentOutline = null;
                    }
                }
                if(cameraHit.collider.gameObject.tag == "Item")
                {
                    //アイテム取得(String)
                    if(Input.GetKeyDown(KeyCode.E))
                    {
                        string itemName = cameraHit.collider.gameObject.name.Replace("(Clone)", "");
                        // (clone)を削除
                        itemName = itemName.Replace("(clone)", "").Trim();
                        _getItemName.Value = itemName;
                        getItemAudioSource.Play();
                        
                        Destroy(cameraHit.collider.gameObject);//   獲得したアイテムを削除する
                    }
                }
                
            }
            //  Test用のイベント
            // if(Input.GetKeyDown(KeyCode.F))
            // {
            //     Cursor.lockState = CursorLockMode.None;
            //     Cursor.visible = true;
            //     testChangeScene?.Invoke("Clear");
            //     getItemAudioSource.Play();
            //     Debug.Log("PlaySound");
            // }
            
            
            if(isDownDash == false)
            {
                if(Input.GetKey(KeyCode.LeftShift))
                {  
                    if(isWallHit != true)// 壁に当たっていないときのみダッシュ可能
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
            
            
            
            
            if(isInDoor == true)// 扉に近づいて尚且つEキーを入力したとき
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    //  扉を開ける処理
                    _doorController.idDoorOpen = !_doorController.idDoorOpen;
                }
            }
            if(isGrounded == true)
            {
                if(EventSystem.current.currentSelectedGameObject == null)
                {
                    if(Input.GetKeyDown(KeyCode.Space))
                    {
                        Jump();
                    }
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                } 
                
            }
            
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                //  カーソルの表示、非表示を行う    //
                ChangeCursorVisible();
            }

            anim.SetBool("Run", _isRunAnimation);
            anim.SetFloat("Walk",Mathf.Abs(_moveSpeedAnimation));
        }
        
        void FixedUpdate()
        {
            float inputHorizontal = Input.GetAxis("Horizontal");
            float inputVertical = Input.GetAxis("Vertical");
            // 入力方向をベクトルに変換
            Vector3 inputDirection = new Vector3(inputHorizontal, 0f, inputVertical).normalized;            

            // カメラの向きに合わせて入力ベクトルを変換
            inputDirection = Camera.main.transform.TransformDirection(inputDirection);          

            // 壁との衝突処理用のRayを生成
            Ray ray = new Ray(rayLaunchPosition.position, inputDirection);

            //Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 2);
            
            if(isRestrictedMovement != true && isCameraMovementPaused != true)
            {
                if (Physics.Raycast(ray,out inputHit,movementRayDistance))
                {
                    
                    if(inputHit.collider.gameObject.tag == "Wall")//  壁に当たった時
                    {
                        if(inputVertical > 0)// 前に入力しているとき
                        {
                            isWallHit = true;
                            // 法線方向に対してプレイヤーをスライドさせる方向を計算
                            Vector3 slideDirection = Vector3.ProjectOnPlane(transform.forward, inputHit.normal).normalized; 
                            Vector3 slideMovement = slideDirection * moveSpeed * Time.fixedDeltaTime;
                            rb.MovePosition(transform.position + slideMovement);
                        }
                        else if(inputVertical < 0)//    後ろに入力しているとき
                        {
                            isWallHit = true;
                            // 法線方向に対してプレイヤーをスライドさせる方向を計算
                            Vector3 slideDirection = Vector3.ProjectOnPlane(transform.forward, inputHit.normal).normalized; 
                            Vector3 slideMovement = -slideDirection * moveSpeed * Time.fixedDeltaTime;
                            rb.MovePosition(transform.position + slideMovement);
                        }
                        else if(inputHorizontal != 0) //    横入力しているときはスライドしない
                        {
                            return;
                        }
                    }
                    else Move(inputHorizontal,inputVertical);
                }
                else//  オブジェクトに当たっていない時
                {
                    isWallHit = false;
                    Move(inputHorizontal,inputVertical);
                }
            }
            
            
            
        }
        
        public void GetStatus()
        {
            foreach (PlayerStatus status in playerDataBase.playerStatus)
            {
                if(status.type == "JumpPower")
                {
                    if(float.TryParse(status.value, out jumpPower))//   stringからfloatに変換成功したかを確かめるメソッド
                    {
                        
                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
                if(status.type == "MaxDashStamina")
                {
                    if(float.TryParse(status.value, out maxDashStamina))//   stringからfloatに変換成功したかを確かめるメソッド
                    {
                        
                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
                if(status.type == "DashMoveSpeed")
                {
                    if(float.TryParse(status.value, out dashMoveSpeed))//   stringからfloatに変換成功したかを確かめるメソッド
                    {
                        
                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
                if(status.type == "WalkMoveSpeed")
                {
                    if(float.TryParse(status.value, out walkMoveSpeed))//   stringからfloatに変換成功したかを確かめるメソッド
                    {
                        
                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
                if(status.type == "SlowdownMoveSpeed")
                {
                    if(float.TryParse(status.value, out slowdownMoveSpeed))//   stringからfloatに変換成功したかを確かめるメソッド
                    {
                        
                    }
                    else Debug.Log("PlayerStatusの変換に失敗");
                }
            }
        }
        

        public void Move(float _inputHorizontal,float _inputVertical)
        {
            
            // 入力の正規化
            Vector3 inputDirection = new Vector3(_inputHorizontal, 0.0f, _inputVertical).normalized;

            // プレイヤーの向きに合わせて入力の変換
            inputDirection = transform.TransformDirection(inputDirection);

            // 速度のスケーリング
            Vector3 velocity = inputDirection * moveSpeed;
            
            //rb.MovePosition(transform.position + transform.forward * moveSpeed * Time.deltaTime);//   テスト用の移動
            rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
                
            
        }
        public void Jump()
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
        public void Rotation(Vector3 cameraTransform)
        {
            // キャラクタの回転を設定
            transform.eulerAngles = new Vector3(0,cameraTransform.y,0);
        }
        public void StartDash()
        {
            if(dashStamina >= 0)
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
        public void StartWalk()
        {
            moveSpeed = walkMoveSpeed;//   ダッシュ終了処理
            if(dashStamina <= maxDashStamina)
            {
                dashStamina += Time.deltaTime;//    スタミナ回復
            }
            
        }
        //  スタミナ切れの処理
        public void CoolDownDash()
        {
            moveSpeed = slowdownMoveSpeed;//   減速
            if(dashStamina <= maxDashStamina)
            {
                dashStamina += Time.deltaTime;//    スタミナ最大回復まで待機
            }
            else isDownDash = false;//  回復したらダッシュ可能
        }
        public void GetMaxStamina()
        {
            snycPlayerMaxDashStamina?.Invoke(maxDashStamina);
            
        }

        public void ChangeCursorVisible()
        {
            if(Cursor.visible == false)
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
            if(other.gameObject.tag == "Student" && isCameraMovementPaused == true)
            {
                Debug.Log("瞬間移動");
                reItemSpawnEvent?.Invoke("EscapeKey");
                reCameraEvent?.Invoke();
                transform.position = reStartPosition.position;
                isCameraMovementPaused = false;
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
                eventKey?.Invoke(true);
                
            }
            // SceneChange
            if (collision.gameObject.tag == "Clear")
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                changeScene?.Invoke("Clear");
            }
        }
        private void OnTriggerExit(Collider other)
        {
            // ドアの開閉を可能にする
            if (other.gameObject.tag == "Door")
            {
                isInDoor = false;
                _doorController.GetIsOutLine(false);
                eventKey?.Invoke(false);
            }
        }
        
    }
}

