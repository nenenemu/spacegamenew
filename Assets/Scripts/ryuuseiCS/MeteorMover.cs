using UnityEngine;

public class MeteorMover : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 3f;          // 進む速さ
    public Vector2 direction = Vector2.right; // 進む方向（生成側で設定）

    [Header("曲線設定")]
    public float curveAmplitude = 1f; // 揺れ幅
    public float curveFrequency = 1f; // 揺れ速度

    private float t;

    void Update()
    {
        t += Time.deltaTime;

        // 基本移動
        transform.Translate(direction.normalized * speed * Time.deltaTime);

        // 曲線揺れ（上下）
        float offset = Mathf.Sin(t * curveFrequency) * curveAmplitude;
        transform.position += new Vector3(0, offset * Time.deltaTime, 0);
    }
}
