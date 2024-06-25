using UnityEngine;
using MVRP.Item.Models;
using MVRP.Item.Managers;
using MVRP.Player.Views;
using MVRP.Doors.Models;
using MVRP.Cameras.Models;
using MVRP.Player.Models;
using UniRx;

namespace MVRP.Items.Presenter
{
    public class ItemPresenter : MonoBehaviour
    {
        //  ItemManager //
        [SerializeField] private ItemManager _itemManager;
        //  PlayerView  //
        [SerializeField] private PlayerView _playerView;
        //  PlayerModel //
        [SerializeField] private PlayerModel _playerModel;
        //  DoorController  //
        [SerializeField] private DoorController _doorController;
        //  ItemSpawn   //
        [SerializeField] private ItemSpawn _itemSpawn;
        //  CameraControl    //
        [SerializeField] private CameraControl _cameraControl;

        private void Start()
        {
            _itemManager._viewItem = _playerView.SetItemView;
            _playerView._itemSearchFunction = _itemManager.ItemSearch;
            _itemManager.releaseEscape = _doorController.ReleaseEscape;
            _itemSpawn.getItemObject = _itemManager.GetItemObject;
            //  アイテムオブジェクトの取得と設定
            _itemSpawn.getIsSpawn = _itemManager.GetIsSpawn;
            _itemSpawn.setIsSpawn = _itemManager.SetIsSpawn;
            //  乱数を記録した変数の取得と設定
            _itemSpawn.getPreviousRandom = _itemManager.GetPreviousRandom;
            _itemSpawn.setPreviousRandom = _itemManager.SetPreviousRandom;
            //  生成の上限数と生成した数の取得と設定
            _itemSpawn.getMaxSpawnCountAndSpawnCount = _itemManager.MaxSpawnCountAndSpawnCount;
            _itemSpawn.setSpawnCount = _itemManager.SetSpawnCount;
            //  敵が透けるアイテム効果
            _itemManager.onRevealEvent.Subscribe(x => {
                int param1 = x.Item1;
                float param2 = x.Item2;
                _cameraControl.SetURPData(param1,param2);
                }).AddTo(this);
            _itemManager.ViewEffectiveTime += HandleViewEffectiveTime;
            //  脱出鍵の周りが見える効果
            _itemManager.viewEscapeKey = _playerView.SetCameraRawImage;
            _itemManager.spawnItem = _itemSpawn.SpawnItemIfNameExists;
            _itemManager.setItemNameFromPlayerModel = _playerModel.SetItemName;
        }
        // コルーチンを開始するためのハンドラーメソッド
        private void HandleViewEffectiveTime(float time)
        {
            StartCoroutine(_playerView.GetEffectiveTime(time));
        }
    }
}