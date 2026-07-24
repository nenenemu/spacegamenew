using UnityEngine;

public class JoyconSplitTest : MonoBehaviour
{
    private JoyconManager jm;

    void Start()
    {
        jm = JoyconManager.Instance;

        if (jm == null)
        {
            Debug.LogError("JoyconManagerNoFind");
        }
    }

    void Update()
    {
        if (jm == null || jm.j == null) return;

        foreach (var jc in jm.j)
        {
            if (jc == null) continue;

            string side = jc.isLeft ? "LEFT" : "RIGHT";

            // =========================
            // 🎮 ボタン入力
            // =========================
            foreach (Joycon.Button b in System.Enum.GetValues(typeof(Joycon.Button)))
            {
                if (jc.GetButtonDown(b))
                {
                    Debug.Log($"[{side}] osareta: {b}");
                }
            }

            // =========================
            // 🕹 スティック入力
            // =========================
            float[] stick = jc.GetStick();

            float x = stick[0];
            float y = stick[1];

            // ノイズ対策（デッドゾーン）
            if (Mathf.Abs(x) < 0.1f) x = 0;
            if (Mathf.Abs(y) < 0.1f) y = 0;

            if (x != 0 || y != 0)
            {
                Debug.Log($"[{side}] Stick X:{x:F2} Y:{y:F2}");
            }
        }
    }
}