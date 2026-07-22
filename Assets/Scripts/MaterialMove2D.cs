using UnityEngine;

public class MaterialMove2D : MonoBehaviour
{
    public Sprite[] sprites = new Sprite[8];
    public int materialIndex;

    public float speed = 5f;

    Rigidbody2D rb;
    SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        materialIndex = Random.Range(0, sprites.Length);
        UpdateMaterial();

        Vector2 dir;
        do { dir = Random.insideUnitCircle; }
        while (dir.magnitude < 0.2f);

        rb.linearVelocity = dir.normalized * speed;
    }



    void FixedUpdate()
    {
        rb.linearVelocity = rb.linearVelocity.normalized * speed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // プレイヤーには何もしない（HP処理はプレイヤー側）
        if (col.gameObject.CompareTag("Wall"))
        {
            materialIndex++;
            if (materialIndex >= sprites.Length)
                materialIndex = 0;

            UpdateMaterial();
        }
    }

    void UpdateMaterial()
    {
        sr.sprite = sprites[materialIndex];
    }
}
