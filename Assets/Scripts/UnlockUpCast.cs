using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockUpCast : MonoBehaviour
{
    [SerializeField] GameObject particles;
    [SerializeField] GameObject canvasUI;
    bool used = false;

    void Start()
    {
        if (PlayerMovement.Instance.unlockedUpCast)
        {
            Destroy(gameObject);
        }
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

        yield return new WaitForSeconds(4f);
        PlayerMovement.Instance.unlockedUpCast = true;
        SaveData.Instance.SavePlayerData();
        canvasUI.SetActive(false);
        Destroy(gameObject);
    }
}
