using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject meteorPrefab;   // 流星のPrefab
    public Transform[] spawnPoints;   // 生成ポイント（複数OK）
    public Vector2[] directions;      // 各ポイントの飛ぶ方向

    public void SpawnAll()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject m = Instantiate(meteorPrefab,
                                       spawnPoints[i].position,
                                       spawnPoints[i].rotation);

            // 方向を設定
            MeteorMover mover = m.GetComponent<MeteorMover>();
            mover.direction = directions[i];
        }
    }

    public void SpawnOne(int index)
    {
        GameObject m = Instantiate(meteorPrefab,
                                   spawnPoints[index].position,
                                   spawnPoints[index].rotation);

        MeteorMover mover = m.GetComponent<MeteorMover>();
        mover.direction = directions[index];
    }
}
