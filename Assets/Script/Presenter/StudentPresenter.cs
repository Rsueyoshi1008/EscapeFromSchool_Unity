using UnityEngine;
using MVRP.Students.Models;
using MVRP.Students.Views;
using MVRP.Player.Models;
using MVRP.Cameras.Models;
using System.Collections.Generic;
namespace MVRP.Students.Presenter
{
    public class StudentPresenter : MonoBehaviour
    {
        [SerializeField] private List<StudentFieldView> _studentFieldView;
        [SerializeField] private List<StudentsTraffic> _studentsTraffic;
        [SerializeField] private List<StudentView> _studentView;
        [SerializeField] private PlayerModel _playerModel;
        [SerializeField] private CameraControl _cameraControl;
        void Start()
        {
            // リストのサイズが同じであることを確認
            if (_studentsTraffic.Count != _studentView.Count || _studentsTraffic.Count != _studentFieldView.Count)
            {
                Debug.LogError("Lists _studentFieldView and _studentsTraffic must be of the same length.");
                return;
            }
            foreach (var student in _studentFieldView)
            {
                //  カメラ制御とPlayerの移動の制御
                student.enemyTransformAction = _cameraControl.AimCameraAtEnemy;
                student.onField = _playerModel.PlayerMoveAtEnemy;
            }
            // 両方のリストの要素に対して対応する処理を行う
            for (int i = 0; i < _studentView.Count; i++)
            {
                _studentsTraffic[i].hideStudentCanvas = _studentView[i].HideCanvas;
                _studentsTraffic[i].resetHitEvent = _studentFieldView[i].ReSetHitEvent;
            }
        }
    }
}