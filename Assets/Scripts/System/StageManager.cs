using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using R3;

public class StageManager : MonoBehaviour
{
    private static readonly int mainTex = Shader.PropertyToID("_MainTex");

    public enum StageType
    {
        Enemy,
        Shop,
        Boss,
        Events,
        Other,
    }

    [SerializeField]
    private int enemyStageNum = 3;
    [SerializeField]
    private Material m;

    public List<StageType> stageTypes = new List<StageType>();
    public readonly ReactiveProperty<int> currentStage = new(-1);
    private int enemyStageCount;

    public StageType GetCurrentStageType()
    {
        return stageTypes[currentStage.Value];
    }

    public void NextStage()
    {
        Debug.Log("NextStage");
        Utils.Instance.WaitAndInvoke(0.2f, () =>
        {
            SeManager.Instance.PlaySe("footsteps");
        });
        DOTween.To(() => m.GetTextureOffset(mainTex), x => m.SetTextureOffset(mainTex, x), new Vector2(1, 0), 2.0f)
            .SetEase(Ease.InOutSine).OnComplete(() =>
            {
                m.SetTextureOffset(mainTex, new Vector2(0, 0));
            }).SetUpdate(true); 

        Utils.Instance.WaitAndInvoke(2.0f, () =>
        {
            if (currentStage.Value + 1 < stageTypes.Count)
                currentStage.Value++;
            else
                currentStage.Value = 0;

            switch (stageTypes[currentStage.Value])
            {
                case StageType.Enemy:
                    enemyStageCount++;
                    GameManager.Instance.enemyContainer.SpawnEnemy(enemyStageCount);
                    GameManager.Instance.ChangeState(GameManager.GameState.BattlePreparation);
                    break;
                case StageType.Boss:
                    GameManager.Instance.enemyContainer.SpawnBoss();
                    GameManager.Instance.ChangeState(GameManager.GameState.BattlePreparation);
                    break;
                case StageType.Shop:
                    GameManager.Instance.ChangeState(GameManager.GameState.Shop);
                    break;
                case StageType.Events:
                    GameManager.Instance.ChangeState(GameManager.GameState.Other);
                    break;
                case StageType.Other:
                    GameManager.Instance.ChangeState(GameManager.GameState.Other);
                    break;
            }
        });
    }

    private void DecideStage()
    {
        int shopNum = enemyStageNum / 2;
        stageTypes.Clear();
        for (int i = 0; i < enemyStageNum + shopNum; i++) stageTypes.Add(StageType.Other);

        for (int i = 0; i < shopNum; i++)
        {
            int index = GameManager.Instance.RandomRange(1, stageTypes.Count);
            if (stageTypes[index] == StageType.Shop)
            {
                i--;
                continue;
            }
            stageTypes[index] = StageType.Shop;
        }

        for (int i = 0; i < stageTypes.Count; i++)
        {
            if (stageTypes[i] == StageType.Other || stageTypes[i] == StageType.Shop)
            {
                stageTypes[i] = StageType.Enemy;
            }
        }
        stageTypes.Add(StageType.Boss);
    }

    public void Start()
    {
        DecideStage();
        // stageTypes[0] = StageType.Shop;
        m.SetTextureOffset(mainTex, new Vector2(0, 0));
    }
}
