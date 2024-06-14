using UnityEngine;
using System.Collections.Generic;
using MVRP.Player.Models;
using MVRP.Item.Managers;
using MVRP.Students.Models;
using MVRP.Doors.Models;
using MVRP.TutorialsPopup.Views;
using MVRP.Player.Views;
using MVRP.Item.Models;

public class InitializeManager : MonoBehaviour
{
    //  Player
    [SerializeField] private PlayerView _playerView;
    [SerializeField] private PlayerModel _playerModel;
    [SerializeField] private TutorialsPopupView _tutorialsPopupView;
    [SerializeField] private ClearColliderController _clearColliderController;
    //  Item
    [SerializeField] private ItemManager _itemManager;
    [SerializeField] private ItemSpawn _itemSpawn;
    //  Door
    [SerializeField] private List<DoorController> _DoorController;
    //  Student
    [SerializeField] private List<StudentsTraffic> _studentsTraffic;
    public void initialize()
    {
        _playerView.Initialize();
        _playerModel.Initialize();
        _clearColliderController.Initialize();
        _itemManager.Initialize();
        _itemSpawn.Initialize();
        _tutorialsPopupView.Initialize();
        foreach (var doorController in _DoorController)
        {
            doorController.initialize();// 屋上の開放を初期化
        }
        
        foreach (var student in _studentsTraffic)
        {
            student.initialize();
        }
        
    }
}
