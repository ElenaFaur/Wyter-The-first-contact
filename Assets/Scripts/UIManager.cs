using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject inventory;
    [SerializeField] GameObject deathScreen;
    [SerializeField] GameObject halfMana, fullMana;
    [SerializeField] public GameObject saveText;

    public enum ManaState
    {
        FullMana,
        HalfMana
    }
    public ManaState manaState;
    public SceneFader sceneFader;

    public GameObject confirmResetPanel;
    public GameObject resetEndingPanel;
    public GameObject cancelEndingPanel;

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

    private void Start()
    {
        sceneFader = GetComponentInChildren<SceneFader>(true);

        // if (deathScreen == null)
        // {
        //     deathScreen = GameObject.Find("Death Screen");
        // }
    }

    public void SwitchMana(ManaState _manaState)
    {
        switch (_manaState)
        {
            case ManaState.FullMana:
                halfMana.SetActive(false);
                fullMana.SetActive(true);
                break;
            case ManaState.HalfMana:
                fullMana.SetActive(false);
                halfMana.SetActive(true);
                break;
        }

        manaState = _manaState;
    }

    public IEnumerator ActiveDeathScreen()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.In));

        yield return new WaitForSecondsRealtime(0.8f);
        deathScreen.SetActive(true);
    }

    public IEnumerator DeactivateDeathScreen()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        deathScreen.SetActive(false);
        StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.Out));
    }
}
