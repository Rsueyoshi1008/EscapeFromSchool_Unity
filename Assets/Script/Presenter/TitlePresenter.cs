using UnityEngine;
using MVRP.Game.managers;

namespace MVRP.Player.Presenter
{
    public sealed class TitlePresenter : MonoBehaviour
    {
        //  GameManager
        [SerializeField] private GameManager _gameManager;
        //  view
        [SerializeField] private TitleView _titleView;

        private void Start()
        {
            _titleView.changeScene = _gameManager.ChangeScene;
        }
    }
}

