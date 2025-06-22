using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    [ContextMenu("open")]
    public void Open()
    {
        anim.SetTrigger("open");
    }
}
