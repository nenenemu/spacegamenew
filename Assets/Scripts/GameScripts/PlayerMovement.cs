using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MaterialValue
{
    public int baby1;
    public int baby2;
}

public class PlayerMovement : MonoBehaviour
{
    private bool gameEnd = false;

    [Header("制限時間")]
    public float limitTime = 60f;

    private float currentTime;

    private bool timeOver = false;
    

    [Header("方向表示")]
    public Transform arrowRoot;

    private SpriteRenderer spriteRenderer;

    [Header("サブ時自動増加")]
    public float subAddTime = 3f; //何秒ごとに増えるか
    public int subAddValue = 1;   //増える量

    private float subTimer = 0f;

    private float inputCooldown = 0.2f;
    private float inputTimer = 0f;

    [Header("素材ゲージ増減")]
    public MaterialValue Sipasent = new();
    public MaterialValue Cpasent = new();
    public MaterialValue Hepasent = new();
    public MaterialValue Hpasent = new();
    public MaterialValue Mgpasent = new();
    public MaterialValue Npasent = new();
    public MaterialValue Fepasent = new();
    public MaterialValue Nipasent = new();
    public MaterialValue Opasent = new();

    //-------------------------
    // 赤ちゃん
    //-------------------------
    private bool baby1Select = true;

    private int baby1jundo = 50;
    private int baby2jundo = 50;

    //-------------------------
    // UI
    //-------------------------
    [Header("UI")]
    public GameObject UIs;

    public Image gage;
    public Image gagesita;

    public Image gage2;
    public Image gagesita2;

    public TMPro.TextMeshProUGUI timerText;

    //-------------------------
    // 赤ちゃんオブジェクト
    //-------------------------
    [Header("赤ちゃん")]
    public GameObject child;
    public GameObject child2;

    private GameObject currentChild;
    private GameObject subChild;

    //-------------------------
    // その他
    //-------------------------
    public StageManager1 stageManager;

    [Header("カメラ")]
    public Camera mainCamera;

    public float minY = 0f;
    public float maxY = 17840f;

    public GameObject Empty1;

    private JoyconManager jm;
    private Rigidbody2D rb;

    [Header("噴射力")]
    public float thrustPower = 20f;

    //----------------------------------------------------
    void Start()
    {
        UIs = GameObject.Find("UIs");

        if (UIs != null)
        {
            UIs.SetActive(true);

            gage = UIs.transform.Find("gage").GetComponent<Image>();
            gagesita = UIs.transform.Find("gagesita").GetComponent<Image>();

            gage2 = UIs.transform.Find("gage2").GetComponent<Image>();
            gagesita2 = UIs.transform.Find("gagesita2").GetComponent<Image>();

            timerText =
            UIs.transform.Find("Timer")
            .GetComponent<TMPro.TextMeshProUGUI>();

            gage.gameObject.SetActive(true);
            gagesita.gameObject.SetActive(true);

            gage2.gameObject.SetActive(true);
            gagesita2.gameObject.SetActive(true);

            timerText.gameObject.SetActive(true);

            UpdateTimerUI();

            
        }

        currentChild = child;
        subChild = child2;

        currentChild.SetActive(true);
        subChild.SetActive(false);

        if (stageManager == null)
            stageManager = FindObjectOfType<StageManager1>();

        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        jm = JoyconManager.Instance;

        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRendererがありません！");
        }

        if (rb == null)
        {
            Debug.LogError("PlayerにRigidbody2Dが付いていません！");
            enabled = false;
            return;
        }

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        UpdateGauge();

        currentTime = limitTime;
    }
    //----------------------------------------------------
    void Update()
    {
        if (!timeOver && !gameEnd)
        {
            currentTime -= Time.deltaTime;


            UpdateTimerUI();


            if (currentTime <= 0)
            {
                currentTime = 0;
                timeOver = true;


                if (!gameEnd)
                {
                    gameEnd = true;

                    stageManager.TimeOver();
                }
            }
        }

        // クールタイム減少
        if (inputTimer > 0)
        {
            inputTimer -= Time.deltaTime;
        }


        bool change = false;


        if (inputTimer <= 0)
        {
            // Joy-Con
            if (jm != null && jm.j != null)
            {
                foreach (var jc in jm.j)
                {
                    if (jc == null) continue;

                    if (!jc.isLeft)
                    {
                        if (jc.GetButtonDown(Joycon.Button.DPAD_DOWN))
                        {
                            change = true;
                        }
                    }
                }
            }


            if (Input.GetKeyDown(KeyCode.E))
            {
                change = true;
            }
        }


        if (change)
        {
            ChangeBaby();
            inputTimer = inputCooldown;
        }


        AutoAddSubBaby();

        UpdateDirection();

        UpdateArrowRotation();
    }

    //----------------------------------------------------
    void ChangeBaby()
    {
        // 現在の赤ちゃんの位置を保存
        Vector3 pos = currentChild.transform.position;

        // 赤ちゃん入れ替え
        currentChild.SetActive(false);

        GameObject temp = currentChild;
        currentChild = subChild;
        subChild = temp;

        currentChild.transform.position = pos;
        currentChild.SetActive(true);

        // 操作対象変更
        baby1Select = !baby1Select;

        // ゲージ更新
        UpdateGauge();
    }

    //----------------------------------------------------
    void FixedUpdate()
    {
        // 赤ちゃん追従
        if (currentChild != null && Empty1 != null)
        {
            currentChild.transform.position =
                Vector3.Lerp(
                    currentChild.transform.position,
                    Empty1.transform.position,
                    0.1f);
        }

        Vector2 thrustDirection = Vector2.zero;
        bool thrust = false;
        bool joyconConnected = false;

        //-----------------------------------
        // Joy-Con
        //-----------------------------------
        if (jm != null && jm.j != null)
        {
            foreach (var jc in jm.j)
            {
                if (jc == null) continue;

                if (jc.isLeft)
                {
                    float[] stick = jc.GetStick();

                    float x = stick[0];
                    float y = stick[1];

                    if (Mathf.Abs(x) < 0.2f) x = 0;
                    if (Mathf.Abs(y) < 0.2f) y = 0;

                    thrustDirection = new Vector2(x, y);

                    joyconConnected = true;
                }
                else
                {
                    if (jc.GetButton(Joycon.Button.DPAD_RIGHT))
                    {
                        thrust = true;
                    }
                }
            }
        }

        //-----------------------------------
        // キーボード
        //-----------------------------------
        if (!joyconConnected)
        {
            thrustDirection = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));

            thrust = Input.GetKey(KeyCode.Space);
        }

        //-----------------------------------
        // 噴射
        //-----------------------------------
        if (thrust && thrustDirection.sqrMagnitude > 0.01f)
        {
            rb.AddForce(
                thrustDirection.normalized * thrustPower,
                ForceMode2D.Force);
        }
    }

    //----------------------------------------------------
    // ゲージ更新
    //----------------------------------------------------
    void UpdateGauge()
    {
        if (baby1Select)
        {
            Baby1(gage);
            Baby2(gage2);
        }
        else
        {
            Baby2(gage);
            Baby1(gage2);
        }
    }

    void Baby1(Image img)
    {
        if (img == null) return;

        img.fillAmount = Mathf.Clamp01(baby1jundo / 100f);
    }

    void Baby2(Image img)
    {
        if (img == null) return;

        img.fillAmount = Mathf.Clamp01(baby2jundo / 100f);
    }

    //----------------------------------------------------
    // 順度追加
    //----------------------------------------------------
    void AddJundo(MaterialValue value)
    {
        if (baby1Select)
        {
            baby1jundo += value.baby1;
            baby1jundo = Mathf.Clamp(baby1jundo, 0, 100);
        }
        else
        {
            baby2jundo += value.baby2;
            baby2jundo = Mathf.Clamp(baby2jundo, 0, 100);
        }

        UpdateGauge();
    }

    //----------------------------------------------------
    // 素材取得
    //----------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Si"))
            AddJundo(Sipasent);
        else if (collision.CompareTag("C"))
            AddJundo(Cpasent);
        else if (collision.CompareTag("He"))
            AddJundo(Hepasent);
        else if (collision.CompareTag("H"))
            AddJundo(Hpasent);
        else if (collision.CompareTag("Mg"))
            AddJundo(Mgpasent);
        else if (collision.CompareTag("N"))
            AddJundo(Npasent);
        else if (collision.CompareTag("Fe"))
            AddJundo(Fepasent);
        else if (collision.CompareTag("Ni"))
            AddJundo(Nipasent);
        else if (collision.CompareTag("O"))
            AddJundo(Opasent);
        else
            return;

        Destroy(collision.gameObject);
    }

    //----------------------------------------------------
    // ゴール
    //----------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // すでに終了していたら無視
        if (gameEnd)
            return;


        if (collision.gameObject.CompareTag("Goal1"))
        {
            gameEnd = true;

            stageManager.StageClear(
                1,
                baby1jundo,
                baby2jundo
            );
        }
        else if (collision.gameObject.CompareTag("Goal2"))
        {
            gameEnd = true;

            stageManager.StageClear(
                2,
                baby1jundo,
                baby2jundo
            );
        }
        else if (collision.gameObject.CompareTag("Goal3"))
        {
            gameEnd = true;

            stageManager.StageClear(
                3,
                baby1jundo,
                baby2jundo
            );
        }
        else if (collision.gameObject.CompareTag("Goal4"))
        {
            gameEnd = true;

            stageManager.StageClear(
                4,
                baby1jundo,
                baby2jundo
            );
        }
        else if (collision.gameObject.CompareTag("Goal5"))
        {
            gameEnd = true;

            stageManager.StageClear(
                5,
                baby1jundo,
                baby2jundo
            );
        }
    }

    //----------------------------------------------------
    // カメラ
    //----------------------------------------------------
    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        mainCamera.transform.position = new Vector3(
            0f,
            Mathf.Clamp(transform.position.y, minY, maxY),
            -10f
        );
    }

    void AutoAddSubBaby()
    {
        // ゴール・時間切れ後は増加停止
        if (gameEnd)
            return;


        subTimer += Time.deltaTime;


        if (subTimer >= subAddTime)
        {
            subTimer = 0;


            // baby1操作中ならbaby2がサブ
            if (baby1Select)
            {
                baby2jundo += subAddValue;
                baby2jundo = Mathf.Clamp(baby2jundo, 0, 100);
            }
            else
            {
                // baby2操作中ならbaby1がサブ
                baby1jundo += subAddValue;
                baby1jundo = Mathf.Clamp(baby1jundo, 0, 100);
            }


            UpdateGauge();
        }
    }

    void UpdateDirection()
    {
        if (spriteRenderer == null)
            return;


        float x = rb.linearVelocity.x;


        //右移動
        if (x > 0.1f)
        {
            spriteRenderer.flipX = false;
        }
        //左移動
        else if (x < -0.1f)
        {
            spriteRenderer.flipX = true;
        }
    }

    void UpdateArrowRotation()
    {
        Vector2 dir = Vector2.zero;


        // JoyCon左スティック
        if (jm != null && jm.j != null)
        {
            foreach (var jc in jm.j)
            {
                if (jc == null)
                    continue;


                if (jc.isLeft)
                {
                    float[] stick = jc.GetStick();

                    dir = new Vector2(
                        stick[0],
                        stick[1]
                    );
                }
            }
        }


        // PC操作
        if (dir.sqrMagnitude < 0.01f)
        {
            dir = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
        }


        // 入力なしなら回転維持
        if (dir.sqrMagnitude < 0.01f)
            return;


        float angle =
            Mathf.Atan2(dir.y, dir.x)
            * Mathf.Rad2Deg;


        arrowRoot.rotation =
            Quaternion.Euler(
                0,
                0,
                angle
            );
    }

    void UpdateTimerUI()
    {
        if (timerText == null)
            return;


        int time =
            Mathf.CeilToInt(currentTime);


        timerText.text =
            time.ToString();
    }
}