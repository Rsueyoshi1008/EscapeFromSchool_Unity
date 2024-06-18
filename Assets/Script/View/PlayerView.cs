using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
namespace MVRP.Player.Views
{
    public class PlayerView : MonoBehaviour
    {
        //  テキスト
        [SerializeField] private Text _itemText;
        [SerializeField] private Text _eventText;
        
        //  ボタン
        [SerializeField] private Button _itemCheckButton;
        //  画像
        [SerializeField] private Image _reticleSprite;
        [SerializeField] private Image _itemSprite;
        //  スライダー
        [SerializeField] private Slider staminaSlider;
        //  アイテム周りを写すテクスチャ
        [SerializeField] private RawImage cameraRawImage;
        //  オーディオ
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickSound;
        public Func<string, string> _itemSearchFunction;
        public void Initialize()
        {
            cameraRawImage.gameObject.SetActive(false);
        }
        void Start()
        {
            _itemText.gameObject.SetActive(false);
            _eventText.gameObject.SetActive(false);
            staminaSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged(staminaSlider.value); });
            _itemCheckButton.onClick.AddListener(OnButtonClick);
        }
        public void Synchronous(float _maxDashStamina)
        {
            staminaSlider.maxValue = _maxDashStamina;// 最大値の設定
            staminaSlider.value = _maxDashStamina;//    最大値に初期化
            
        }
        // コルーチンの定義
        private IEnumerator HideTextAfterDelay(float delay)
        {
            // 指定された秒数待機
            yield return new WaitForSeconds(delay);

            // テキストを非表示にする
            _itemText.gameObject.SetActive(false);
            _itemSprite.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        public void OnButtonClick()
        {
            string itemName = _itemSearchFunction?.Invoke(_itemSprite.sprite.name);//   親Imageの画像名を取得
            if(itemName != "")
            {
                _itemText.text = itemName;
                _itemText.gameObject.SetActive(true);
                
            }
            PlayClickSound();
        }
        public void SetItemView(string t,Sprite image)
        {
            if(t == "null")
            {
                _itemText.text = t;
                _itemText.gameObject.SetActive(false);
                return;
            }
            _itemText.gameObject.SetActive(true);
            _itemSprite.gameObject.SetActive(true);
            //Textではなく取得したアイテムの画像を表示するように変更する
            _itemText.text = t;
            _itemSprite.sprite = image;
            // 4秒後にテキストを非表示にするコルーチンを開始
            StartCoroutine(HideTextAfterDelay(4f));
            
        }
        public void SetCameraRawImage(bool isView)
        {
            cameraRawImage.gameObject.SetActive(isView);
        }
        public void ToggleEventTextVisibility(bool isVisible)
        {
            _eventText.gameObject.SetActive(isVisible);
        }
        public void OnSliderValueChanged(float _stamina)
        {
            staminaSlider.value = _stamina;
        }
        //  非表示  //
        public void HideUI()
        {
            
            _itemText.gameObject.SetActive(false);
            _itemSprite.gameObject.SetActive(false);
            //  レティクルだけ逆の処理をする
            _reticleSprite.gameObject.SetActive(true);
            
        }
        //  表示    //
        public void IndicationUI()
        {
            if(_itemSprite.sprite != null)//    画像があるときのみアイテムを表示
            {
                _itemSprite.gameObject.SetActive(true);
            }
            
            //  レティクルだけ逆の処理をする
            _reticleSprite.gameObject.SetActive(false);
        }
        //  カーソルの表示に合わせてUI表示  //
        public void GetCursorVisibility(bool _isCursor)
        {
            if (_isCursor == true)
            {
                IndicationUI();
            }
            else HideUI();
        }
        public void PlayClickSound()
        {
            if (audioSource != null && clickSound != null)
            {
                audioSource.PlayOneShot(clickSound);
            }
        }
    }
}

