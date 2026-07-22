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
        StageManager1 sm = FindFirstObjectByType<StageManager1>();

        // ★ステージがまだ始まっていないならタイマーを動かさない
        if (sm == null || !sm.canSpawn)
            return;

        // ★経過時間を加算
        Timer11 += Time.deltaTime;

        // ★残り時間を計算（90秒固定）
        float remaining = 90f - Timer11;
        if (remaining < 0) remaining = 0;

        // ★整数だけ表示
        debugText.text = ((int)remaining).ToString();
    }




    IEnumerator SpawnLoop()
    {
        while (true)
        {
            StageManager1 sm = FindFirstObjectByType<StageManager1>();

            if (sm == null || sm.canSpawn)
            {
                Spawn();
            }

            yield return new WaitForSeconds(currentInterval);

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