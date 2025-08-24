using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;
using VContainer;

public class StageManager : MonoBehaviour
{
    [Header("ステージ")]
    [SerializeField] private Shop shop;
    [SerializeField] private Treasure treasure;
    [SerializeField] private ClearScreenView clearScreenView;
    [SerializeField] private StageType startStage;
    public readonly ReactiveProperty<int> CurrentStageCount = new(-1);
    public static StageNode CurrentStage { get; private set; }
    
    private MapGenerator _mapGenerator;
    private StageMapRenderer _mapRenderer;
    private BackgroundController _backgroundController;
    private IRandomService _randomService;
    private IContentService _contentService;
    private StageEventPresenter _stageEventPresenter;
    
    [Inject]
    public void InjectDependencies(IRandomService randomService, IContentService contentService, StageEventPresenter stageEventPresenter)
    {
        _contentService = contentService;
        _randomService = randomService;
        _stageEventPresenter = stageEventPresenter;
    }
    
    public void StartFromFirstStage() => NextStage(_mapGenerator.GetStartNode()).Forget();

    public void SetNextNodeActive()
    {
        // ボスステージの場合、ボスを倒した後に次のアクトを準備する
        if (CurrentStage?.Type == StageType.Boss)
        {
            PrepareNextAct();
            
            // デモ版ではact2で終了
            var isDemoClear = _contentService.IsDemoPlay && _contentService.Act > 1;
            clearScreenView.Show(_contentService.Act, isDemoClear);
            return;
        }
        
        // 通常のステージの場合、次のステージをアクティブにする
        _mapRenderer.SetNextNodeActive(CurrentStage, _mapGenerator.MapNodes);
    }

    /// <summary>
    /// ボスを倒した後、次のアクトを準備する。
    /// </summary>
    private void PrepareNextAct()
    {
        _mapGenerator.GenerateMap();
        _mapRenderer.DrawMap(_mapGenerator.MapNodes, _mapGenerator.GetStageData());
        _mapRenderer.SetButtonEvents(_mapGenerator.MapNodes, n => NextStage(n).Forget());
        _mapRenderer.ChangeFocusNode(_mapGenerator.GetStartNode(), _mapGenerator.MapNodes);
            
        var startTransform = _mapGenerator.GetStartNode().Obj.GetComponent<RectTransform>();
        _mapRenderer.MovePlayerIcon(startTransform, 0f); // 即座に移動
            
        // CurrentStageをnullにリセットして、スタートノードをアクティブにする
        CurrentStage = null;
        _mapRenderer.SetNextNodeActive(null, _mapGenerator.MapNodes);
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
        _backgroundController.PlayStageTransition();
        
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

        StageType finalStage;
        if(CurrentStage.Type == StageType.Events)
        {
            // ランダムなステージに移動
            var r = _randomService.Chance(0.75f) ? 4 : _randomService.RandomRange(0, 4);
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
                _stageEventPresenter.StartEvent();
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
        _backgroundController = this.GetComponent<BackgroundController>();
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