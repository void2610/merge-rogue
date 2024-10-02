using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class StageManager : MonoBehaviour
{
    public enum StageType
    {
        enemy,
        shop,
        boss,
        events,
        other,
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
        SeManager.instance.PlaySe("footsteps");
        DOTween.To(() => m.GetTextureOffset("_MainTex"), x => m.SetTextureOffset("_MainTex", x), new Vector2(1, 0), 2.0f).SetEase(Ease.Linear).OnComplete(() =>
        {
            m.SetTextureOffset("_MainTex", new Vector2(0, 0));
        });

        Utils.instance.WaitAndInvoke(2.0f, () =>
        {
            currentStage++;
            if (currentStage >= stageTypes.Count)
            {
                currentStage = 0;
            }
            GameManager.instance.uiManager.UpdateStageText(currentStage + 1);

            switch (stageTypes[currentStage])
            {
                case StageType.enemy:
                    enemyStageCount++;
                    GameManager.instance.enemyContainer.SpawnEnemy(enemyStageCount);
                    GameManager.instance.ChangeState(GameManager.GameState.BattlePreparation);
                    break;
                case StageType.boss:
                    GameManager.instance.enemyContainer.SpawnBoss();
                    GameManager.instance.ChangeState(GameManager.GameState.BattlePreparation);
                    break;
                case StageType.shop:
                    GameManager.instance.ChangeState(GameManager.GameState.Shop);
                    break;
                case StageType.events:
                    GameManager.instance.ChangeState(GameManager.GameState.Other);
                    break;
                case StageType.other:
                    GameManager.instance.ChangeState(GameManager.GameState.Other);
                    break;
            }
        });
    }

    private void DecideStage()
    {
        int shopNum = enemyStageNum / 2;
        stageTypes.Clear();
        for (int i = 0; i < enemyStageNum + shopNum; i++) stageTypes.Add(StageType.other);

        for (int i = 0; i < shopNum; i++)
        {
            int index = GameManager.instance.RandomRange(1, stageTypes.Count);
            if (stageTypes[index] == StageType.shop)
            {
                i--;
                continue;
            }
            stageTypes[index] = StageType.shop;
        }

        for (int i = 0; i < stageTypes.Count; i++)
        {
            if (stageTypes[i] == StageType.other || stageTypes[i] == StageType.shop)
            {
                stageTypes[i] = StageType.enemy;
            }
        }
        stageTypes.Add(StageType.boss);
    }

    public void Start()
    {
        DecideStage();
        stageTypes[0] = StageType.shop;
        GameManager.instance.uiManager.UpdateStageText(currentStage);
        m.SetTextureOffset("_MainTex", new Vector2(0, 0));
    }
}
