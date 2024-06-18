using UnityEngine;
using MVRP.Item.Models;
using MVRP.Item.Managers;
using MVRP.Player.Views;
using MVRP.Doors.Models;
using MVRP.Cameras.Models;

namespace MVRP.Items.Presenter
{
    public class ItemPresenter : MonoBehaviour
    {
        //  ItemManager //
        [SerializeField] private ItemManager _itemManager;
        //  PlayerView  //
        [SerializeField] private PlayerView _playerView;
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
            _itemSpawn._getItemObject = _itemManager.GetItemObject;
            //  敵が透けるアイテム効果
            _itemManager.onRevealEvent = _cameraControl.SetURPData;
            //  脱出鍵の周りが見える効果
            _itemManager.viewEscapeKey = _playerView.SetCameraRawImage;
            _itemManager.spawnItem = _itemSpawn.SpawnItemIfNameExists;
        }
    }
}