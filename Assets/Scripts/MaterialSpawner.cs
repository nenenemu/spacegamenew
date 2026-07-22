using UnityEngine;
using System.Collections;

public class MaterialSpawner : MonoBehaviour
{
    [Header("素材のPrefab")]
    public GameObject[] materials;

    [Header("生成ポイントの親（Empty）")]
    public Transform spawnParent;   // ← これが親 Empty

    [Header("生成ポイント（7本のEmpty）")]
    public Transform[] lanes;       // ← spawnParent の子

    [Header("プレイヤー")]
    public Transform player;

    [Header("生成ポイントのYオフセット")]
    public float spawnYOffset = -20f;

    [Header("生成間隔")]
    public float minInterval = 0.5f;
    public float maxInterval = 2.0f;

    [Header("落下速度")]
    public float minSpeed = 2.0f;
    public float maxSpeed = 6.0f;

    [Header("生成した素材の親（Stage）")]
    public Transform stageRoot;

    void Update()
    {
        // ★ 親 Empty をプレイヤーのYに追従させる
        Vector3 pos = spawnParent.position;
        pos.y = player.position.y + spawnYOffset;
        spawnParent.position = pos;
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        StageManager1 manager = FindFirstObjectByType<StageManager1>();

        while (true)
        {
            while (manager != null && !manager.canSpawn)
                yield return null;

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            if (manager != null && !manager.canSpawn)
                continue;

            Transform lane = lanes[Random.Range(0, lanes.Length)];
            GameObject prefab = materials[Random.Range(0, materials.Length)];

            GameObject obj = Instantiate(prefab, lane.position, Quaternion.identity, stageRoot);

            float speed = Random.Range(minSpeed, maxSpeed);

            MaterialFall fall = obj.AddComponent<MaterialFall>();
            fall.speed = speed;
        }
    }
}

public class MaterialFall : MonoBehaviour
{
    public float speed = 1.0f;

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.y <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
