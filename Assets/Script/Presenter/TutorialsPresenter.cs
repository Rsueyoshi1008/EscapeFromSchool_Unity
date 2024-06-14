using UnityEngine;
using MVRP.Player.Models;
using MVRP.TutorialsPopup.Views;
namespace MVRP.Tutorials.Presenters
{
    public class TutorialsPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerModel _playerModel;
        [SerializeField] private TutorialsPopupView _tutorialsPopupView;
        void Start()
        {
            _tutorialsPopupView.staminaSnyc = _playerModel.GetMaxStamina;
            _tutorialsPopupView.unLockPlayerMovementEvent = _playerModel.LockMovement;
        }
    }
    
    
}