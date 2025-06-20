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
    [SerializeField] private int debugCoin = 0;

    public float TimeScale { get; private set; } = 1.0f;
    public bool IsGameOver { get; private set; }
    public Player Player { get; private set; }
    public StageManager StageManager => stageManager;
    public EnemyContainer EnemyContainer => enemyContainer;
    
    public readonly ReactiveProperty<BigInteger> Coin = new(0);
    
    private IScoreService _scoreService;
    private ScoreDisplayComponent _scoreDisplayComponent;
    private IInputProvider _inputProvider;
    private IContentService _contentService;
    private IGameSettingsService _gameSettingsService;
    
    [Inject]
    public void InjectDependencies(IScoreService scoreService, ScoreDisplayComponent scoreDisplayComponent, IInputProvider inputProvider, IContentService contentService, IGameSettingsService gameSettingsService)
    {
        _scoreService = scoreService;
        _scoreDisplayComponent = scoreDisplayComponent;
        _inputProvider = inputProvider;
        _contentService = contentService;
        _gameSettingsService = gameSettingsService;
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
        var isDoubleSpeed = _gameSettingsService.ToggleDoubleSpeed();
        TimeScale = isDoubleSpeed ? 3.0f : 1.0f;
        Time.timeScale = TimeScale;
    }

    public void GameOver()
    {
        IsGameOver = true;
        ChangeState(GameState.GameOver);
        _scoreDisplayComponent.ShowScore(StageManager.CurrentStageCount.Value + 1, EnemyContainer.DefeatedEnemyCount.Value, Coin.Value);
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
                
                StageManager.SetNextNodeActive();
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
        if (_gameSettingsService.IsDoubleSpeedEnabled())
        {
            TimeScale = 3.0f;
            Time.timeScale = TimeScale;
        }
        
        AddCoin(Application.isEditor ? debugCoin : 10);
        
        treasure.OpenTreasure(Treasure.TreasureType.Initial);
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
