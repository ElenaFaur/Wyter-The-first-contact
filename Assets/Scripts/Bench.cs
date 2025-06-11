using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bench : MonoBehaviour
{
    public bool interacted;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player") && Input.GetKeyDown(KeyCode.R))
        {
            interacted = true;
            GameManager.Instance.SetBenchRespawn((Vector2)transform.position);
            SaveData.Instance.benchSceneName = SceneManager.GetActiveScene().name;
            SaveData.Instance.benchPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
            SaveData.Instance.SaveBench();
            SaveData.Instance.SavePlayerData();
        }
    }

    private void OnTriggerExit2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player"))
        {
            interacted = false;
        }
    }
}
