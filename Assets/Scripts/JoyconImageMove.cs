using UnityEngine;
using UnityEngine.UI;

public class LeftJoyconController : MonoBehaviour
{
    private JoyconManager jm;

    public RectTransform targetImage;
    public float speed = 500f;

    void Start()
    {
        jm = JoyconManager.Instance;

        if (jm == null)
        {
            Debug.LogError("JoyconManagerNoFind");
        }

        if (targetImage == null)
        {
            Debug.LogError("ImageisNoSetting");
        }
    }

    void Update()
    {
        if (jm == null || jm.j == null) return;
        if (targetImage == null) return;

        foreach (var jc in jm.j)
        {
            if (jc == null) continue;

            // ⭐ 左だけ使う
            if (!jc.isLeft) continue;

            // =========================
            // 🎮 ボタン入力（左だけ）
            // =========================
            foreach (Joycon.Button b in System.Enum.GetValues(typeof(Joycon.Button)))
            {
                if (jc.GetButtonDown(b))
                {
                    Debug.Log($"[LEFT] osareta: {b}");
                }
            }

            // =========================
            // 🕹 左スティックで移動
            // =========================
            float[] stick = jc.GetStick();

            float x = stick[0];
            float y = stick[1];

            // デッドゾーン
            if (Mathf.Abs(x) < 0.1f) x = 0;
            if (Mathf.Abs(y) < 0.1f) y = 0;

            Vector2 move = new Vector2(x, y) * speed * Time.deltaTime;

            targetImage.anchoredPosition += move;
        }
    }
}