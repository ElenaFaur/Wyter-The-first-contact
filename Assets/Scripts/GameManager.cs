using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public UnityEngine.Vector2 platformingRespawnPoint;
    public UnityEngine.Vector2 respawnPoint;
    [SerializeField] Bench bench;
    private UnityEngine.Vector2 lastBenchRespawnPoint;
    public GameObject shade;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        SaveData.Instance.Initialize();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (PlayerMovement.Instance != null)
        {
            if (PlayerMovement.Instance.halfMana)
            {
                SaveData.Instance.LoadShadeData();
                if (SaveData.Instance.sceneWithShade == SceneManager.GetActiveScene().name || SaveData.Instance.sceneWithShade == "")
                {
                    Instantiate(shade, SaveData.Instance.shadePos, SaveData.Instance.shadeRot);
                }
            }
        }

        SaveScene();
        DontDestroyOnLoad(gameObject);
        bench = FindObjectOfType<Bench>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveData.Instance.SavePlayerData();
        }
    }

    public void SaveScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SaveData.Instance.sceneNames.Add(currentSceneName);
    }
    public void RespawnPlayer()
    {
        SaveData.Instance.LoadBench();
        if (SaveData.Instance.benchSceneName != null)
        {
            SceneManager.LoadScene(SaveData.Instance.benchSceneName);
        }
        if (SaveData.Instance.benchPos != null)
        {
            respawnPoint = SaveData.Instance.benchPos;
        }
        else
        {
            respawnPoint = platformingRespawnPoint;
        }

        PlayerMovement.Instance.transform.position = respawnPoint;
        PlayerMovement.Instance.Respawned();
        StartCoroutine(UIManager.Instance.DeactivateDeathScreen());
    }
    
}
