using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseMaxHealth : MonoBehaviour
{
    [SerializeField] GameObject particles;
    [SerializeField] GameObject canvasUI;
    [SerializeField] HeartShards heartShards;
    bool used = false;

    void Start()
    {
        // if (PlayerMovement.Instance.maxHealth >= PlayerMovement.Instance.maxTotalHealth)
        // {
        //     Destroy(gameObject);
        // }
    }

    private void OnTriggerEnter2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player") && !used)
        {
            used = true;
            StartCoroutine(ShowUI());
        }
    }

    IEnumerator ShowUI()
    {
        GameObject _particles = Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(_particles, 0.5f);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.5f);

        canvasUI.SetActive(true);
        heartShards.initialFillAmount = PlayerMovement.Instance.heartShards * 0.25f;
        PlayerMovement.Instance.heartShards++;
        heartShards.targetFillAmount = PlayerMovement.Instance.heartShards * 0.25f;

        StartCoroutine(heartShards.LerpFill());

        yield return new WaitForSeconds(2.5f);
        SaveData.Instance.SavePlayerData();
        canvasUI.SetActive(false);
        Destroy(gameObject);
    }
}
