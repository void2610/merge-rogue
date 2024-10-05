using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

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
    private int currentStage = -1;
    private int enemyStageCount = 0;

    public StageType GetCurrentStageType()
    {
        return stageTypes[currentStage];
    }

    public void NextStage()
    {
        SeManager.Instance.PlaySe("footsteps");
        DOTween.To(() => m.GetTextureOffset(mainTex), x => m.SetTextureOffset("_MainTex", x), new Vector2(1, 0), 2.0f).SetEase(Ease.Linear).OnComplete(() =>
        {
            m.SetTextureOffset(mainTex, new Vector2(0, 0));
        });

        Utils.Instance.WaitAndInvoke(2.0f, () =>
        {
            currentStage++;
            if (currentStage >= stageTypes.Count)
            {
                currentStage = 0;
            }
            GameManager.Instance.uiManager.UpdateStageText(currentStage + 1);

            switch (stageTypes[currentStage])
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
        stageTypes[0] = StageType.Shop;
        GameManager.Instance.uiManager.UpdateStageText(currentStage);
        m.SetTextureOffset(mainTex, new Vector2(0, 0));
    }
}
