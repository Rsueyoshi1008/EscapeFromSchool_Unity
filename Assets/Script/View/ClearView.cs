using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ClearView : MonoBehaviour
{
    //  ボタン
    [SerializeField] private Button _changeSceneButton;
    //  イベント
    public UnityAction<string> changeScene;
    //  オーディオ
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    void Start()
    {
        // ボタンにイベントを登録
        _changeSceneButton.onClick.AddListener(ChangeScene);
    }
    public void ChangeScene()
    {
        PlayClickSound();
        changeScene?.Invoke("Title");
    }
    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
