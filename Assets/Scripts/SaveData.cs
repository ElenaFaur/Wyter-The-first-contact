using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct SaveData
{
    public static SaveData Instance;

    // map stuff
    public List<string> sceneNamesList; // Use List for JSON serialization
    public HashSet<string> sceneNames
    {
        get => sceneNamesList != null ? new HashSet<string>(sceneNamesList) : new HashSet<string>();
        set => sceneNamesList = new List<string>(value);
    }

    // bench stuff
    public string benchSceneName;
    public Vector2 benchPos;

    // player stuff
    public int playerHealth;
    public int playerHeartShards;
    public float playerMana;
    public bool playerHalfMana;
    public Vector2 playerPosition;
    public string lastScene;
    public bool playerUnlockedWallJump;
    public bool playerUnlockedDash;
    public bool playerUnlockedJump;
    // public bool playerUnlockedBlueDoor;
    public bool playerUnlockedSideCast, playerUnlockedUpCast, playerUnlockedDownCast;

    // enemies stuff
    public Vector2 shadePos;
    public string sceneWithShade;
    public Quaternion shadeRot;

    private static string benchPath => Application.persistentDataPath + "/save.bench.json";
    private static string playerPath => Application.persistentDataPath + "/save.player.json";
    private static string shadePath => Application.persistentDataPath + "/save.shade.json";

    public void Initialize()
    {
        if (!File.Exists(benchPath))
        {
            File.WriteAllText(benchPath, "");
        }
        if (!File.Exists(playerPath))
        {
            File.WriteAllText(playerPath, "");
        }
        if (!File.Exists(shadePath))
        {
            File.WriteAllText(shadePath, "");
        }
        if (sceneNamesList == null)
        {
            sceneNamesList = new List<string>();
        }
    }

    public void SaveBench()
    {
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(benchPath, json);
    }

    public void LoadBench()
    {
        if (File.Exists(benchPath))
        {
            string json = File.ReadAllText(benchPath);
            if (!string.IsNullOrEmpty(json))
            {
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                benchSceneName = loaded.benchSceneName;
                benchPos = loaded.benchPos;
            }
        }
    }

    public void SavePlayerData()
    {
        playerHealth = PlayerMovement.Instance.Health;
        playerHeartShards = PlayerMovement.Instance.heartShards;
        playerMana = PlayerMovement.Instance.Mana;
        playerHalfMana = PlayerMovement.Instance.halfMana;
        playerUnlockedWallJump = PlayerMovement.Instance.unlockedWallJump;
        playerUnlockedDash = PlayerMovement.Instance.unlockedDash;
        playerUnlockedJump = PlayerMovement.Instance.unlockedJump;
        playerUnlockedSideCast = PlayerMovement.Instance.unlockedSideCast;
        playerUnlockedUpCast = PlayerMovement.Instance.unlockedUpCast;
        playerUnlockedDownCast = PlayerMovement.Instance.unlockedDownCast;
        // playerUnlockedBlueDoor = PlayerMovement.Instance.unlockedBlueDoor;
        playerPosition = PlayerMovement.Instance.transform.position;
        lastScene = SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(playerPath, json);
    }

    public void LoadPlayerData()
    {
        if (File.Exists(playerPath))
        {
            string json = File.ReadAllText(playerPath);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                    playerHealth = loaded.playerHealth;
                    playerHeartShards = loaded.playerHeartShards;
                    playerMana = loaded.playerMana;
                    playerHalfMana = loaded.playerHalfMana;
                    playerUnlockedWallJump = loaded.playerUnlockedWallJump;
                    playerUnlockedDash = loaded.playerUnlockedDash;
                    playerUnlockedJump = loaded.playerUnlockedJump;
                    playerUnlockedSideCast = loaded.playerUnlockedSideCast;
                    playerUnlockedUpCast = loaded.playerUnlockedUpCast;
                    playerUnlockedDownCast = loaded.playerUnlockedDownCast;
                    // playerUnlockedBlueDoor = loaded.playerUnlockedBlueDoor;
                    playerPosition = loaded.playerPosition;
                    lastScene = loaded.lastScene;

                    SceneManager.LoadScene(lastScene);
                    PlayerMovement.Instance.transform.position = playerPosition;
                    PlayerMovement.Instance.halfMana = playerHalfMana;
                    PlayerMovement.Instance.unlockedWallJump = playerUnlockedWallJump;
                    PlayerMovement.Instance.unlockedDash = playerUnlockedDash;
                    PlayerMovement.Instance.unlockedJump = playerUnlockedJump;
                    PlayerMovement.Instance.unlockedSideCast = playerUnlockedSideCast;
                    PlayerMovement.Instance.unlockedUpCast = playerUnlockedUpCast;
                    PlayerMovement.Instance.unlockedDownCast = playerUnlockedDownCast;
                    // PlayerMovement.Instance.unlockedBlueDoor = playerUnlockedBlueDoor;
                    PlayerMovement.Instance.Health = playerHealth;
                    PlayerMovement.Instance.heartShards = playerHeartShards;
                    PlayerMovement.Instance.Mana = playerMana;
                }
                catch
                {
                    Debug.LogWarning("save.player.json is corrupted or incomplete. Resetting to defaults.");
                    ResetPlayerDefaults();
                }
            }
        }
        else
        {
            Debug.Log("Player save file doesn't exist");
            ResetPlayerDefaults();
        }
    }

    private void ResetPlayerDefaults()
    {
        PlayerMovement.Instance.halfMana = false;
        PlayerMovement.Instance.Health = PlayerMovement.Instance.maxHealth;
        PlayerMovement.Instance.heartShards = 0;
        PlayerMovement.Instance.Mana = 0.5f;
        PlayerMovement.Instance.unlockedWallJump = false;
        PlayerMovement.Instance.unlockedDash = false;
        PlayerMovement.Instance.unlockedJump = false;
        PlayerMovement.Instance.unlockedSideCast = false;
        PlayerMovement.Instance.unlockedUpCast = false;
        PlayerMovement.Instance.unlockedDownCast = false;
        // PlayerMovement.Instance.unlockedBlueDoor = false;
    }

    public void SaveShadeData()
    {
        sceneWithShade = SceneManager.GetActiveScene().name;
        shadePos = Shade.Instance.transform.position;
        shadeRot = Shade.Instance.transform.rotation;

        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(shadePath, json);
    }

    public void LoadShadeData()
    {
        if (File.Exists(shadePath))
        {
            string json = File.ReadAllText(shadePath);
            if (!string.IsNullOrEmpty(json))
            {
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                sceneWithShade = loaded.sceneWithShade;
                shadePos = loaded.shadePos;
                shadeRot = loaded.shadeRot;
            }
        }
        else
        {
            Debug.Log("Shade doesn't exist");
        }
    }
}