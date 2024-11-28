using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using R3;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    [Serializable]
    private class StageData
    {
        public StageType stageType;
        public float probability;
        public int interval;
    }
    
    public enum StageType
    {
        Enemy,
        Shop,
        Boss,
        Events,
        Other,
    }

    [Header("背景")]
    [SerializeField] private Material m;
    [SerializeField] private List<GameObject> torches = new();
    [SerializeField] private Vector3 defaultTorchPosition;
    [SerializeField] private float torchInterval = 5;

    [Header("ステージ")]
    [SerializeField] private int stageLength = 10;
    [SerializeField] private List<StageData> stageData　= new();
    [SerializeField] private List<StageType> stageTypes = new();
    
    public readonly ReactiveProperty<int> currentStage = new(-1);
    private static readonly int mainTex = Shader.PropertyToID("_MainTex");
    private int enemyStageCount;
    private Tween torchTween;

    public StageType GetCurrentStageType()
    {
        return stageTypes[currentStage.Value];
    }

    public void NextStage()
    {
        // 演出
        Utils.Instance.WaitAndInvoke(0.2f, () =>
        {
            SeManager.Instance.PlaySe("footsteps");
        });
        DOTween.To(() => m.GetTextureOffset(mainTex), x => m.SetTextureOffset(mainTex, x), new Vector2(1, 0), 2.0f)
            .SetEase(Ease.InOutSine).OnComplete(() =>
            {
                m.SetTextureOffset(mainTex, new Vector2(0, 0));
                
                var tmp = torches[0];
                torches.RemoveAt(0);
                torches.Add(tmp);
                torchTween.Kill();
                tmp.transform.position = defaultTorchPosition + new Vector3(torchInterval * (torches.Count-1), 0, 0);
            }).SetUpdate(true); 
        
        for(var i = 0; i < torches.Count; i++)
        {
            var t = torches[i];
            var tween = t.transform.DOMove(t.transform.position - new Vector3(torchInterval, 0, 0), 2.0f)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
            if (i == 0) torchTween = tween;
        }
        torches[^1].SetActive(Random.Range(0.0f, 1.0f) < 0.5f);

        

        // ステージ進行
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
                    EventManager.OnShopEnter.Trigger(0);
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
        var availableStages = new List<StageType>();
        var stageIntervals = new Dictionary<StageType, int>();

        foreach (var data in stageData)
        {
            for (var i = 0; i < data.probability * 100; i++)
            {
                availableStages.Add(data.stageType);
            }
            stageIntervals[data.stageType] = 0;
        }

        for (var i = 0; i < stageLength; i++)
        {
            var selectableStages = availableStages.FindAll(stage => stageIntervals[stage] <= 0);
            if (selectableStages.Count == 0)
            {
                foreach (var key in stageIntervals.Keys.ToList())
                {
                    stageIntervals[key]--;
                }
                selectableStages = availableStages;
            }

            StageType selectedStage = selectableStages[UnityEngine.Random.Range(0, selectableStages.Count)];
            stageTypes.Add(selectedStage);

            foreach (var key in stageIntervals.Keys.ToList())
            {
                stageIntervals[key]--;
            }
            stageIntervals[selectedStage] = stageData.Find(data => data.stageType == selectedStage).interval;
        }
    }

    public void Start()
    {
        DecideStage();
        stageTypes[0] = StageType.Enemy;
        
        m.SetTextureOffset(mainTex, new Vector2(0, 0)); 
    }
}
