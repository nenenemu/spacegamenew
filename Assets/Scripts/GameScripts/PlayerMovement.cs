using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int SiCount = 0;
    public int CCount = 0;
    public int HeCount = 0;
    public int HCount = 0;
    public int MgCount = 0;
    public int NCount = 0;
    public int FeCount = 0;
    public int NiCount = 0;
    public int OCount = 0;

    public UnityEngine.UI.Text SiText;
    public UnityEngine.UI.Text CText;
    public UnityEngine.UI.Text HeText;
    public UnityEngine.UI.Text HText;
    public UnityEngine.UI.Text MgText;
    public UnityEngine.UI.Text NText;
    public UnityEngine.UI.Text FeText;
    public UnityEngine.UI.Text NiText;
    public UnityEngine.UI.Text OText;





    public StageManager1 stageManager;

    [Header("カメラ")]
    public Camera mainCamera;

    public float minY = 0f;
    public float maxY = 17840f;

    public GameObject child;
    public GameObject Empty1;

    private JoyconManager jm;
    private Rigidbody2D rb;

    [Header("噴射力")]
    public float thrustPower = 20f;

    void Start()
    {
        if (stageManager == null)
            stageManager = FindObjectOfType<StageManager1>();

        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        jm = JoyconManager.Instance;

        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("PlayerにRigidbody2Dが付いていません！");
            enabled = false;
            return;
        }

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    void FixedUpdate()
    {
        if (child != null && Empty1 != null)
        {
            child.transform.position =
                Vector3.Lerp(child.transform.position,
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

                // 左Joy-Con：噴射方向
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
                // 右Joy-Con：噴射
                else
                {
                    if (jc.GetButton(Joycon.Button.DPAD_RIGHT))
                    {
                        thrust = true;
                    }
                }
            }
        }

        void Update()
        {
            SiText.text = "Si: " + SiCount.ToString();
            CText.text = "C: " + CCount.ToString();
            HeText.text = "He: " + HeCount.ToString();
            HText.text = "H: " + HCount.ToString();
            MgText.text = "Mg: " + MgCount.ToString();
            NText.text = "N: " + NCount.ToString();
            FeText.text = "Fe: " + FeCount.ToString();
            NiText.text = "Ni: " + NiCount.ToString();
            OText.text = "O: " + OCount.ToString();
        }

        //-----------------------------------
        // キーボード
        //-----------------------------------
        if (!joyconConnected)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            thrustDirection = new Vector2(h, v);

            thrust = Input.GetKey(KeyCode.Space);
        }

        //-----------------------------------
        // 噴射
        //-----------------------------------
        if (thrust && thrustDirection.sqrMagnitude > 0.01f)
        {
            rb.AddForce(thrustDirection.normalized * thrustPower, ForceMode2D.Force);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Goal1"))
        {
            stageManager.StageClear(1); // ★ 次はステージ2
        }
        else if (collision.gameObject.CompareTag("Goal2"))
        {
            stageManager.StageClear(2);
        }
        else if (collision.gameObject.CompareTag("Goal3"))
        {
            stageManager.StageClear(3);
        }
        else if (collision.gameObject.CompareTag("Goal4"))
        {
            stageManager.StageClear(4);
        }
        else if (collision.gameObject.CompareTag("Goal5"))
        {
            stageManager.StageClear(5);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Si"))
        {
            SiCount++;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("C"))
        {
            CCount++;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("He"))
        {
            HeCount++;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("H"))
        {
            HCount++;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Mg"))
        {
            MgCount++;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("N"))
        {
            NCount++;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Fe"))
        {
            FeCount++;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Ni"))
        {
            NiCount++;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("O"))
        {
            OCount++;
            Destroy(collision.gameObject);
        }
    }



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
}