using UnityEngine;

public class PlayerCollect2D : MonoBehaviour
{
    PlayerMovement2D player;

    void Start()
    {
        player = GetComponent<PlayerMovement2D>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {


        MaterialMove2D mat = col.gameObject.GetComponent<MaterialMove2D>();

        if (mat == null)
            return;

        Debug.Log("Index = " + mat.materialIndex);
        Debug.Log("配列サイズ = " + mat.canCollectList.Length);

        Debug.Log("取得した素材");
        Debug.Log("materialIndex = " + mat.materialIndex);
        Debug.Log("canCollect = " + mat.canCollectList[mat.materialIndex]);

        if (mat.canCollectList[mat.materialIndex])
        {
            // 正解なら回復
            player.Heal(10);
        }
        else
        {
            // 不正解ならダメージ
            player.Damage(20);
        }

        Destroy(col.gameObject);
    }
}