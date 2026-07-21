using UnityEngine;

public class MaterialMove2D : MonoBehaviour
{

    [Header("æ“¾‰Â”\‚È‘fŞ")]
    public bool[] canCollectList = new bool[8];

    public float speed = 5f;

    [Header("F‚ğ9ŒÂ“o˜^")]
    public Color[] colors = new Color[8];

    [HideInInspector]
    public int materialIndex;

    Rigidbody2D rb;
    SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // ƒ‰ƒ“ƒ_ƒ€‚È‘fŞ
        materialIndex = Random.Range(0, 8);
        UpdateMaterial();

        // ƒ‰ƒ“ƒ_ƒ€‚È•ûŒü
        Vector2 dir;

        do
        {
            dir = Random.insideUnitCircle;
        }
        while (dir.magnitude < 0.2f);

        rb.linearVelocity = dir.normalized * speed;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = rb.linearVelocity.normalized * speed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Wall"))
            return;

        materialIndex++;

        if (materialIndex >= 8)
            materialIndex = 0;

        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        if (colors.Length > materialIndex)
            sr.color = colors[materialIndex];

        Debug.Log("Œ»İ‚ÌmaterialIndex = " + materialIndex);
    }
}