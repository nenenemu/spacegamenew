using UnityEngine;

public class Meteor : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // •Çƒ^ƒO‚È‚ç”½ŽË
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 velocity = rb.linearVelocity;

            // X•ûŒü‚¾‚¯”½“]
            velocity.x *= -1;

            rb.linearVelocity = velocity;
        }
    }
}