using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    [SerializeField] private GameObject[] targetsToToggle;
    private bool playerInRange = false;
    private bool isOn = true;
    private Animator anim;
    void Start()
    {
        anim = anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.R))
        {
            ToggleTargets();
        }
    }

    private void ToggleTargets()
    {
        isOn = !isOn;

        foreach (GameObject target in targetsToToggle)
        {
            target.SetActive(isOn);
        }

        anim.SetBool("IsOn", isOn);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
