using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


[System.Serializable]
public class ResultImageSet
{
    public Sprite[] images = new Sprite[2];
    // images[0] = 1回目の画像
    // images[1] = 2回目の画像
}


[System.Serializable]
public class BabyMovieData
{
    public string babyName;
    public VideoClip[] movies = new VideoClip[3]; // 0低 1中 2高
}

[System.Serializable]
public class StageResultMovie
{
    public string stageName;
    public BabyMovieData baby1;
    public BabyMovieData baby2;
}

[System.Serializable]
public class StageMaterialRule
{
    public bool[] correct1st = new bool[8];   // ステージ1回目の正解素材
    public bool[] correct2nd = new bool[8];   // ステージ2回目の正解素材
}

public class StageManager1 : MonoBehaviour
{
    public ResultImageSet[] resultImageSets;   // ★ステージごとに2枚ずつ入れる
   

    public Image resultImage;   // ★RawImage の子に置いた Image をここに入れる


    private bool stage3TutorialShown = false;
    private bool stage4TutorialShown = false;

    [Header("Stage Start Images")]
    public Image tutorialImage;

    public Sprite stage1Control;
    public Sprite stage1Material;
    public Sprite stage2Material;
    public Sprite stage34Control;

    public Image stageStartImage;
    public float stageStartTime = 2f;
    public float stageStartFade = 1f;

    private bool showStart = true;

    [HideInInspector]
    public bool canSpawn = false;

    public int stageNumber = 1;

    private bool isTimeOver = false;
    private bool isResultRunning = false;

    public Image timeOverImage;

    public StageResultMovie[] resultMovies;

    public StageMaterialRule[] materialRules;   // ★ステージごとの正解素材セット

    public GameObject[] stagePrefabs;
    public Transform stageRoot;
    public kamikaiwaScript kaiwa;

    public GameObject currentStage;

    private int resultBaby1;
    private int resultBaby2;

    public int stagePlayCount = 0;  // ★ステージ3・4を2回プレイするため

    public VideoPlayer videoPlayer;
    public RawImage movieImage;

    public GameObject goalImage;
    public GameObject[] hideUIObjects;

    public Image blackFade;
    public float fadeTime = 1f;

    // ★START画像
    public Image startImage;

    void Start()
    {
        if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
        }

        if (stageStartImage != null)
        {
            Color c = stageStartImage.color;
            c.a = 0f;
            stageStartImage.color = c;
            stageStartImage.gameObject.SetActive(false);
        }

        if (timeOverImage != null)
            timeOverImage.gameObject.SetActive(false);

        if (blackFade != null)
        {
            Color c = blackFade.color;
            c.a = 0;
            blackFade.color = c;
        }

        if (startImage != null)
        {
            Color sc = startImage.color;
            sc.a = 0;
            startImage.color = sc;
            startImage.gameObject.SetActive(false);
        }

    }

    public void LoadStage(int index)
    {
        stageNumber = index + 1;

        // ★追加
        if (stageNumber == 3 || stageNumber == 4)
            showStart = true;

        if (currentStage != null)
            Destroy(currentStage);

        currentStage = Instantiate(stagePrefabs[index], stageRoot);
    }

    // ★素材正解判定（PlayerMovement2D から呼ばれる）
    public bool IsCorrectMaterial(int materialIndex)
    {
        int stageIdx = stageNumber - 1;

        if (stagePlayCount == 0)
            return materialRules[stageIdx].correct1st[materialIndex];
        else
            return materialRules[stageIdx].correct2nd[materialIndex];
    }

    public void StageClear(int nextStage, int baby1, int baby2)
    {
        resultBaby1 = baby1;
        resultBaby2 = baby2;

        DestroyAllMaterials();

        // ★ステージ3・4は2回プレイしてからリザルト
        if (stageNumber == 3 || stageNumber == 4)
        {
            stagePlayCount++;

            if (stagePlayCount < 2)
            {
                showStart = true;
                StartCoroutine(StageTransitionEffect());
                return;
            }
            else
            {
                stagePlayCount = 0;
                showStart = false;   // ★2回目終了後はSTARTを出さない
            }
        }

        StartCoroutine(ResultSequence(nextStage));
    }

    private void DestroyAllMaterials()
    {
        MaterialMove2D[] mats = FindObjectsByType<MaterialMove2D>(FindObjectsSortMode.None);
        foreach (var m in mats)
            Destroy(m.gameObject);
    }

    int GetResult(int value)
    {
        if (value < 34) return 0;
        if (value < 67) return 1;
        return 2;
    }

    // ★ステージ3・4の間の演出（巨大化なし）
    IEnumerator StageTransitionEffect()
    {
        canSpawn = false;

        // ゴール画像が出ていたら少し待ってから消す
        if (goalImage != null && goalImage.activeSelf)
        {
            yield return new WaitForSeconds(2f);
            goalImage.SetActive(false);
        }
        // 暗転
        yield return StartCoroutine(Fade(blackFade, 0, 1));

        // ★暗転したまま1秒待つ
        yield return new WaitForSecondsRealtime(1f);

        // 同じステージをもう一度ロード
        LoadStage(stageNumber - 1);
        DestroyStageMaterials();

        // ★ロード後も暗転したまま0.5秒待つ
        yield return new WaitForSecondsRealtime(0.5f);
        // 明転
        yield return StartCoroutine(Fade(blackFade, 1, 0));

        // ゲームはまだ止める
        canSpawn = false;

        if (showStart)
        {
            stageStartImage.gameObject.SetActive(true);

            Color c = stageStartImage.color;
            c.a = 0f; // ← ここを追加（重要）
            stageStartImage.color = c;

            // ここからフェードイン
            yield return new WaitForSecondsRealtime(stageStartTime);

            float t = 0f;
            while (t < stageStartFade)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / stageStartFade);
                stageStartImage.color = c;
                yield return null;
            }

            c.a = 0f;
            stageStartImage.color = c;
            stageStartImage.gameObject.SetActive(false);
        }


        // ここでゲーム開始
        canSpawn = true;
    }


    IEnumerator ResultSequence(int nextStage)
    {
        if (isResultRunning)
            yield break;

        isResultRunning = true;

        if (!isTimeOver)
        {
            if (goalImage != null)
                goalImage.SetActive(true);

            yield return new WaitForSecondsRealtime(2f);
        }

        foreach (GameObject obj in hideUIObjects)
            if (obj != null) obj.SetActive(false);

        yield return StartCoroutine(Fade(blackFade, 0, 1));

        if (goalImage != null)
            goalImage.SetActive(false);

        if (currentStage != null)
            Destroy(currentStage);



        movieImage.gameObject.SetActive(true);

        // ★1回目の画像を表示（RawImage の子 Image）
        resultImage.sprite = resultImageSets[nextStage - 1].images[0];
        resultImage.gameObject.SetActive(true);


        videoPlayer.clip =
            resultMovies[nextStage - 1].baby1.movies[
                GetResult(resultBaby1)
            ];

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
        yield return null;
        videoPlayer.Pause();

        yield return StartCoroutine(Fade(blackFade, 1, 0));

        videoPlayer.Play();
        while (videoPlayer.isPlaying)
            yield return null;

        // ★2回目の画像に切り替え
        resultImage.sprite = resultImageSets[nextStage - 1].images[1];


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
            StartCoroutine(BeginStage(nextStage));
            isResultRunning = false;
        });

        kaiwa.StartKaiwa("Stage" + nextStage + "_Clear");
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
            c.a = Mathf.Lerp(start, end, t / fadeTime);
            img.color = c;
            yield return null;
        }

        c.a = end;
        img.color = c;
    }

    // ★120秒生存 → 低純度扱い
    // ★120秒生存時
    public void PlayerSurvivedFullTime()
    {
        float time = 120f;
        int eval = GetSurvivalEval(time);

        // ★ステージ3・4は2回プレイするので、何回目かで入れ先を変える
        if (stagePlayCount == 0)
        {
            // 1回目 → 惑星1の評価
            resultBaby1 = eval;
        }
        else
        {
            // 2回目 → 惑星2の評価
            resultBaby2 = eval;
        }

        StageClear(stageNumber, resultBaby1, resultBaby2);
    }




    // ★死亡時の時間分岐
    // ★死亡時の時間分岐
    public void PlayerDiedInSurvivalStage(float time)
    {
        int eval = GetSurvivalEval(time);

        if (stagePlayCount == 0)
        {
            // 1回目 → 惑星1の評価
            resultBaby1 = eval;
        }
        else
        {
            // 2回目 → 惑星2の評価
            resultBaby2 = eval;
        }

        StageClear(stageNumber, resultBaby1, resultBaby2);
    }


    int GetSurvivalEval(float time)
    {
        if (time >= 120f) return 100;   // 120秒生存
        if (time >= 80f) return 100;   // 80〜120秒で死亡
        if (time >= 30f) return 50;    // 30〜80秒で死亡
        return 0;                       // 0〜30秒で死亡
    }




    void DestroyStageMaterials()
    {
        string[] tags =
        {
        "Si",
        "C",
        "He",
        "H",
        "Mg",
        "N",
        "Fe",
        "Ni",
        "O"
    };

        foreach (string tag in tags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objs)
                Destroy(obj);
        }
    }

    IEnumerator StageStartEffect()
    {
        canSpawn = false;

        // 白画像を表示
        stageStartImage.gameObject.SetActive(true);

        Color c = stageStartImage.color;
        c.a = 1f;
        stageStartImage.color = c;

        // 白→透明
        float t = 0f;
        while (t < stageStartFade)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / stageStartFade);
            stageStartImage.color = c;
            yield return null;
        }

        stageStartImage.gameObject.SetActive(false);

        // START画像
        startImage.gameObject.SetActive(true);

        Color sc = startImage.color;
        sc.a = 1f;
        startImage.color = sc;

        yield return new WaitForSecondsRealtime(stageStartTime);

        t = 0f;
        while (t < stageStartFade)
        {
            t += Time.deltaTime;
            sc.a = Mathf.Lerp(1f, 0f, t / stageStartFade);
            startImage.color = sc;
            yield return null;
        }

        startImage.gameObject.SetActive(false);

        canSpawn = true;
    }

    public IEnumerator BeginStage(int nextStage)
    {
        canSpawn = false;

        // 前のステージを消す
        if (currentStage != null)
        {
            Destroy(currentStage);
            currentStage = null;
        }

        // 残っている素材も全部消す
        DestroyStageMaterials();

        yield return null;

        LoadStage(nextStage);

        yield return StartCoroutine(Fade(blackFade, 1, 0));

        // ステージ1
        if (nextStage == 0)
        {
            yield return StartCoroutine(
                ShowTutorial(
                    stage1Control,
                    stage1Material
                ));
        }
        // ステージ2
        else if (nextStage == 1)
        {
            yield return StartCoroutine(
                ShowTutorial(
                    stage2Material
                ));
        }
        // ステージ3だけ説明を出す
        else if (nextStage == 2)
        {
            if (!stage3TutorialShown)
            {
                stage3TutorialShown = true;

                yield return StartCoroutine(
                    ShowTutorial(
                        stage34Control
                    ));
            }
        }

        canSpawn = true;
    }


    IEnumerator ShowTutorial(params Sprite[] sprites)
    {
        canSpawn = false;

        JoyconManager jm = JoyconManager.Instance;

        tutorialImage.gameObject.SetActive(true);

        foreach (Sprite s in sprites)
        {
            tutorialImage.sprite = s;

            while (true)
            {
                bool next = Input.GetKeyDown(KeyCode.Return);

                if (!next && jm != null && jm.j != null)
                {
                    foreach (var jc in jm.j)
                    {
                        if (jc == null) continue;

                        if (!jc.isLeft &&
                            jc.GetButtonDown(Joycon.Button.DPAD_RIGHT))
                        {
                            next = true;
                            break;
                        }
                    }
                }

                if (next)
                    break;

                yield return null;
            }
        }

        tutorialImage.gameObject.SetActive(false);

        canSpawn = true;
    }
}
