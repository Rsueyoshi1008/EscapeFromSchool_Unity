using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
namespace MVRP.TutorialsPopup.Views
{
    public class TutorialsPopupView : MonoBehaviour
    {
        //  テキスト
        [SerializeField] private Text tutorialText;
        //  テキストのバックグラウンド
        [SerializeField] private Image tutorialTextBacGround;
        //  画像
        [SerializeField] private Image staminaArray;
        [SerializeField] private Image miniMapArray;
        //  ボタン
        [SerializeField] private Button _tutorialButton;
        //  InGameViewオブジェクト
        [SerializeField] private RawImage miniMap;
        [SerializeField] private Slider stamina;
        //  イベント
        public UnityAction staminaSnyc;
        public UnityAction unLockPlayerMovementEvent;// Playerの移動制限を解除する
        //  表示する文章
        [SerializeField] private List<string> tutorialSentence;
        //  オーディオ
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickSound;
        
        bool isTutorialSentenceEnd = false;//   チュートリアル完了監視
        int buttonClickCount = 0;// ボタンを押した数
        public void Initialize()
        {
            isTutorialSentenceEnd = false;
            buttonClickCount = 0;
            IndicationUI();
            SetText(tutorialSentence[buttonClickCount]);
            stamina.gameObject.SetActive(false);
            miniMap.gameObject.SetActive(false);
            staminaArray.gameObject.SetActive(false);
            miniMapArray.gameObject.SetActive(false);
        }
        void Start()
        {
            SetText(tutorialSentence[buttonClickCount]);//  テスト用に
            _tutorialButton.onClick.AddListener(OnButtonClick);
        }
        
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(isTutorialSentenceEnd == true)
                {
                    HideUI();
                    InGameViewIndication();
                }
                
            }
        }
        public void OnButtonClick()
        {
            buttonClickCount++;
            PlayClickSound();
            if(!isTutorialSentenceEnd)
            {
                if(tutorialSentence[buttonClickCount] == "Shiftキーを押している間は走れるけどスタミナがあるから左上を見ながらスタミナ切れに注意しよう")//    スタミナゲージを分かりやすくするImageの表示処理
                {
                    stamina.gameObject.SetActive(true);
                    staminaArray.gameObject.SetActive(true);
                }
                else if(tutorialSentence[buttonClickCount] == "画面に右上の方にミニマップが表示されるよ")
                {
                    miniMap.gameObject.SetActive(true);
                    miniMapArray.gameObject.SetActive(true);
                }
                else if(tutorialSentence[buttonClickCount] == "理解できたかな？Escapeキーを押して始めよう！")
                {
                    SetText(tutorialSentence[buttonClickCount]);
                    isTutorialSentenceEnd = true;
                }
                else
                {
                    staminaArray.gameObject.SetActive(false);
                    miniMapArray.gameObject.SetActive(false);
                }
                SetText(tutorialSentence[buttonClickCount]);
            }
            else return;
        }
        private void InGameViewIndication()
        {
            unLockPlayerMovementEvent?.Invoke();
            staminaSnyc?.Invoke();
        }
        public void SetText(string t)
        {
            tutorialText.text = t;
        }
        public void PlayClickSound()
        {
            if (audioSource != null && clickSound != null)
            {
                audioSource.PlayOneShot(clickSound);
            }
        }
        //  非表示  //
        public void HideUI()
        {
            tutorialText.gameObject.SetActive(false);
            _tutorialButton.gameObject.SetActive(false);
            tutorialTextBacGround.gameObject.SetActive(false);
        }
        //  表示    //
        public void IndicationUI()
        {
            tutorialText.gameObject.SetActive(true);
            _tutorialButton.gameObject.SetActive(true);
            tutorialTextBacGround.gameObject.SetActive(true);
        }
    }
}

