using System;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;
using VContainer;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Merge,
        EnemyAttack,
        AfterBattle,
        LevelUp,
        MapSelect,
        GameOver,
        Clear,
    }
    public GameState state = GameState.Merge;
    
    [Header("オブジェクト")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] private Treasure treasure;
    [SerializeField] private EnemyContainer enemyContainer;
    [SerializeField] private AfterBattleUI afterBattleUI;
    [SerializeField] private StageManager stageManager;
    
    [Header("デバッグ")]
    [SerializeField] private int debugCoin;

    public float TimeScale
    {
        get => Time.timeScale;
        set => Time.timeScale = Mathf.Clamp(value, 1f, 3f);
    }
    public bool IsGameOver { get; private set; }
    public Player Player { get; private set; }
    public EnemyContainer EnemyContainer => enemyContainer;
    
    public readonly ReactiveProperty<BigInteger> Coin = new(0);
    
    private IScoreService _scoreService;
    private ScoreDisplayComponent _scoreDisplayComponent;
    private IInputProvider _inputProvider;
    private IContentService _contentService;
    private IRelicService _relicService;
    private InventoryConfiguration _inventoryConfiguration;
    private SettingsManager _settingsManager;
    
    [Inject]
    public void InjectDependencies(IScoreService scoreService, ScoreDisplayComponent scoreDisplayComponent, IInputProvider inputProvider, IContentService contentService, IRelicService relicService, InventoryConfiguration inventoryConfiguration, SettingsManager settingsManager = null)
    {
        _scoreService = scoreService;
        _scoreDisplayComponent = scoreDisplayComponent;
        _inputProvider = inputProvider;
        _contentService = contentService;
        _relicService = relicService;
        _inventoryConfiguration = inventoryConfiguration;
        _settingsManager = settingsManager;
    }
    
    public void AddCoin(int amount)
    {
        var finalAmount = EventManager.OnCoinGain.Process(amount);
        Coin.Value += finalAmount;
    }
    
    public void SubCoin(int amount)
    {
        var finalAmount = EventManager.OnCoinConsume.Process(amount);
        
        if (finalAmount < 0 || Coin.Value < finalAmount) return;
        
        SeManager.Instance.PlaySe("coin");
        Coin.Value -= finalAmount;
    }

    public void ChangeTimeScale()
    {
        // SettingsManagerから現在のゲーム速度を取得
        var gameSpeedSetting = _settingsManager?.GetSetting<SliderSetting>("ゲーム速度");
        if (gameSpeedSetting != null) TimeScale = gameSpeedSetting.CurrentValue;
    }

    public void GameOver()
    {
        IsGameOver = true;
        ChangeState(GameState.GameOver);
        _scoreDisplayComponent.ShowScore(stageManager.CurrentStageCount.Value + 1, EnemyContainer.DefeatedEnemyCount.Value, Coin.Value);
    }
    
    public void TweetScore()
    {
        _scoreService.TweetScore();
    }

    public void ChangeState(GameState newState) => ChangeStateAsync(newState).Forget();
    
    // ReSharper disable Unity.PerformanceAnalysis
    private async UniTaskVoid ChangeStateAsync(GameState newState)
    {
        state = newState;
        switch (newState)
        {
            case GameState.Merge:
                MergeManager.Instance.StartMerge();
                break;
            case GameState.EnemyAttack:
                EnemyContainer.Action();
                break;
            case GameState.MapSelect:
                // デモ版ではact2で終了
                # if DEMO_PLAY
                    if (StageManager.CurrentStage?.Type == StageType.Boss && _contentService.Act > 1)
                    {
                        UIManager.Instance.EnableCanvasGroup("Clear", true);
                        break;
                    } 
                # endif 
                
                stageManager.SetNextNodeActive();
                await UniTask.Delay(400, DelayType.UnscaledDeltaTime);
                UIManager.Instance.OnClickMapButtonForce(true);
                break;
            case GameState.AfterBattle:
                afterBattleUI.OpenAfterBattle();
                UIManager.Instance.EnableCanvasGroup("AfterBattle", true);
                break;
            case GameState.LevelUp:
                if (Player.CanLevelUp())
                {
                    UIManager.Instance.EnableCanvasGroup("LevelUp", true);
                }
                else
                {
                    Player.RemainingLevelUps = 0;
                    ChangeState(GameState.AfterBattle);
                }
                break;
        }
    }
    
    private void Awake()
    {
        if (Instance){
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        Register.Clear();
        DOTween.SetTweensCapacity(tweenersCapacity: 800, sequencesCapacity: 800);

        Player = playerObj.GetComponent<Player>();
    }

    public void Start()
    {
        // SettingsManagerから現在のゲーム速度を取得して適用
        var gameSpeedSetting = _settingsManager?.GetSetting<SliderSetting>("ゲーム速度");
        if (gameSpeedSetting != null)
        {
            TimeScale = gameSpeedSetting.CurrentValue;
            // 設定値が変更されたときの処理を登録
            gameSpeedSetting.OnValueChanged.Subscribe(speed => TimeScale = speed).AddTo(this);
        }

        AddCoin(Application.isEditor ? debugCoin : 10);
        
        // テスト用レリックの追加（エディタでのみ）
        // RelicUIManagerのStart()が実行された後に実行するため、1フレーム遅延させる
        if (Application.isEditor && _inventoryConfiguration && _inventoryConfiguration.TestRelics != null)
        {
            AddTestRelicsAfterDelay().Forget();
        }
        
        treasure.OpenTreasure(Treasure.TreasureType.Initial);
    }
    
    /// <summary>
    /// テスト用レリックを遅延追加するUniTask
    /// RelicUIManagerのStart()が実行された後に実行される
    /// </summary>
    private async UniTaskVoid AddTestRelicsAfterDelay()
    {
        // 1フレーム待機してRelicUIManagerの初期化を完了させる
        await UniTask.Yield();
        
        foreach (var relic in _inventoryConfiguration.TestRelics)
        {
            if (relic) _relicService.AddRelic(relic);
        }
    }

    private void Update()
    {
        if (_inputProvider.UI.OpenPause.triggered)
            UIManager.Instance.OnClickPauseButton();
        if (_inputProvider.UI.OpenMap.triggered)
            UIManager.Instance.OnClickMapButton();
        if (_inputProvider.UI.ChangeSpeed.triggered)
            UIManager.Instance.OnClickSpeedButton();
        if (_inputProvider.UI.OpenTutorial.triggered)
            UIManager.Instance.OnClickTutorialButton();
        if (_inputProvider.UI.ResetCursor.triggered)
            UIManager.Instance.ResetSelectedGameObject();
        if (_inputProvider.UI.ToggleVirtualMouse.triggered)
            UIManager.Instance.ToggleVirtualMouse();
        if (_inputProvider.UI.ToggleCursorState.triggered)
            UIManager.Instance.ToggleCursorState();
        if(!UIManager.Instance.IsVirtualMouseActive())
            UIManager.Instance.SetVirtualMousePosition(new Vector2(9999, 9999));
    }
}
