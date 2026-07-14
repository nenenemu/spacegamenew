using UnityEngine;

public class StageManager1 : MonoBehaviour
{
    public GameObject[] stagePrefabs;
    public Transform stageRoot;
    public kamikaiwaScript kaiwa;

    private GameObject currentStage;

    // 最初の紙芝居が終わったらステージ1を生成する
    void Start()
    {
        
    }

    public void LoadStage(int index)
    {
        if (currentStage != null)
            Destroy(currentStage);

        currentStage = Instantiate(stagePrefabs[index], stageRoot);
    }

    // ★ プレイヤーから nextStage を受け取る
    public void StageClear(int nextStage)
    {
        // 生成された素材を全部消す
        DestroyAllMaterials();

        if (currentStage != null)
            Destroy(currentStage);

        // 会話終了後に次ステージを生成
        kaiwa.SetFinishEvent(() =>
        {
            LoadStage(nextStage);
        });

        kaiwa.StartKaiwa("Stage" + nextStage + "_Clear");
    }

    private void DestroyAllMaterials()
    {
        MaterialFall[] materials = FindObjectsByType<MaterialFall>(FindObjectsSortMode.None);

        foreach (MaterialFall mat in materials)
        {
            Destroy(mat.gameObject);
        }
    }


}
