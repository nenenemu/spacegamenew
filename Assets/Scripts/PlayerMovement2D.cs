using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("ゲームオーバー画像")]
    public GameObject gameOverImage;

    [Header("HP")]
    public int maxHP = 100;
    private int hp;

    [Header("HPゲージ")]
    public Image hpImage;

    public float speed = 7f;

    private Rigidbody2D rb;
    private Vector2 input;


    [Header("JoyCon設定")]
    public bool useJoyCon = true;

    private Joycon leftJoycon;


    void Start()
    {
        hp = maxHP;
        UpdateHP();

        rb = GetComponent<Rigidbody2D>();

        if (gameOverImage != null)
            gameOverImage.SetActive(false);


        // JoyCon取得
        if (useJoyCon)
        {
            try
            {
                List<Joycon> joycons = JoyconManager.Instance.j;

                foreach (var joycon in joycons)
                {
                    // 左JoyCon
                    if (joycon.isLeft)
                    {
                        leftJoycon = joycon;
                        break;
                    }
                }
            }
            catch
            {
                Debug.Log("JoyCon未接続");
            }
        }
    }


    void Update()
    {
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

            Vector2 joyInput = new Vector2(
                stick[0],
                stick[1]
            );


            // JoyConが倒されている時だけ使用
            if (joyInput.magnitude > 0.15f)
            {
                input = joyInput;
            }
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
            Debug.Log("GameOver");

            if (gameOverImage != null)
                gameOverImage.SetActive(true);

            enabled = false;
            rb.linearVelocity = Vector2.zero;
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