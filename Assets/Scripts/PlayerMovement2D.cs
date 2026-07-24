using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerMovement2D : MonoBehaviour
{



    public float limitTime = 90f;

    public StageManager1 stageManager;
    public float survivalTime; // 経過時間

    [Header("GameOverImage")]
    public GameObject gameOverImage;

    [Header("HP")]
    public int maxHP = 100;
    private int hp;

    [Header("HPGage")]
    public Image hpImage;

    public float speed = 7f;

    private Rigidbody2D rb;
    private Vector2 input;


    [Header("JoyCon設Setting")]
    public bool useJoyCon = true;

    private Joycon leftJoycon;


    void Start()
    {
        hp = maxHP;
        UpdateHP();

        rb = GetComponent<Rigidbody2D>();

        if (gameOverImage != null)
            gameOverImage.SetActive(false);

        // ★ StageManager を自動取得
        stageManager = FindObjectOfType<StageManager1>();

        // ★ カメラを自動取得して位置をリセット
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        // JoyCon取得
        if (useJoyCon)
        {
            try
            {
                List<Joycon> joycons = JoyconManager.Instance.j;

                foreach (var joycon in joycons)
                {
                    if (joycon.isLeft)
                    {
                        leftJoycon = joycon;
                        break;
                    }
                }
            }
            catch
            {
                Debug.Log("JoyConNoFind");
            }
        }
    }




    void Update()
    {
        // ★ステージ開始前は時間を進めない
        if (stageManager == null || !stageManager.canSpawn)
            return;

        // 経過時間を加算
        survivalTime += Time.deltaTime;

        if (survivalTime >= limitTime)
        {
            stageManager.canSpawn = false;   // ★追加
            enabled = false;                 // ★追加

            stageManager.PlayerSurvivedFullTime();
            return;
        }

        //=========================
        // キーボード入力
        //=========================

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");


        //=========================
        // JoyCon入力
        //=========================

        if (leftJoycon != null)
        {
            float[] stick = leftJoycon.GetStick();

            input = new Vector2(
                stick[0],
                stick[1]
            );
        }

        input = input.normalized;
    }


    void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }


    void UpdateHP()
    {
        if (hpImage != null)
            hpImage.fillAmount = (float)hp / maxHP;
    }


    public void Damage(int value)
    {
        hp -= value;

        if (hp < 0)
            hp = 0;

        UpdateHP();

        if (hp == 0)
        {
            if (gameOverImage != null)
                gameOverImage.SetActive(true);

            enabled = false;
            rb.linearVelocity = Vector2.zero;

            // ★追加：ステージ3・4専用の死亡分岐
            stageManager.PlayerDiedInSurvivalStage(survivalTime);
        }

    }


    public void Heal(int value)
    {
        hp += value;

        if (hp > maxHP)
            hp = maxHP;

        UpdateHP();
    }


}