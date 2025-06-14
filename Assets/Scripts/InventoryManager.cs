using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] Image heartShard;
    [SerializeField] GameObject upCast, sideCast, downCast;
    [SerializeField] GameObject dash, varJump, wallJump;

    private void OnEnable()
    {
        heartShard.fillAmount = PlayerMovement.Instance.heartShards * 0.25f;

        //spells
        upCast.SetActive(PlayerMovement.Instance.unlockedUpCast);
        sideCast.SetActive(PlayerMovement.Instance.unlockedSideCast);
        downCast.SetActive(PlayerMovement.Instance.unlockedDownCast);

        //abilities
        dash.SetActive(PlayerMovement.Instance.unlockedDash);
        varJump.SetActive(PlayerMovement.Instance.unlockedJump);
        wallJump.SetActive(PlayerMovement.Instance.unlockedWallJump);
    }
}
