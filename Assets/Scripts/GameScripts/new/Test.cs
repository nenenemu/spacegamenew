using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Test : MonoBehaviour
{
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


    enum State//名まえがここにenumの軟化って感じ
    {
        taiki,
        StartMovie,
        Title,
        kamisibai
        
    }

    private State state = State.taiki;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        taiki.gameObject.SetActive(true);
        titleImage.gameObject.SetActive(false);
        movieImage.gameObject.SetActive(false);

        foreach (UnityEngine.UI.Image img in sibai)
        {
            img.gameObject.SetActive(false);
        }

        state = State.taiki;

        jm = JoyconManager.Instance;


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


                if (inbuttom)
                {
                    state = State.StartMovie;
                    inbuttom = false;
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


                //最初だけ全部表示
                if (!sibaiStart)
                {
                    foreach (Image img in sibai)
                    {
                        img.gameObject.SetActive(true);

                        //透明リセット
                        Color c = img.color;
                        c.a = 1;
                        img.color = c;

                        //位置リセット
                        img.rectTransform.anchoredPosition = Vector2.zero;
                    }

                    sibaiStart = true;
                }


                //入力受付
                if (inbuttom && !sibaiMove)
                {
                    inbuttom = false;

                    StartCoroutine(SlideSibai());
                }


                break;



        }



        /*movieImage.gameObject.SetActive(true);

        videoPlayer.Stop();      // 最初から再生したいので一度止める
        videoPlayer.Play();      // 再生*/
    }

    public void MovieEnd(VideoPlayer vp)
    {
        state = State.Title;
    }

    

    IEnumerator SlideSibai()
    {
        sibaiMove = true;


        Image img = sibai[sibaiIndex];


        Vector2 startPos = img.rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.right * slideDistance;


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
            StartCoroutine(StartKaiwa());

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

        // ★ 会話終了後にステージ1を生成するように StageManager に登録
        stageManager.kaiwa.SetFinishEvent(() =>
        {
            stageManager.LoadStage(0);
        });


        // ★ 紙芝居後の会話開始
        kamikaiwaScript.StartKaiwa("Start");
    }

    void UpdateTitleSelection()
    {
        Color selected = Color.yellow;
        Color normal = Color.white;

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
            // Start
            state = State.kamisibai;
            ST();
        }
        else
        {
            // Options
            OP();
        }
    }







    public void ST()
    {
        state = State.kamisibai;

        inbuttom = false;

        sibaiIndex = 0;
        sibaiStart = false;
        sibaiMove = false;
    }

    public void OP()
    {

    }
}
