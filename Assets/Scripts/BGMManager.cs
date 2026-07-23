using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    [Header("タップ音")]
    public AudioClip tapSE;

    public static BGMManager Instance;

    [Header("AudioSource")]
    public AudioSource bgmSource;
    public AudioSource seSource;

    [Header("BGM")]
    public AudioClip taikiBGM;
    public AudioClip titleBGM;
    public AudioClip firstkaiwaBGM;
    public AudioClip kamishibaiBGM;
    public AudioClip tuujoukaiwaBGM;

    public AudioClip stage12BGM;
    public AudioClip stage34BGM;


    //public AudioClip gameBGM;
    public AudioClip endingBGM;

    [Header("動画SE")]
    public AudioClip movieSE1;
    public AudioClip movieSE2;
    public AudioClip movieSE3;


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

    //================================================
    // BGM
    //================================================

    

    public void PlayTaiki()
    {
        PlayBGM(taikiBGM);
    }

    public void PlayTitle()
    {
        PlayBGM(titleBGM);
    }

    public void PlayKamishibai()//紙芝居威容BGｍ
    {
        PlayBGM(kamishibaiBGM);
    }

    public void Firstkaiwa()//最初の会話のやつ
    {
        PlayBGM(firstkaiwaBGM);
    }


    public void Tuujoukaiwa()//通常会話
    {
        PlayBGM(tuujoukaiwaBGM);
    }

    public void Game12BGM()//ステージ12ようのBGM
    {
        PlayBGM(stage12BGM);
    }

    public void Game34BGM()//ステージ34用のBGM
    {

        PlayBGM(stage34BGM);
    }



    public void PlayEnding()
    {
        PlayBGM(endingBGM);
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    //================================================
    // SE
    //================================================

    public void PlayMovieSE1()
    {
        seSource.PlayOneShot(movieSE1);
    }

    public void PlayMovieSE2()
    {
        seSource.PlayOneShot(movieSE2);
    }

    public void PlaySE(AudioClip clip)
    {
        seSource.PlayOneShot(clip);
    }

    //================================================
    // 動画SEを時間指定で鳴らす
    //================================================

    public void PlayMovieSE(float time1, float time2, float time3)
    {
        StartCoroutine(MovieSE(time1, time2, time3));
    }

    IEnumerator MovieSE(float time1, float time2, float time3)
    {
        yield return new WaitForSeconds(time1);
        PlayMovieSE1();

        yield return new WaitForSeconds(time2 - time1);
        PlayMovieSE2();

        yield return new WaitForSeconds(time3 - time2);
        seSource.PlayOneShot(movieSE3);
    }



    IEnumerator MovieSE(float time1, float time2)
    {
        yield return new WaitForSeconds(time1);
        PlayMovieSE1();

        yield return new WaitForSeconds(time2 - time1);
        PlayMovieSE2();
    }

    //================================================
    // 共通
    //================================================

    void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null)
        {
            Debug.LogError("BGM AudioSource がありません");
            return;
        }

        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StartOpeningMovie()
    {
        bgmSource.Stop();

        PlayMovieSE1();

        StartCoroutine(PlayMovieSE2Delay());
    }

    IEnumerator PlayMovieSE2Delay()
    {
        yield return new WaitForSeconds(3f); // 好きな秒数

        PlayMovieSE2();
    }

    public void PlayTapSE()
    {
        if (tapSE != null)
        {
            seSource.PlayOneShot(tapSE);
        }
    }
}