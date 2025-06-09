using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [Header("背景制御")]
    [SerializeField] private BackgroundController backgroundController;
    
    [Header("マップシステム")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private StageMapRenderer mapRenderer;

    [Header("ステージ")]
    [SerializeField] private Shop shop;
    [SerializeField] private Treasure treasure;
    [SerializeField] private StageEventProcessor stageEventProcessor;
    [SerializeField] private StageType startStage;
    public readonly ReactiveProperty<int> CurrentStageCount = new(-1);
    public static StageNode CurrentStage { get; private set; } = null;
    
    public void StartFirstStage() => NextStage(mapGenerator.MapNodes[0][0]).Forget();

    private void OnNodeClick(StageNode node)
    {
        NextStage(node).Forget();
    }

    public void SetNextNodeActive()
    {
        // ボスを倒したらマップを再生成して次のステージを設定
        if (CurrentStage?.Type == StageType.Boss)
        {
            mapGenerator.GenerateMap();
            mapRenderer.DrawMap(mapGenerator.MapNodes, mapGenerator.GetStageData());
            mapRenderer.SetButtonEvents(mapGenerator.MapNodes, OnNodeClick);
            mapRenderer.ChangeFocusNode(mapGenerator.MapNodes[0][0], mapGenerator.MapNodes);
            
            var playerIcon = mapRenderer.CreatePlayerIcon();
            var lastNode = mapGenerator.MapNodes[^1][0];
            if (lastNode.Obj != null)
            {
                var pos = lastNode.Obj.GetComponent<RectTransform>().localPosition;
                mapRenderer.MovePlayerIcon(pos, 0.5f);
            }
        }
        
        mapRenderer.SetNextNodeActive(CurrentStage, mapGenerator.MapNodes);
    }
    
    private void SetAllNodeInactive()
    {
        mapRenderer.SetAllNodeInactive(mapGenerator.MapNodes);
    }

    private async UniTaskVoid NextStage(StageNode next)
    {
        if (GameManager.Instance.IsGameOver) return;
        if(next.Connections.Count > 0) ChangeFocusNode(next.Connections[0]);
        
        // 演出
        SetAllNodeInactive();
        UIManager.Instance.OnClickMapButtonForce(false);
        SeManager.Instance.WaitAndPlaySe("footsteps", 0.2f);
        backgroundController.PlayStageTransition();
        
        var pos = next.Obj.GetComponent<RectTransform>().localPosition;
        mapRenderer.MovePlayerIcon(pos, 0.5f);
        
        await UniTask.Delay(2000);
        
        // ステージ進行
        CurrentStageCount.Value++;
        CurrentStage = next;
        
        var bgmType = CurrentStage.Type switch
        {
            StageType.Enemy => BgmType.Battle,
            StageType.Boss => BgmType.Boss,
            _ => BgmType.Other
        };
        BgmManager.Instance.PlayRandomBGM(bgmType).Forget();
        
        var r = 0;
        StageType finalStage;
        if(CurrentStage.Type == StageType.Events)
        {
            // ランダムなステージに移動
            if (GameManager.Instance.RandomRange(0.0f, 1.0f) < 0.75f)
                r = 4;
            else
                r = GameManager.Instance.RandomRange(0, 4);

            var stage = (StageType)r;
            // ValueProcessorを通してステージタイプを最終決定
            finalStage = EventManager.OnStageTypeDecision.Process(stage);
        }
        else
        {
            // ValueProcessorを通してステージタイプを最終決定
            finalStage = EventManager.OnStageTypeDecision.Process(CurrentStage.Type);
        }
        
        ProcessStage(finalStage);
        
        // カーソルの位置を変更
    }

    private void ProcessStage(StageType s)
    {
        switch (s)
        {
            case StageType.Enemy:
                // 敵の出現量と強さを設定
                GameManager.Instance.EnemyContainer.SpawnEnemy(CurrentStageCount.Value + 1, CurrentStageCount.Value);
                GameManager.Instance.ChangeState(GameManager.GameState.Merge);
                
                EventManager.OnBattleStart.OnNext(R3.Unit.Default);
                break;
            case StageType.Boss:
                GameManager.Instance.EnemyContainer.SpawnBoss(CurrentStageCount.Value);
                GameManager.Instance.ChangeState(GameManager.GameState.Merge);
                
                // ボス戦もバトル開始として扱う
                EventManager.OnBattleStart.OnNext(R3.Unit.Default);
                break;
            case StageType.Shop:
                EventManager.OnShopEnter.OnNext(R3.Unit.Default);
                
                shop.OpenShop();
                UIManager.Instance.EnableCanvasGroup("Shop", true);
                break;
            case StageType.Rest:
                EventManager.OnRestEnter.OnNext(R3.Unit.Default);
                
                UIManager.Instance.EnableCanvasGroup("Rest", true);
                break;
            case StageType.Treasure:
                treasure.OpenTreasure(Treasure.TreasureType.Normal);
                break;
            case StageType.Events:
                EventManager.OnEventStageEnter.OnNext(StageType.Events);
                UIManager.Instance.EnableCanvasGroup("Event", true);
                stageEventProcessor.StartEvent();
                break;
            case StageType.Undefined:
            default:
                throw new ArgumentOutOfRangeException(nameof(s), s, null);
        }
    }
    
    private void ChangeFocusNode(StageNode node)
    {
        mapRenderer.ChangeFocusNode(node, mapGenerator.MapNodes);
    }

    public void Start()
    {
        // MapGeneratorとMapRendererを初期化
        mapRenderer.Initialize(mapGenerator);
        
        // マップを生成と描画
        mapGenerator.GenerateMap();
        mapGenerator.SetStartStageType(startStage);
        CurrentStage = null;
        
        mapRenderer.DrawMap(mapGenerator.MapNodes, mapGenerator.GetStageData());
        mapRenderer.SetButtonEvents(mapGenerator.MapNodes, OnNodeClick);
        SetAllNodeInactive();
        
        mapRenderer.CreatePlayerIcon();
        
        // カーソルの初期位置を設定
        ChangeFocusNode(mapGenerator.MapNodes[0][0]);
    }
}