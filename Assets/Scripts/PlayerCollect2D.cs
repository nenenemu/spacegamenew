using UnityEngine;

public class PlayerCollect2D : MonoBehaviour
{
    PlayerMovement2D player;
    StageManager1 stageManager;

    void Start()
    {
        player = GetComponent<PlayerMovement2D>();
        stageManager = FindObjectOfType<StageManager1>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // ★ステージ1・2では素材判定を無効化
        if (stageManager.stageNumber <= 2)
            return;

        MaterialMove2D mat = col.gameObject.GetComponent<MaterialMove2D>();
        if (mat == null)
            return;

        int index = mat.materialIndex;

        // ★ステージ3・4の1回目/2回目だけ判定する
        bool isCorrect = stageManager.IsCorrectMaterial(index);

        if (isCorrect)
        {
            player.Heal(20);   // 正解 → 回復
        }
        else
        {
            player.Damage(30); // 不正解 → ダメージ
        }

        Destroy(col.gameObject);
    }
}
