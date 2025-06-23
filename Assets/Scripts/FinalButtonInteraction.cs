using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalButtonInteraction : MonoBehaviour
{
    public static FinalButtonInteraction Instance;
    PlayerMovement player;
    private bool playerInRange;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        player = PlayerMovement.Instance;
    }
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.R))
        {
            UIManager.Instance.confirmResetPanel.SetActive(true);
            Time.timeScale = 0;
            player.enabled = false;
        }
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


    public void OnPressExit()
    {
        Application.Quit();
        // If running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnPressCancel()
    {
        UIManager.Instance.cancelEndingPanel.SetActive(false);
        Time.timeScale = 1;
        player.enabled = true;
    }

    public void OnPressRestart()
    {
        Time.timeScale = 1;
        player.enabled = true;
        SaveData.Instance.DeleteData();
        player.transform.position = new Vector2(0, 0);
        UIManager.Instance.resetEndingPanel.SetActive(false);
        SceneManager.LoadScene("SampleScene");
    }
}
