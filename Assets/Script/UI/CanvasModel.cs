using UnityEngine;
public class CanvasModel : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // キャンバスがプレイヤーの方向を向くように回転させる
        transform.LookAt(playerTransform);

        // UIが正しい方向を向くようにY軸のみを回転させる
        Vector3 rotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, rotation.y, 0);
    }
}
