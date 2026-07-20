using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    private JoyconManager jm;

    public static EndingManager Instance;

    [Serializable]
    public class KaiwaData
    {
        [Header("会話画像")]
        public Image kaiwaImage;

        [Header("この画像の隠すマスク")]
        public Image[] masks;

        [HideInInspector] public Vector2[] defaultSizes;
    }

    [Header("暗転")]
    public Image fadeImage;
    public float fadeTime = 1f;

    [Header("Image1")]
    public Image image1;

    [Header("Image2")]
    public Image image2;

    [Header("Image1表示時間")]
    public float image1Time = 2.5f;

    [Header("Image2表示時間")]
    public float image2Time = 3f;

    [Header("ラスト画像1")]
    public Image lastImage1;

    [Header("ラスト画像2")]
    public Image lastImage2;

    [Header("ラスト画像表示時間")]
    public float lastImageTime = 3f;

    [Header("会話")]
    public KaiwaData[] kaiwas;

    [Header("NEXT")]
    public GameObject nextObject;

    public float eraseSpeed = 500f;

    bool endingStarted;
    int currentKaiwa;
    int lineIndex;
    bool isErasing;
    bool waitingNext;

    Vector3 image2StartPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {


        endingStarted = false;

        jm = JoyconManager.Instance;

        image1.gameObject.SetActive(false);
        image2.gameObject.SetActive(false);
        lastImage1.gameObject.SetActive(false);
        lastImage2.gameObject.SetActive(false);
        nextObject.SetActive(false);

        Color c = fadeImage.color;
        c.a = 0;
        fadeImage.color = c;

        image2StartPos = image2.rectTransform.localPosition;
    }

    public void StartEnding()
    {
        if (endingStarted) return;

        endingStarted = true;
        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        yield return StartCoroutine(Fade(0f, 1f));

        image1.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(1f, 0f));
        yield return new WaitForSeconds(image1Time);

        image1.gameObject.SetActive(false);
        image2.gameObject.SetActive(true);

        BGMManager.Instance.PlayEnding();

        yield return StartCoroutine(Image2Shake());
        yield return new WaitForSeconds(image2Time);

        yield return StartCoroutine(Fade(0f, 1f));
        image2.gameObject.SetActive(false);
        yield return StartCoroutine(Fade(1f, 0f));

        yield return StartCoroutine(KaiwaSequence());

        yield return StartCoroutine(Fade(0f, 1f));

        lastImage1.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(1f, 0f));
        yield return new WaitForSeconds(lastImageTime);

        yield return StartCoroutine(Fade(0f, 1f));
        lastImage1.gameObject.SetActive(false);

        lastImage2.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(1f, 0f));
        yield return new WaitForSeconds(lastImageTime);

        yield return StartCoroutine(SceneFadeManager.Instance.Fade(0f, 1f));

        BGMManager.Instance.PlayTaiki();

        SceneManager.LoadScene("Stage1");
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
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

    IEnumerator Image2Shake()
    {
        RectTransform rt = image2.rectTransform;
        float move = 25f;
        float wait = 0.08f;

        for (int i = 0; i < 2; i++)
        {
            rt.localPosition = image2StartPos + Vector3.left * move;
            yield return new WaitForSeconds(wait);

            rt.localPosition = image2StartPos + Vector3.right * move;
            yield return new WaitForSeconds(wait);
        }

        rt.localPosition = image2StartPos;
    }

    //====================================================
    // 会話本編（kamikaiwaScript 完全移植版）
    //====================================================
    IEnumerator KaiwaSequence()
    {
        // 全ページ初期化
        foreach (var d in kaiwas)
        {
            d.kaiwaImage.gameObject.SetActive(false);
            foreach (var m in d.masks) m.gameObject.SetActive(false);
        }

        currentKaiwa = 0;

        while (currentKaiwa < kaiwas.Length)
        {
            KaiwaData data = kaiwas[currentKaiwa];

            // ★ 削る前のサイズを毎回保存
            data.defaultSizes = new Vector2[data.masks.Length];
            for (int i = 0; i < data.masks.Length; i++)
                data.defaultSizes[i] = data.masks[i].rectTransform.sizeDelta;

            // ★ サイズを戻す
            for (int i = 0; i < data.masks.Length; i++)
                data.masks[i].rectTransform.sizeDelta = data.defaultSizes[i];

            // ★ マスク表示
            foreach (var mask in data.masks)
            {
                mask.gameObject.SetActive(true);
                var c = mask.color;
                c.a = 1f;
                mask.color = c;
            }

            // ページ画像表示
            data.kaiwaImage.gameObject.SetActive(true);

            lineIndex = 0;
            isErasing = true;
            waitingNext = false;
            nextObject.SetActive(false);

            // 削りループ
            while (true)
            {
                bool nextInput = false;


                // PC Enter
                if (Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    nextInput = true;
                }


                // Joy-Con右ボタン
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
                                nextInput = true;
                            }
                        }
                    }
                }




                // NEXT待ち
                if (waitingNext)
                {
                    if (nextInput)
                    {
                        break;
                    }
                    yield return null;
                    continue;
                }

                // スキップ
                if (isErasing && nextInput)
                {
                    foreach (var mask in data.masks)
                    {
                        var s = mask.rectTransform.sizeDelta;
                        s.x = 0;
                        mask.rectTransform.sizeDelta = s;
                        mask.gameObject.SetActive(false);
                    }

                    isErasing = false;
                    waitingNext = true;
                    nextObject.SetActive(true);
                    yield return null;
                    continue;
                }

                // 通常削り
                if (isErasing)
                {
                    var mask = data.masks[lineIndex];
                    var size = mask.rectTransform.sizeDelta;

                    size.x -= eraseSpeed * Time.deltaTime;
                    if (size.x < 0) size.x = 0;

                    mask.rectTransform.sizeDelta = size;

                    if (size.x <= 0)
                    {
                        mask.gameObject.SetActive(false);
                        lineIndex++;

                        if (lineIndex >= data.masks.Length)
                        {
                            isErasing = false;
                            waitingNext = true;
                            nextObject.SetActive(true);
                        }
                    }
                }

                yield return null;
            }

            // ページ終了処理
            nextObject.SetActive(false);
            data.kaiwaImage.gameObject.SetActive(false);
            foreach (var mask in data.masks) mask.gameObject.SetActive(false);

            currentKaiwa++;
        }

        nextObject.SetActive(false);
    }
}
