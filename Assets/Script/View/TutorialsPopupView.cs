using System.Collections;
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
        //  ボタン
        [SerializeField] private Button _tutorialButton;
        //  InGameViewオブジェクト
        [SerializeField] private GameObject inGameObject;
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
            staminaArray.gameObject.SetActive(false);
            inGameObject.SetActive(false);
        }
        void Start()
        {
            SetText(tutorialSentence[buttonClickCount]);//  テスト用に
            _tutorialButton.onClick.AddListener(OnButtonClick);
        }
        
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
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
                    inGameObject.SetActive(true);
                    staminaArray.gameObject.SetActive(true);
                }
                else
                {
                    inGameObject.SetActive(false);
                    staminaArray.gameObject.SetActive(false);
                } 
                if(tutorialSentence[buttonClickCount] == "理解できたかな？Tabキーを押して始めよう！")//    チュートリアルの終わり
                {
                    SetText(tutorialSentence[buttonClickCount]);
                    isTutorialSentenceEnd = true;
                }
                SetText(tutorialSentence[buttonClickCount]);
            }
            else return;
        }
        private void InGameViewIndication()
        {
            inGameObject.SetActive(true);
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

