using System.Collections;
using UnityEngine;

public class MaterialSpawner2D : MonoBehaviour
{
    public TMPro.TextMeshProUGUI debugText;
    public float Timer11;

    public GameObject materialPrefab;

    [Header("スポーン間隔")]
    public float startInterval = 2f;      // 最初
    public float minInterval = 0.3f;      // 最速
    public float intervalDecrease = 0.05f;// 1回ごとに減る

    [Header("生成範囲")]
    public float range = 8f;

    private float currentInterval;

    void Start()
    {
        currentInterval = startInterval;
        StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        Timer11 += Time.deltaTime;

        debugText.text = ("Timer: " + Timer11.ToString());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            Spawn();

            yield return new WaitForSeconds(currentInterval);

            // 少しずつスポーンを速くする
            currentInterval -= intervalDecrease;

            if (currentInterval < minInterval)
                currentInterval = minInterval;
        }
    }

    void Spawn()
    {
        Vector2 pos = new Vector2(
            Random.Range(-range, range),
            Random.Range(-range, range));

        Instantiate(materialPrefab, pos, Quaternion.identity);
    }
}