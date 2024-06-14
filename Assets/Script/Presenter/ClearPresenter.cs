using UnityEngine;

using MVRP.Game.managers;

namespace MVRP.Player.Presenter
{
    public sealed class ClearPresenter : MonoBehaviour
    {
        //  GameManager
        [SerializeField] private GameManager _gameManager;
        //  view
        [SerializeField] private ClearView _clearView;

        private void Start()
        {
            _clearView.changeScene = _gameManager.ChangeScene;
        }
    }
}

