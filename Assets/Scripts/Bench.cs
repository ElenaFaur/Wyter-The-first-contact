using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bench : MonoBehaviour
{
    public bool inRange;
    public bool interacted;
    [SerializeField] float healSpeed = 1f;
    float healTimer;
    // [SerializeField] private GameObject saveText;
    [SerializeField] private float textDuration = 1.5f;

    private void Update()
    {
        if (inRange)
        {
            HealPlayer();
            if (Input.GetKeyDown(KeyCode.R))
            {
                interacted = true;
                SaveData.Instance.benchSceneName = SceneManager.GetActiveScene().name;
                SaveData.Instance.benchPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
                SaveData.Instance.SaveBench();
                SaveData.Instance.SavePlayerData();
                Debug.Log("Benched");

                StartCoroutine(ShowSavedText());
            }
        }
    }

    private IEnumerator ShowSavedText()
    {
        if (UIManager.Instance.saveText != null)
        {
            UIManager.Instance.saveText.SetActive(true);
            yield return new WaitForSeconds(textDuration);
            UIManager.Instance.saveText.SetActive(false);
        }
    }

    private void HealPlayer()
    {
        if (PlayerMovement.Instance.Health < PlayerMovement.Instance.maxHealth)
        {
            healTimer += Time.deltaTime;
            if (healTimer >= healSpeed)
            {
                PlayerMovement.Instance.Health++;
                healTimer = 0;
            }

            PlayerMovement.Instance.pState.healing = false;
            PlayerMovement.Instance.anim.SetBool("healing", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player"))
        {
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player"))
        {
            inRange = false;
            interacted = false;
        }
    }
}
