using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
     [SerializeField] private float respawnDelay = 1f;

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player") && !PlayerMovement.Instance.pState.invincible)
        {
            StartCoroutine(RespawnPlayer());
        }
    }

    IEnumerator RespawnPlayer()
    {
        PlayerMovement.Instance.pState.invincible = true;
        PlayerMovement.Instance.rb.velocity = Vector2.zero;
        PlayerMovement.Instance.TakeDamage(1);

        yield return new WaitForSecondsRealtime(respawnDelay);

        PlayerMovement.Instance.transform.position = GameManager.Instance.platformingRespawnPoint;

        PlayerMovement.Instance.pState.invincible = false;
    }
}
