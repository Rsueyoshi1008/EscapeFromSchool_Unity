using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ClearColliderController : MonoBehaviour
{
    private Collider triggerCollider;
    private IEnumerator HideColliderAfterDelay(float delay)
    {
        // 指定された秒数待機
        yield return new WaitForSeconds(delay);
        triggerCollider.enabled = true;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Initialize()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.enabled = false;
        StartCoroutine(HideColliderAfterDelay(1f));
    }
}
