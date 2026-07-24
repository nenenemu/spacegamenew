using System.Collections;
using UnityEngine;

public class MaterialSpawner2D : MonoBehaviour
{
    public TMPro.TextMeshProUGUI debugText;
    public float Timer11;

    public GameObject materialPrefab;

    [Header("SpawnTime")]
    public float startInterval = 2f;      // 最初
    public float minInterval = 0.3f;      // 最速
    public float intervalDecrease = 0.05f;// 1回ごとに減る

    [Header("seiseihani")]
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

        // STARTが消えるまでタイマーを止める
        if (sm == null || !sm.canPlayerMove)
            return;

        Timer11 += Time.deltaTime;

        float remaining = 90f - Timer11;
        if (remaining < 0)
            remaining = 0;

        debugText.text = ((int)remaining).ToString();

        if (Timer11 >= 90f)
        {
            Timer11 = 90f;
            sm.PlayerSurvivedFullTime();
        }
    }




    IEnumerator SpawnLoop()
    {
        while (true)
        {
            StageManager1 sm = FindFirstObjectByType<StageManager1>();

            if (sm != null && sm.canSpawn && sm.canPlayerMove)
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