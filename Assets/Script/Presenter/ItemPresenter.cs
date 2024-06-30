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
            _itemManager.ReleaseEscape.Subscribe(x => {_doorController.ReleaseEscape();}).AddTo(this);
            
            // アイテムオブジェクトの取得と設定
            _itemSpawn.GetItemObjectRequest
                .Select(request => _itemManager.GetItemObject(request))
                .Where(response => response != null)
                .Subscribe(response => 
                {
                    _itemSpawn.SetPrefabObject(response); // ここでprefabObjectに値を代入
                });
            //  アイテムオブジェクトの取得と設定
            _itemSpawn.getIsSpawn = _itemManager.GetIsSpawn;
            //  乱数を記録した変数の取得と設定
            _itemSpawn.getPreviousRandom = _itemManager.GetPreviousRandom;
            //  生成の上限数と生成した数の取得と設定
            _itemSpawn.getMaxSpawnCountAndSpawnCount = _itemManager.MaxSpawnCountAndSpawnCount;
            //  敵が透けるアイテム効果
            _itemManager.onRevealEvent.Subscribe(x => {
                int param1 = x.Item1;
                float param2 = x.Item2;
                _cameraControl.SetURPData(param1,param2);
                }).AddTo(this);
            _itemManager.ViewEffectiveTime += HandleViewEffectiveTime;
            //  脱出鍵の周りが見える効果
            _itemManager.ViewEscapeKey.Subscribe(x => _playerView.SetCameraRawImage(x)).AddTo(this);
            _itemManager.spawnItem = _itemSpawn.SpawnItemIfNameExists;
            _itemManager.setItemNameFromPlayerModel = _playerModel.SetItemName;

            // ItemSpawnのUniRxイベントを購読
            _itemSpawn.SetPreviousRandom.Subscribe(x => _itemManager.SetPreviousRandom(x.Item1, x.Item2)).AddTo(this);
            _itemSpawn.SetIsSpawn.Subscribe(x => _itemManager.SetIsSpawn(x)).AddTo(this);
            _itemSpawn.SetSpawnCount.Subscribe(x => _itemManager.SetSpawnCount(x.Item1, x.Item2)).AddTo(this);
        }

        // コルーチンを開始するためのハンドラーメソッド
        private void HandleViewEffectiveTime(float time)
        {
            StartCoroutine(_playerView.GetEffectiveTime(time));
        }
    }
}