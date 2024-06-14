using UnityEngine;
using MVRP.Player.Models;
using MVRP.Cameras.Models;
using MVRP.Player.Views;
using MVRP.Setting.Views;
using MVRP.Item.Managers;
using MVRP.RayCast.Models;
using MVRP.Item.Models;
using MVRP.TutorialsPopup.Views;
using MVRP.Game.managers;
using UniRx;
using MVRP.Doors.Models;
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
        
        private void Start()
        {
            _playerModel.snycPlayerMaxDashStamina = _playerView.Synchronous;
            _playerModel.testChangeScene = _gameManager.ChangeScene;
            _playerModel.GetPlayerStamina.Subscribe(x => {_playerView.OnSliderValueChanged((float)x);}).AddTo(this);
            // Playerのアイテム取得を監視
            _playerModel.GetItemName.Subscribe(x => {_itemManager.GetName((string)x);}).AddTo(this);
            
            _playerModel.reCameraEvent = _cameraControl.ReleaseCameraLock;
            _playerModel.reItemSpawnEvent = _itemSpawn.SpawnItemIfNameExists;
            _playerModel.itemRawImageEvent = _playerView.SetRawImage;
            
            //  マウスセンシ設定値の監視
            _settingView.Sensitivity.Subscribe(x => {_cameraControl.SyncMouseSensitivity((float)x);}).AddTo(this);

            //  カーソル表示の監視
            _playerModel.IsCursor.Subscribe(x => {_playerView.GetCursorVisibility((bool)x);}).AddTo(this);
            _playerModel.IsCursor.Subscribe(x => {_settingView.GetCursorVisibility((bool)x);}).AddTo(this);
            //  カメラの視点固定
            _cameraControl.cameraEulerAngles = _playerModel.GetCameraEulerAngles;
            _playerModel.eventKey = _playerView.ToggleEventTextVisibility;
            //  Rayで取得したオブジェクトの受け取り
            _rayCastManager.managerRayCast_Name += _playerModel.GetRayCastObjectName;
            //  シーン遷移
            _playerModel.changeScene = _gameManager.ChangeScene;
            
        }
    }
}

