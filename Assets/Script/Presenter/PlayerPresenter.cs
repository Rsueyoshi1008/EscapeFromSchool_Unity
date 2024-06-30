using UnityEngine;
using MVRP.Player.Models;
using MVRP.Cameras.Models;
using MVRP.Player.Views;
using MVRP.Setting.Views;
using MVRP.Item.Managers;
using MVRP.RayCast.Models;
using MVRP.Item.Models;
using MVRP.Game.managers;
using MVRP.Floor.managers;
using UniRx;
namespace MVRP.Player.Presenter
{
    public sealed class PlayerPresenter : MonoBehaviour
    {
        //  GameManager
        [SerializeField] private GameManager _gameManager;
        //  view
        [SerializeField] private PlayerView _playerView;
        //  model
        [SerializeField] private PlayerModel _playerModel;
        //  SettingView
        [SerializeField] private SettingView _settingView;
        //  CameraModel
        [SerializeField] private CameraControl _cameraControl;
        // ColliderController
        [SerializeField] private ItemManager _itemManager;
        //  RayCast
        [SerializeField] private RayCastManager _rayCastManager;
        //  ItemSpawn  //
        [SerializeField] private ItemSpawn _itemSpawn;
        //  FloorManager  //
        [SerializeField] private FloorManager _floorManager;
        
        private void Start()
        {
            _playerModel.SnycPlayerMaxDashStamina.Subscribe(x => {_playerView.Synchronous((float)x);}).AddTo(this);
            _playerModel.testChangeScene = _gameManager.ChangeScene;
            _playerModel.GetPlayerStamina.Subscribe(x => {_playerView.OnSliderValueChanged((float)x);}).AddTo(this);
            // Playerのアイテム取得を監視
            _playerModel.GetItemName.Subscribe(x => {_itemManager.GetName(x);}).AddTo(this);
            
            //  敵につかまった時にカメラを固定にする
            _playerModel.ReCameraEvent.Subscribe(x => {_cameraControl.ReleaseCameraLock();}).AddTo(this);

            //  敵につかまった時にアイテムの位置を更新
            _playerModel.ReItemSpawnEvent.Subscribe(x => {_itemSpawn.UpdateItemPosition(x);}).AddTo(this);
            
            //  今何階にいるかを発行
            _playerModel.SetFloorEvent.Subscribe(x => {_floorManager.GetFloorCount(x);}).AddTo(this);
            
            //  マウスセンシ設定値の監視
            _settingView.Sensitivity.Subscribe(x => {_cameraControl.SyncMouseSensitivity((float)x);}).AddTo(this);

            //  カーソル表示の監視
            _playerModel.IsCursor.Subscribe(x => {_playerView.GetCursorVisibility(x);}).AddTo(this);
            _playerModel.IsCursor.Subscribe(x => {_settingView.GetCursorVisibility(x);}).AddTo(this);

            //  カメラの視点固定
            _cameraControl.cameraEulerAngles = _playerModel.GetCameraEulerAngles;

            //  操作可能オブジェクトに触れたときにキーの表示をする
            _playerModel.EventKey.Subscribe(x => {_playerView.ToggleEventTextVisibility(x);}).AddTo(this);

            //  Rayで取得したオブジェクトの受け取り
            _rayCastManager.managerRayCast_Name += _playerModel.GetRayCastObjectName;

            //  シーン遷移
            _playerModel.ChangeScene.Subscribe(x => {_gameManager.ChangeScene(x);}).AddTo(this);
        }
    }
}

