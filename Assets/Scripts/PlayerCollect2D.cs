using UnityEngine;

[System.Serializable]
public class PlayerSpriteData
{
    public int stageNumber;

    // 1回目
    public Sprite firstSprite;

    // 2回目
    public Sprite secondSprite;
}

public class PlayerCollect2D : MonoBehaviour
{
    [Header("プレイヤー見た目変更")]
    public SpriteRenderer playerRenderer;

    public PlayerSpriteData[] playerSpriteDatas;

    PlayerMovement2D player;
    StageManager1 stageManager;

    // ★ Prefab内の素材画像（3つ）
    public UnityEngine.UI.Image[] materialImages;

    // ★ 1回目用の画像セット
    public Sprite[] firstSprites;

    // ★ 2回目用の画像セット
    public Sprite[] secondSprites;

    void Start()
    {
        player = GetComponent<PlayerMovement2D>();
        stageManager = FindObjectOfType<StageManager1>();

        ApplyMaterialImages();
        ApplyPlayerSprite();
    }

    public void ApplyMaterialImages()
    {
        // ステージ3・4だけ切り替える
        if (stageManager.stageNumber == 3 || stageManager.stageNumber == 4)
        {
            // ★ 1回目
            if (stageManager.stagePlayCount == 0)
            {
                for (int i = 0; i < materialImages.Length; i++)
                    materialImages[i].sprite = firstSprites[i];
            }
            // ★ 2回目
            else
            {
                for (int i = 0; i < materialImages.Length; i++)
                    materialImages[i].sprite = secondSprites[i];
            }
        }
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        if (stageManager.stageNumber <= 2)
            return;

        MaterialMove2D mat = col.gameObject.GetComponent<MaterialMove2D>();
        if (mat == null)
            return;

        int index = mat.materialIndex;

        bool isCorrect = stageManager.IsCorrectMaterial(index);

        if (isCorrect)
            player.Heal(10);
        else
            player.Damage(20);

        Destroy(col.gameObject);
    }

    public void ApplyPlayerSprite()
    {
        foreach (var data in playerSpriteDatas)
        {
            if (data.stageNumber != stageManager.stageNumber)
                continue;


            if (stageManager.stagePlayCount == 0)
            {
                playerRenderer.sprite = data.firstSprite;
            }
            else
            {
                playerRenderer.sprite = data.secondSprite;
            }


            break;
        }
    }
}


