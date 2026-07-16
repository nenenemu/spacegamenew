using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class BabyMovieData
{
    public string babyName;

    [Header("0:低 1:中 2:高")]
    public VideoClip[] movies = new VideoClip[3];
}

[System.Serializable]
public class StageResultMovie
{
    public string stageName;

    [Header("1体目")]
    public BabyMovieData baby1;

    [Header("2体目")]
    public BabyMovieData baby2;
}

public class StageManager1 : MonoBehaviour
{
    public int stageNumber = 1;   // 現在のステージ番号


    private bool isTimeOver = false;

    [Header("時間切れ")]
    public Image timeOverImage;


    [Header("ステージごとの結果動画")]
    public StageResultMovie[] resultMovies;


    public GameObject[] stagePrefabs;
    public Transform stageRoot;
    public kamikaiwaScript kaiwa;

    public GameObject currentStage;

    // ゴール時の純度保存
    private int resultBaby1;
    private int resultBaby2;


    [Header("結果動画")]
    public VideoPlayer videoPlayer;
    public RawImage movieImage;

    [Header("ゴール演出")]
    public GameObject goalImage;

    public float goalDisplayTime = 2f;

    [Header("ゴール時に消すUI")]
    public GameObject[] hideUIObjects;


    [Header("暗転")]
    public Image whiteFade;
    public Image blackFade;

    public float fadeTime = 1f;

    // 最初の紙芝居が終わったらステージ1を生成する
    void Start()
    {
        if (timeOverImage != null)
        {
            timeOverImage.gameObject.SetActive(false);
        }

        if (blackFade != null)
        {
            Color c = blackFade.color;
            c.a = 0;
            blackFade.color = c;
        }
    }

    public void LoadStage(int index)
    {
        stageNumber = index + 1;  // 1始まりにする

        if (currentStage != null)
            Destroy(currentStage);

        currentStage = Instantiate(stagePrefabs[index], stageRoot);
    }


    // ★ プレイヤーから nextStage を受け取る
    public void StageClear(
    int nextStage,
    int baby1,
    int baby2
)
    {
        resultBaby1 = baby1;
        resultBaby2 = baby2;


        DestroyAllMaterials();


        StartCoroutine(ResultSequence(nextStage));
    }

    private void DestroyAllMaterials()
    {
        MaterialFall[] materials = FindObjectsByType<MaterialFall>(FindObjectsSortMode.None);

        foreach (MaterialFall mat in materials)
        {
            Destroy(mat.gameObject);
        }
    }


    int GetResult(int value)
    {
        if (value < 34)
            return 0;

        if (value < 67)
            return 1;

        return 2;
    }

    IEnumerator ResultSequence(int nextStage)
    {
        //-------------------------
        // ゴール表示
        //-------------------------




        if (!isTimeOver)
        {
            if (goalImage != null)
                goalImage.SetActive(true);


            yield return new WaitForSecondsRealtime(2f);
        }


        //-------------------------
        // UI非表示
        //-------------------------

        foreach (GameObject obj in hideUIObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }


        //-------------------------
        // 白暗転
        //-------------------------

        yield return StartCoroutine(
            Fade(blackFade, 0, 1)
        );


        //-------------------------
        // 完全白
        //-------------------------

        if (goalImage != null)
            goalImage.SetActive(false);



        //-------------------------
        // ステージ削除
        //-------------------------

        if (currentStage != null)
            Destroy(currentStage);



        //-------------------------
        // 動画準備
        //-------------------------

        movieImage.gameObject.SetActive(true);


        videoPlayer.clip =
            resultMovies[nextStage - 1].baby1.movies[
                GetResult(resultBaby1)
            ];


        videoPlayer.Prepare();


        while (!videoPlayer.isPrepared)
            yield return null;



        // 1フレームだけ読み込み
        videoPlayer.Play();

        yield return null;

        videoPlayer.Pause();



        //-------------------------
        // 白解除
        //-------------------------

        yield return StartCoroutine(
        Fade(blackFade, 1, 0)
        );


        //-------------------------
        // 完全透明になった後
        //-------------------------

        Time.timeScale = 1f;


        // 1フレーム待つ
        yield return null;


        // 完全透明後に再生
        videoPlayer.Play();



        while (videoPlayer.isPlaying)
            yield return null;



        //-------------------------
        // 2本目
        //-------------------------

        yield return StartCoroutine(
            PlayMovie(
                resultMovies[nextStage - 1].baby2.movies[
                    GetResult(resultBaby2)
                ]
            )
        );


        movieImage.gameObject.SetActive(false);



        kaiwa.SetFinishEvent(() =>
        {
            LoadStage(nextStage);
        });


        kaiwa.StartKaiwa(
            "Stage" + nextStage + "_Clear"
        );
    }

    IEnumerator PlayMovie(VideoClip clip)
    {
        videoPlayer.clip = clip;

        videoPlayer.Prepare();


        while (!videoPlayer.isPrepared)
            yield return null;


        videoPlayer.Play();


        while (videoPlayer.isPlaying)
            yield return null;
    }

    IEnumerator Fade(Image img, float start, float end)
    {
        img.gameObject.SetActive(true);


        Color c = img.color;

        float t = 0;


        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;


            c.a = Mathf.Lerp(
                start,
                end,
                t / fadeTime
            );


            img.color = c;


            yield return null;
        }


        c.a = end;
        img.color = c;
    }

    public void TimeOver()
    {
        resultBaby1 = 100;
        resultBaby2 = 100;

        StartCoroutine(TimeOverSequence());
    }

    IEnumerator TimeOverSequence()
    {
        Time.timeScale = 0f;


        // 制限時間切れ画像表示
        if (timeOverImage != null)
            timeOverImage.gameObject.SetActive(true);


        yield return new WaitForSecondsRealtime(2f);


        // UIを消す
        foreach (GameObject obj in hideUIObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }


        // 時間切れ画像を消す
        if (timeOverImage != null)
            timeOverImage.gameObject.SetActive(false);


        // ★追加
        // ステージ内に残っている素材削除
        DestroyAllMaterials();


        // 純度100扱い
        resultBaby1 = 100;
        resultBaby2 = 100;


        isTimeOver = true;


        yield return StartCoroutine(
            ResultSequence(1)
        );
    }



}
