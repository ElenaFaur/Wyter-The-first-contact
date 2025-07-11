using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private float fadeTime;
    private Image fadeOutUIImage;

    public enum FadeDirection
    {
        In,
        Out
    }

    private void Awake()
    {
        fadeOutUIImage = GetComponent<Image>();
        fadeOutUIImage.enabled = false; 
    }

    public void CallFadeAndLoadScene(string _sceneToLoad)
    {
        StartCoroutine(FadeAndLoadScene(FadeDirection.In, _sceneToLoad));
    }

     public IEnumerator FadeAndLoadScene(FadeDirection _fadeDirection, string _sceneToLoad)
    {
        fadeOutUIImage.enabled = true;

        yield return Fade(_fadeDirection);

        SceneManager.LoadScene(_sceneToLoad);
    }

    public IEnumerator Fade(FadeDirection _fadeDirection)
    {
        float _alpha = _fadeDirection == FadeDirection.Out ? 1 : 0;
        float _fadeEndValue = _fadeDirection == FadeDirection.Out ? 0 : 1;

        if (_fadeDirection == FadeDirection.Out)
        {
            while (_alpha >= _fadeEndValue)
            {
                SetColorImage(ref _alpha, _fadeDirection);
                yield return null;
            }

            fadeOutUIImage.enabled = false;
        }
        else
        {
            fadeOutUIImage.enabled = true;
            while (_alpha <= _fadeEndValue)
            {
                SetColorImage(ref _alpha, _fadeDirection);
                yield return null;
            }

        }
    }

    void SetColorImage(ref float _alpha, FadeDirection _fadeDirection)
    {
        fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, _alpha);
        _alpha += Time.deltaTime * (1 / fadeTime) * ( _fadeDirection == FadeDirection.Out ? -1 : 1);
    }
}
