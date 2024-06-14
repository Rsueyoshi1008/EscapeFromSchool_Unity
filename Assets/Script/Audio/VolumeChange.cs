using UnityEngine;

public class VolumeChange : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private AudioSource audioSource;
    private AudioLowPassFilter lowPassFilter;
    [SerializeField] private float maxVolumeDistance; // 音量が最大になる距離
    [SerializeField] private float heightFactor; // 高さによる音量減衰の係数
    [SerializeField] private float maxCutoffFrequency; // 高さの差がないときの最大カットオフ周波数
    [SerializeField] private float minCutoffFrequency; // 高さの差が最大のときの最小カットオフ周波数
    void Start()
    {
        lowPassFilter = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        AdjustFootstepVolume();
        AdjustFootstepFilter();
    }
    void AdjustFootstepVolume()
    {
        if (target == null || audioSource == null) return;

        Vector3 targetPosition = target.position;
        Vector3 position = transform.position;

        float distance = Vector3.Distance(new Vector3(targetPosition.x, 0, targetPosition.z), new Vector3(position.x, 0, position.z));
        float heightDifference = Mathf.Abs(targetPosition.y - position.y);

        // 距離に基づいて基本音量を計算
        float volume = Mathf.Clamp01(1 - (distance / maxVolumeDistance));
        // 高さの差に基づいて音量を減衰
        volume *= Mathf.Clamp01(1 - (heightDifference * heightFactor));

        audioSource.volume = volume;
        //Debug.Log(heightDifference + "  /  " + Mathf.Clamp01(1 - (heightDifference * heightFactor)));
    }
    void AdjustFootstepFilter()
    {
        if (target == null || lowPassFilter == null) return;

        Vector3 targetPosition = target.position;
        Vector3 position = transform.position;

        float heightDifference = Mathf.Abs(targetPosition.y - position.y);
        // 高さの差に基づいてカットオフ周波数を調整
        float cutoffFrequency = Mathf.Lerp(maxCutoffFrequency, minCutoffFrequency, heightDifference * heightFactor);

        lowPassFilter.cutoffFrequency = cutoffFrequency;
    }
}
