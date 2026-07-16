using UnityEngine;

public class MeteorTrigger : MonoBehaviour
{
    public MeteorSpawner spawner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spawner.SpawnAll();   // ‘S•”o‚·
            // spawner.SpawnOne(0); // 1‚Â‚¾‚¯o‚·ê‡
        }
    }
}
