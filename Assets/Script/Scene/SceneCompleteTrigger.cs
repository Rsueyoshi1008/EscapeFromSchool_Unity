using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneCompleteTrigger : MonoBehaviour
{
    // クリアシーンの名前
    [SerializeField] private string clearSceneName = "ClearScene";

    // トリガーに他のコライダーが入ったときに呼び出されるメソッド
    private void OnTriggerEnter(Collider other)
    {
        // プレイヤータグを持つオブジェクトがトリガーに入った場合のみ処理を行う
        if (other.CompareTag("Player"))
        {
            // シーンをロードする
            //SceneManager.LoadScene(clearSceneName);
        }
    }
}
