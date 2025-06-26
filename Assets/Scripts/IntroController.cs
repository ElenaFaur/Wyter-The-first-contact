using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        StartCoroutine(WaitForVideoEnd(vp));
    }

    IEnumerator WaitForVideoEnd(VideoPlayer vp)
    {
        while (vp.isPlaying)
        {
            // Exit early if player presses any key
            if (Input.anyKeyDown)
            {
                LoadMainMenu();
                yield break;
            }

            yield return null;
        }

        LoadMainMenu();
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
