using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Test : MonoBehaviour
{
    private bool inmovie = false;

    [Header("動画")]
    public VideoClip openingMovie;
    public VideoClip endingMovie;

    private SceneFadeManager sceneFade;

    [Header("シーン移動用フェード")]
    public Image sceneFadeImage;
    public float sceneFadeTime = 1f;

    private bool sceneFadeEnd = false;

    [Header("フェード用")]
    public Image fadeImage;
    public float fadeTime = 1.0f;

    [Header("紙芝居用暗転")]
    public Image sibaiFadeImage;
    public float sibaiFadeTime = 1.0f;


    [Header("タイトル選択UI")]
    public Button startButton;
    public Button optionsButton;

    private int titleIndex = 0; // 0 = Start, 1 = Options



    public StageManager1 stageManager;

    [Header("紙芝居終了後")]
    public float kaiwaWaitTime = 2.0f;

    [Header("紙芝居設定")]
    public float slidespeed = 1.0f;
    public float slideDistance = 1000f;

    private int sibaiIndex = 0;
    private bool sibaiMove = false;
    private bool sibaiStart = false;


    public kamikaiwaScript kamikaiwaScript;

    private JoyconManager jm;
    private bool VideoP = false;

    public bool inbuttom = false;

    public UnityEngine.UI.Image taiki;

    public RawImage movieImage;
    public VideoPlayer videoPlayer;

    public RawImage titleImage;


    [Header("紙芝居")]
    public UnityEngine.UI.Image[] sibai;


    public AudioSource audioSource;

    //public AudioClip Start1;
    //public AudioClip Defolt;


    enum State
    {
        taiki,
        FadeIn,
        StartMovieWait,
        FadeOut,
        StartMovie,
        Title,
        StartToKamisibai, //追加
        KamisibaiFadeOut, //追加
        kamisibai,
        EndMovie        //追加
    }


    private State state = State.taiki;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BGMManager.Instance.PlayTaiki();

        Color fc = fadeImage.color;
        fc.a = 0;
        fadeImage.color = fc;

        Color sc = sibaiFadeImage.color;
        sc.a = 0;
        sibaiFadeImage.color = sc;

        taiki.gameObject.SetActive(true);
        titleImage.gameObject.SetActive(false);
        movieImage.gameObject.SetActive(false);

        foreach (UnityEngine.UI.Image img in sibai)
        {
            img.gameObject.SetActive(false);
        }

        state = State.taiki;

        jm = JoyconManager.Instance;

        sceneFade = SceneFadeManager.Instance;

        // シーン移動用の黒を透明化
        StartCoroutine(StartSceneFade());
    }

    private void FixedUpdate()
    {
        if (jm != null && jm.j != null)
        {
            foreach (var jc in jm.j)
            {
                if (jc == null) continue;

                // 右Joy-Con
                if (!jc.isLeft)
                {
                    if (jc.GetButtonDown(Joycon.Button.DPAD_RIGHT))
                    {
                        inbuttom = true;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            inbuttom = true;
        }


        switch (state)
        {
            case State.taiki:
                taiki.gameObject.SetActive(true);
                titleImage.gameObject.SetActive(false);
                movieImage.gameObject.SetActive(false);

                foreach (UnityEngine.UI.Image img in sibai)
                {
                    img.gameObject.SetActive(false);
                }


                if (sceneFadeEnd && inbuttom)
                {
                    inbuttom = false;
                    StartCoroutine(FadeInToMovie());
                }



                break;



            case State.StartMovie:
                taiki.gameObject.SetActive(false);
                movieImage.gameObject.SetActive(true);
                titleImage.gameObject.SetActive(false);

                foreach (UnityEngine.UI.Image img in sibai)
                {
                    img.gameObject.SetActive(false);
                }

                if (!VideoP)
                {
                    videoPlayer.loopPointReached += MovieEnd;
                    videoPlayer.Play();
                    VideoP = true;
                }



                break;

            case State.Title:

                taiki.gameObject.SetActive(false);
                movieImage.gameObject.SetActive(false);
                titleImage.gameObject.SetActive(true);

                foreach (UnityEngine.UI.Image img in sibai)
                    img.gameObject.SetActive(false);

                // ★ 選択状態の見た目更新
                UpdateTitleSelection();

                // ★ 入力処理
                HandleTitleInput();

                break;



            case State.kamisibai:

                taiki.gameObject.SetActive(false);
                movieImage.gameObject.SetActive(false);
                titleImage.gameObject.SetActive(false);


                if (!sibaiStart)
                {
                    foreach (Image img in sibai)
                    {
                        img.gameObject.SetActive(true);

                        Color c = img.color;
                        c.a = 1;
                        img.color = c;

                        img.rectTransform.anchoredPosition = Vector2.zero;
                    }

                    sibaiStart = true;
                }


                // ★変更
                if (inbuttom)
                {
                    inbuttom = false;

                    if (!sibaiMove)
                    {
                        StartCoroutine(SlideSibai());
                    }
                }


                break;

            case State.EndMovie:


                if(inmovie)
                {
                    taiki.gameObject.SetActive(false);
                    movieImage.gameObject.SetActive(false);
                    titleImage.gameObject.SetActive(false);

                    foreach (Image img in sibai)
                        img.gameObject.SetActive(false);
                }
                else
                {
                    taiki.gameObject.SetActive(false);
                    movieImage.gameObject.SetActive(true);
                    titleImage.gameObject.SetActive(false);

                    foreach (Image img in sibai)
                        img.gameObject.SetActive(false);
                }

                    

                break;



        }



        /*movieImage.gameObject.SetActive(true);

        videoPlayer.Stop();      // 最初から再生したいので一度止める
        videoPlayer.Play();      // 再生*/
    }

    public void MovieEnd(VideoPlayer vp)
    {
        StartCoroutine(MovieEndToTitle());
    }

    public void EndingMovieEnd(VideoPlayer vp)
    {
        StartCoroutine(EndingMovieToKaiwa());
    }




    IEnumerator SlideSibai()
    {


        sibaiMove = true;

        inbuttom = false; // ★追加

        Image img = sibai[sibaiIndex];


        Vector2 startPos = img.rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.right * slideDistance;

        // ★最後のページだけはスライドしない
        if (sibaiIndex == sibai.Length - 1)
        {
            sibaiMove = true;

            // ボタンが押されたら少し待って動画
            StartCoroutine(StartEndingMovie());

            yield break;
        }


        float time = 0;


        while (time < slidespeed)
        {
            time += Time.deltaTime;


            float t = time / slidespeed;


            //右移動
            img.rectTransform.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);


            //透明化
            Color c = img.color;
            c.a = 1 - t;
            img.color = c;


            yield return null;
        }


        //消す
        img.gameObject.SetActive(false);


        sibaiIndex++;


        //まだページがある
        if (sibaiIndex < sibai.Length)
        {
            sibaiMove = false;
        }
        else
        {
            sibaiMove = true;
            StartCoroutine(StartEndingMovie());
        }
    }

    /*IEnumerator StartKaiwa()
    {
        //少し待つ
        yield return new WaitForSeconds(kaiwaWaitTime);


        //会話開始
        kamikaiwaScript.StartKaiwa("Start");

    }*/

    IEnumerator StartKaiwa()
    {
        yield return new WaitForSeconds(kaiwaWaitTime);

        stageManager.kaiwa.SetFinishEvent(() =>
        {
            BGMManager.Instance.PlayGame();

            StartCoroutine(stageManager.BeginStage(0));
        });

        kamikaiwaScript.StartKaiwa("Start");
    }

    void UpdateTitleSelection()
    {
        Color selected = Color.white;              // 選択中：100%
        Color normal = new Color(1f, 1f, 1f, 0.5f); // 通常：50%

        if (titleIndex == 0)
        {
            startButton.image.color = selected;
            optionsButton.image.color = normal;
        }
        else
        {
            startButton.image.color = normal;
            optionsButton.image.color = selected;
        }
    }


    void HandleTitleInput()
    {
        bool moved = false;

        // Joy-Con
        if (jm != null && jm.j != null)
        {
            foreach (var jc in jm.j)
            {
                if (jc == null) continue;

                // 左Joy-Con：上下選択
                if (jc.isLeft)
                {
                    float[] stick = jc.GetStick();
                    float y = stick[1];

                    if (y > 0.5f) { titleIndex = 0; moved = true; }
                    if (y < -0.5f) { titleIndex = 1; moved = true; }
                }
                else
                {
                    // 右Joy-Con：決定
                    if (jc.GetButtonDown(Joycon.Button.DPAD_RIGHT))
                    {
                        SelectTitle();
                    }
                }
            }
        }

        // キーボード
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            titleIndex = 0;
            moved = true;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            titleIndex = 1;
            moved = true;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SelectTitle();
        }

        if (moved)
            UpdateTitleSelection();
    }


    void SelectTitle()
    {
        if (titleIndex == 0)
        {
            StartCoroutine(StartKamisibaiFade());
        }
        else
        {
            // Options
            OP();
        }
    }







    public void ST()
    {
        inbuttom = false;

        sibaiIndex = 0;
        sibaiStart = false;
        sibaiMove = false;
    }

    public void OP()
    {

    }

    IEnumerator Fade(float from, float to, float time)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < time)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / time);
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }

    IEnumerator FadeInToMovie()
    {
        BGMManager.Instance.StartOpeningMovie();

        // 黒フェードを有効化
        fadeImage.gameObject.SetActive(true);

        // ① 黒フェードIN（0 → 1）
        yield return StartCoroutine(Fade(0f, 1f, fadeTime));

        // ② 黒が完全に1になった瞬間に画面切り替え
        taiki.gameObject.SetActive(false);
        titleImage.gameObject.SetActive(false);
        movieImage.gameObject.SetActive(true);

        // ③ 動画の最初のフレームを準備
        videoPlayer.clip = openingMovie;
        videoPlayer.time = 0;
        videoPlayer.Play();
        videoPlayer.Pause();   // 最初のフレームで止める

        // ★★★ ここで state を移動させる（重要） ★★★
        state = State.StartMovie;

        // ★★★ RawImage が描画されるまで 1 フレーム待つ ★★★
        yield return null;

        // ④ 黒フェードOUT（1 → 0）
        yield return StartCoroutine(Fade(1f, 0f, fadeTime));

        // ⑤ 黒が完全に透明になったら動画再生
        videoPlayer.Play();
    }




    IEnumerator MovieEndToTitle()
    {
        // 動画停止
        videoPlayer.Pause();

        // フェードアウト開始
        float t = 0;
        Color mc = movieImage.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;

            mc.a = Mathf.Lerp(1f, 0f, t / fadeTime);
            movieImage.color = mc;

            yield return null;
        }

        // 完全透明
        mc.a = 0f;
        movieImage.color = mc;

        // 消す
        movieImage.gameObject.SetActive(false);

        // 状態変更（必要なら）
        state = State.Title;

        BGMManager.Instance.PlayTitle();
    }

    IEnumerator StartKamisibaiFade()
    {
        inbuttom = false;

        state = State.StartToKamisibai;


        // 暗転
        yield return StartCoroutine(SibaiFade(0f, 1f, sibaiFadeTime));


        // 黒画面中に紙芝居初期化
        ST();


        // 黒画面維持
        yield return new WaitForSeconds(0.3f);


        // 紙芝居表示状態へ
        state = State.kamisibai;

        BGMManager.Instance.PlayKamishibai();


        // まだ黒いので1フレーム待つ
        yield return null;


        // 暗転解除
        yield return StartCoroutine(SibaiFade(1f, 0f, sibaiFadeTime));


        // 念のため入力禁止解除
        inbuttom = false;
    }

    IEnumerator SibaiFade(float from, float to, float time)
    {
        float t = 0;
        Color c = sibaiFadeImage.color;

        while (t < time)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(from, to, t / time);
            sibaiFadeImage.color = c;

            yield return null;
        }

        c.a = to;
        sibaiFadeImage.color = c;
    }

    IEnumerator StartSceneFade()
    {
        if (sceneFade == null)
        {
            Debug.LogError("SceneFadeManagerがありません");
            yield break;
        }


        yield return StartCoroutine(sceneFade.Fade(1f, 0f));

        sceneFadeEnd = true;
    }

    IEnumerator StartEndingMovie()
    {
        // 最後の絵を見せる時間
        yield return new WaitForSeconds(2f);

        state = State.EndMovie;

        movieImage.gameObject.SetActive(true);

        Color c = movieImage.color;
        c.a = 1f;
        movieImage.color = c;

        videoPlayer.Stop();

        videoPlayer.clip = endingMovie;

        videoPlayer.loopPointReached -= MovieEnd;
        videoPlayer.loopPointReached -= EndingMovieEnd;
        videoPlayer.loopPointReached += EndingMovieEnd;

        videoPlayer.Play();
    }

    IEnumerator EndingMovieToKaiwa()
    {
        videoPlayer.Stop();

        // イベント解除
        videoPlayer.loopPointReached -= EndingMovieEnd;

        // ★ここでだけ暗転
        yield return StartCoroutine(SibaiFade(0f, 1f, sibaiFadeTime));

        movieImage.gameObject.SetActive(false);

        // 会話開始
        yield return StartCoroutine(StartKaiwa());

        // ★ここでは暗転解除しない

        sibaiFadeImage.gameObject.SetActive(false);

        inmovie = true;
    }

}
