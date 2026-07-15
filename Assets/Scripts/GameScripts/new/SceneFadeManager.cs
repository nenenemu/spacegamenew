using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance;

    public Image fadeImage;
    public float fadeTime = 1f;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public IEnumerator Fade(float from, float to)
    {
        float t = 0;

        Color c = fadeImage.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(from, to, t / fadeTime);
            fadeImage.color = c;

            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}