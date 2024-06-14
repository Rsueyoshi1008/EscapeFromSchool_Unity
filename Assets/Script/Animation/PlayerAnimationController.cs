using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    public bool run;
    void Start()
    {
        if (run) run = false;
    }
    void Update()
    {
        if (Input.GetAxisRaw("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0)
        {
            run = false;
        }
        else
        {
            run = true;
        }
        anim.SetBool("Run", run);
    }
}
