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
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            // ★ ランダムレーン選択（親の子）
            Transform lane = lanes[Random.Range(0, lanes.Length)];

            // ★ 素材選択
            GameObject prefab = materials[Random.Range(0, materials.Length)];

            // ★ 生成（Stage の子にする）
            GameObject obj = Instantiate(prefab, lane.position, Quaternion.identity, stageRoot);

            // ★ 落下速度
            float speed = Random.Range(minSpeed, maxSpeed);

            // ★ 落下処理
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
