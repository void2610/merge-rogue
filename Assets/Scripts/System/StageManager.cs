using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

public class StageManager : MonoBehaviour
{
    [Header("背景制御")]
    [SerializeField] private BackgroundController backgroundController;
    

    [Header("ステージ")]
    [SerializeField] private Shop shop;
    [SerializeField] private Treasure treasure;
    [SerializeField] private StageEventProcessor stageEventProcessor;
    [SerializeField] private StageType startStage;
    public readonly ReactiveProperty<int> CurrentStageCount = new(-1);
    public static StageNode CurrentStage { get; private set; }
    
    private MapGenerator _mapGenerator;
    private StageMapRenderer _mapRenderer;
    
    public void StartFromFirstStage() => NextStage(_mapGenerator.GetStartNode()).Forget();

    public void SetNextNodeActive()
    {
        // ボスを倒したらマップを再生成して次のステージを設定
        if (CurrentStage?.Type == StageType.Boss)
        {
            _mapGenerator.GenerateMap();
            _mapRenderer.DrawMap(_mapGenerator.MapNodes, _mapGenerator.GetStageData());
            _mapRenderer.SetButtonEvents(_mapGenerator.MapNodes, n => NextStage(n).Forget());
            _mapRenderer.ChangeFocusNode(_mapGenerator.GetStartNode(), _mapGenerator.MapNodes);
            
            var startTransform = _mapGenerator.GetStartNode().Obj.GetComponent<RectTransform>();
            _mapRenderer.MovePlayerIcon(startTransform, 0f); // 即座に移動
        }
        
        _mapRenderer.SetNextNodeActive(CurrentStage, _mapGenerator.MapNodes);
    }
    
    private void SetAllNodeInactive()
    {
        _mapRenderer.SetAllNodeInactive(_mapGenerator.MapNodes);
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
        
        var nextTransform = next.Obj.GetComponent<RectTransform>();
        _mapRenderer.MovePlayerIcon(nextTransform, 0.5f);
        
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
                
                EventManager.OnBattleStart.OnNext(Unit.Default);
                break;
            case StageType.Boss:
                GameManager.Instance.EnemyContainer.SpawnBoss(CurrentStageCount.Value);
                GameManager.Instance.ChangeState(GameManager.GameState.Merge);
                
                // ボス戦もバトル開始として扱う
                EventManager.OnBattleStart.OnNext(Unit.Default);
                break;
            case StageType.Shop:
                EventManager.OnShopEnter.OnNext(Unit.Default);
                
                shop.OpenShop();
                UIManager.Instance.EnableCanvasGroup("Shop", true);
                break;
            case StageType.Rest:
                EventManager.OnRestEnter.OnNext(Unit.Default);
                
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
            default:
                throw new ArgumentOutOfRangeException(nameof(s), s, null);
        }
    }
    
    private void ChangeFocusNode(StageNode node)
    {
        _mapRenderer.ChangeFocusNode(node, _mapGenerator.MapNodes);
    }

    private void Awake()
    {
        _mapGenerator = this.GetComponent<MapGenerator>();
        _mapRenderer = this.GetComponent<StageMapRenderer>();
    }

    private void Start()
    {
        // マップを生成と描画
        _mapGenerator.GenerateMap();
        _mapGenerator.SetStartStageType(startStage);
        CurrentStage = null;
        
        _mapRenderer.DrawMap(_mapGenerator.MapNodes, _mapGenerator.GetStageData());
        _mapRenderer.SetButtonEvents(_mapGenerator.MapNodes, n => NextStage(n).Forget());
        SetAllNodeInactive();
        
        // 実際にレンダリングされたUIオブジェクトのRectTransformを使用
        var startTransform = _mapGenerator.GetStartNode().Obj.GetComponent<RectTransform>();
        _mapRenderer.MovePlayerIcon(startTransform, 0f); // 即座に移動
        
        // カーソルの初期位置を設定
        ChangeFocusNode(_mapGenerator.GetStartNode());
    }
}