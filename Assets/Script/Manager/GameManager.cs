using UnityEngine;
namespace MVRP.Game.managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainGameObject;
        [SerializeField] private GameObject clearGameObject;
        [SerializeField] private GameObject titleGameObject;
        [SerializeField] private InitializeManager _initializeManager;
        void Start()
        {
            Application.targetFrameRate = 60; //FPSを60に設定
            mainGameObject.SetActive(false);
            clearGameObject.SetActive(false);
            titleGameObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void ChangeScene(string sceneName)
        {
            if (sceneName == "Clear")
            {
                clearGameObject.SetActive(true);
                mainGameObject.SetActive(false);
                titleGameObject.SetActive(false);
            }
            if (sceneName == "MainGame")
            {
                mainGameObject.SetActive(true);
                clearGameObject.SetActive(false);
                titleGameObject.SetActive(false);
                _initializeManager.initialize();
            }
            if (sceneName == "Title")
            {
                titleGameObject.SetActive(true);
                mainGameObject.SetActive(false);
                clearGameObject.SetActive(false);
            }

        }
    }
}

