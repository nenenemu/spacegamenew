using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackHollScript : MonoBehaviour
{
    public float pullRadius = 5f;     // 吸引範囲
    public float pullSpeed = 0.1f;    // 吸い込む速さ
    public GameObject player;             // プレイヤーの参照

    private Transform playerTransform; // プレイヤーのTransform参照



    void Start()
    {

        //player = GameObject.FindWithTag("Player").transform; player Transform player
    }

    void Update()
    {
        playerTransform = player.transform; // プレイヤーのTransformを取得


        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist < pullRadius)
        {
            playerTransform.position = Vector3.Lerp(
                playerTransform.position,
                transform.position,
                pullSpeed
            );
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene("Action");
        }
    }
}
