using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float speed = 3f;      // ˆÚ“®‘¬“x
    private int direction = 1;    // 1 = ‰E, -1 = ¶

    void Update()
    {
        // ‰¡ˆÚ“®
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ¶‰E‚Ì•Ç‚ÉG‚ê‚½‚ç”½“]
        if (other.CompareTag("Wall"))
        {
            direction *= -1;
        }
    }
}
