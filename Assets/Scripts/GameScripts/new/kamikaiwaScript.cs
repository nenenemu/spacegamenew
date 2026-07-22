using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class KaiwaPage
{
    [Header("ページの画像（1枚絵）")]
    public UnityEngine.UI.Image pageImage;

    [Header("このページの隠すマスク")]
    public UnityEngine.UI.Image[] lineMasks;

    [HideInInspector]
    public Vector2[] defaultSizes;
}

[Serializable]
public class KaiwaData
{
    [Header("会話名")]
    public string kaiwaName;

    [Header("ページ一覧")]
    public KaiwaPage[] pages;
}

public class kamikaiwaScript : MonoBehaviour
{
    private JoyconManager jm;

    private float inputCooldown = 0.2f;
    private float inputTimer = 0f;

    public EndingManager endingManager;

    private bool firstFade = false;

    [Header("開始フェード")]
    public UnityEngine.UI.Image fadeImage;
    public float fadeTime = 1f;

    private bool isFade = false;

    public System.Action OnDialogueFinished;


    private bool waitingNext = false;
    public UnityEngine.UI.Image nextImage;   // ← NEXT 表示用


    [Header("登録する会話データ")]
    public KaiwaData[] kaiwaList;

    private KaiwaData current;
    private int pageIndex = 0;
    private int lineIndex = 0;

    private bool isErasing = false;
    public float eraseSpeed = 500f;

    public bool kaiwaBool;


    void Start()
    {
        jm = JoyconManager.Instance;
    }

    void Update()
    {
        if (!kaiwaBool || current == null)
            return;


        // 入力クールタイム減少
        if (inputTimer > 0)
        {
            inputTimer -= Time.deltaTime;
        }


        bool nextInput = false;


        // PC Enter
        if (inputTimer <= 0 && Input.GetKeyDown(KeyCode.Return))
        {
            nextInput = true;
            inputTimer = inputCooldown;
        }


        // Joy-Con右
        if (inputTimer <= 0 && jm != null && jm.j != null)
        {
            foreach (var jc in jm.j)
            {
                if (jc == null) continue;

                if (!jc.isLeft)
                {
                    if (jc.GetButtonDown(Joycon.Button.DPAD_RIGHT))
                    {
                        nextInput = true;
                        inputTimer = inputCooldown;
                    }
                }
            }
        }


        // NEXT待ち
        if (waitingNext)
        {
            if (nextInput)
            {
                waitingNext = false;
                nextImage.gameObject.SetActive(false);
                NextPage();
            }

            return;
        }


        // 削り中スキップ
        if (nextInput && !isFade && isErasing)
        {
            SkipAll();
            return;
        }


        // 通常削り
        if (isErasing)
        {
            var page = current.pages[pageIndex];
            var mask = page.lineMasks[lineIndex];

            var size = mask.rectTransform.sizeDelta;
            size.x -= eraseSpeed * Time.deltaTime;

            if (size.x < 0)
                size.x = 0;

            mask.rectTransform.sizeDelta = size;


            if (size.x <= 0)
            {
                mask.gameObject.SetActive(false);
                lineIndex++;

                if (lineIndex >= page.lineMasks.Length)
                {
                    isErasing = false;
                    waitingNext = true;
                    nextImage.gameObject.SetActive(true);
                }
            }
        }
    }

    public void StartKaiwa(string name)
    {
        BGMManager.Instance.PlayKaiwa();

        current = null;

        foreach (var d in kaiwaList)
        {
            if (d.kaiwaName == name)
            {
                current = d;
                break;
            }
        }

        if (current == null)
        {
            Debug.LogError("会話データがない: " + name);
            return;
        }

        kaiwaBool = true;
        pageIndex = 0;

        firstFade = true;

        ShowPage();

        isErasing = false;

        StartCoroutine(FadeStart());
    }

    void ShowPage()
    {
        var page = current.pages[pageIndex];

        // ★ 全ページのマスクを消す
        foreach (var p in current.pages)
        {
            foreach (var m in p.lineMasks)
                m.gameObject.SetActive(false);
        }

        // 全ページ非表示
        foreach (var p in current.pages)
            p.pageImage.gameObject.SetActive(false);

        // このページの画像を表示
        page.pageImage.gameObject.SetActive(true);

        // ★★★ ここが最重要：毎回「削る前のサイズ」を保存する
        page.defaultSizes = new Vector2[page.lineMasks.Length];
        for (int i = 0; i < page.lineMasks.Length; i++)
        {
            // 今のサイズを絶対に保存（削る前のサイズ）
            page.defaultSizes[i] = page.lineMasks[i].rectTransform.sizeDelta;
        }

        // ★★★ 次ページに行く前に絶対にサイズを戻す
        for (int i = 0; i < page.lineMasks.Length; i++)
        {
            page.lineMasks[i].rectTransform.sizeDelta = page.defaultSizes[i];
        }

        // ★ マスクを表示
        foreach (var mask in page.lineMasks)
        {
            mask.gameObject.SetActive(true);

            var c = mask.color;
            c.a = 1f;
            mask.color = c;
        }

        lineIndex = 0;

        if (firstFade)
        {
            isErasing = false;
        }
        else
        {
            isErasing = true;
        }
    }


    void NextPage()
    {
        isErasing = false;

        pageIndex++;

        if (pageIndex >= current.pages.Length)
        {
            EndKaiwa();
            return;
        }

        ShowPage();
    }

    void SkipAll()
    {
        var page = current.pages[pageIndex];

        foreach (var mask in page.lineMasks)
        {
            var s = mask.rectTransform.sizeDelta;
            s.x = 0;
            mask.rectTransform.sizeDelta = s;
            mask.gameObject.SetActive(false);
        }

        // ★ 次ページへ行かず NEXT を出す
        isErasing = false;
        waitingNext = true;
        nextImage.gameObject.SetActive(true);
    }


    /*void EndKaiwa()
    {
        kaiwaBool = false;

        foreach (var p in current.pages)
            p.pageImage.gameObject.SetActive(false);

        if (current.kaiwaName == "Stage4_Clear")
        {
            endingManager.StartEnding();
            return;
        }

        BGMManager.Instance.StopBGM();   // ←追加

        OnDialogueFinished?.Invoke();
        OnDialogueFinished = null;
    }*/

    void EndKaiwa()
    {
        kaiwaBool = false;

        foreach (var p in current.pages)
            p.pageImage.gameObject.SetActive(false);

        if (current.kaiwaName == "Stage4_Clear")
        {
            endingManager.StartEnding();
            return;
        }

        BGMManager.Instance.StopBGM();

        OnDialogueFinished?.Invoke();
        OnDialogueFinished = null;
    }


    public void SetFinishEvent(System.Action act)
    {
        OnDialogueFinished = act;
    }

    public IEnumerator FadeStart()
    {
        isFade = true;

        fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;


        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            c.a = Mathf.Lerp(1f, 0f, timer / fadeTime);
            fadeImage.color = c;

            yield return null;
        }


        // 完全透明
        c.a = 0f;
        fadeImage.color = c;

        fadeImage.gameObject.SetActive(false);


        isFade = false;
        firstFade = false;
        isErasing = true;
    }

    /*IEnumerator FadeEnd()
    {
        isFade = true;

        fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            c.a = Mathf.Lerp(0f, 1f, timer / fadeTime);
            fadeImage.color = c;

            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;

        // ページ非表示
        foreach (var p in current.pages)
            p.pageImage.gameObject.SetActive(false);

        if (current.kaiwaName == "Stage4_Clear")
        {
            endingManager.StartEnding();
            yield break;
        }

        BGMManager.Instance.StopBGM();

        OnDialogueFinished?.Invoke();
        OnDialogueFinished = null;

        isFade = false;
    }*/



}
