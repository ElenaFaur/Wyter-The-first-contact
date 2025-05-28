using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, animLength);
    }
}
