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

    [Header("ステージごとの結果動画")]
    public StageResultMovie[] resultMovies;


    public GameObject[] stagePrefabs;
    public Transform stageRoot;
    public kamikaiwaScript kaiwa;

    private GameObject currentStage;

    // ゴール時の純度保存
    private int resultBaby1;
    private int resultBaby2;


    [Header("結果動画")]
    public VideoPlayer videoPlayer;
    public RawImage movieImage;


    [Header("暗転")]
    public Image whiteFade;
    public Image blackFade;

    public float fadeTime = 1f;

    // 最初の紙芝居が終わったらステージ1を生成する
    void Start()
    {
        
    }

    public void LoadStage(int index)
    {
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
        // ゴール画面のまま暗転
        yield return StartCoroutine(
            Fade(blackFade, 0, 1)
        );


        // 完全暗転後にステージ削除
        if (currentStage != null)
            Destroy(currentStage);



        // 動画画像表示
        movieImage.gameObject.SetActive(true);


        // 動画セット
        videoPlayer.clip =
            resultMovies[nextStage - 1].baby1.movies[
                GetResult(resultBaby1)
            ];


        videoPlayer.Prepare();


        while (!videoPlayer.isPrepared)
            yield return null;


        // ★最初の1フレームだけ表示
        videoPlayer.Play();
        yield return null;
        videoPlayer.Pause();



        // 暗転解除
        yield return StartCoroutine(
            Fade(blackFade, 1, 0)
        );


        // 動画開始
        videoPlayer.Play();


        while (videoPlayer.isPlaying)
            yield return null;



        yield return StartCoroutine(
            PlayMovie(
                resultMovies[nextStage - 1].baby2.movies[
                    GetResult(resultBaby2)
                ]
            )
        );

        // ★2本の動画が終わったので動画画面を消す
        movieImage.gameObject.SetActive(false);

        // 会話
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
            t += Time.deltaTime;

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


}
