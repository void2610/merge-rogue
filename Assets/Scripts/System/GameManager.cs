using System;
using System.Linq;
using System.Numerics;
using UnityEngine;
using R3;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Merge,
        PlayerAttack,
        EnemyAttack,
        AfterBattle,
        LevelUp,
        MapSelect,
        StageMoving,
        Event,
        GameOver,
        Clear,
    }
    public GameState state = GameState.Merge;
    
    [Header("オブジェクト")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] private Treasure treasure;
    [SerializeField] private EnemyContainer enemyContainer;
    [SerializeField] private AfterBattleUI afterBattleUI;
    
    [Header("デバッグ")]
    [SerializeField] private int debugCoin = 0;

    // Sceneのライフサイクルに合わせてDisposeするためのCompositeDisposable
    public readonly CompositeDisposable SceneDisposables = new ();
    public float TimeScale { get; private set; } = 1.0f;
    public bool IsGameOver { get; private set; } = false;
    public Player Player { get; private set; }
    public StageManager StageManager => GetComponent<StageManager>();
    public ScoreManager ScoreManager => GetComponent<ScoreManager>();
    public EnemyContainer EnemyContainer => enemyContainer;
    
    public readonly ReactiveProperty<BigInteger> Coin = new(0);
    private string _seedText;
    private int _seed = 42;
    private System.Random _random;

    public float RandomRange(float min, float max)
    {
        var randomValue = (float)(this._random.NextDouble() * (max - min) + min);
        return randomValue;
    }

    public int RandomRange(int min, int max)
    {
        var randomValue = this._random.Next(min, max);
        return randomValue;
    }
    
    public void AddCoin(int amount)
    {
        EventManager.OnCoinGain.Trigger(amount);
        var c = EventManager.OnCoinGain.GetAndResetValue();
        Coin.Value += c; 
    }
    
    public void SubCoin(int amount)
    {
        EventManager.OnCoinConsume.Trigger(amount);
        var c = EventManager.OnCoinConsume.GetAndResetValue();
        Coin.Value -= c;
    }

    public void ChangeTimeScale()
    {
        if (PlayerPrefs.GetInt("IsDoubleSpeed", 0) == 0)
        {
            TimeScale = 3.0f;
            PlayerPrefs.SetInt("IsDoubleSpeed", 1);
        }
        else
        {
            TimeScale = 1.0f;
            PlayerPrefs.SetInt("IsDoubleSpeed", 0);
        }
        Time.timeScale = TimeScale;
    }

    public void GameOver()
    {
        IsGameOver = true;
        ChangeState(GameState.GameOver);
        ScoreManager.ShowScore(StageManager.CurrentStageCount.Value + 1, EnemyContainer.DefeatedEnemyCount.Value, Coin.Value);
    }
    
    public void ChangeState(GameState newState)
    {
        state = newState;
        switch (newState)
        {
            case GameState.StageMoving:
                // レベルアップが残っている場合はレベルアップ画面を表示
                if (UIManager.Instance.remainingLevelUps > 0 && Player.CanLevelUp())
                    ChangeState(GameState.LevelUp);
                else
                    ChangeState(GameState.MapSelect);
                break;
            case GameState.Merge:
                Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
                MergeManager.Instance.StartMerge();
                break;
            case GameState.PlayerAttack:
                Physics2D.simulationMode = SimulationMode2D.Script;
                MergeManager.Instance.Attack().Forget();
                break;
            case GameState.EnemyAttack:
                EnemyContainer.Action();
                break;
            case GameState.MapSelect:
                StageManager.SetNextNodeActive();
                UIManager.Instance.OnClickMapButtonForce(true);
                break;
            case GameState.Event:
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
                    UIManager.Instance.remainingLevelUps = 0;
                    ChangeState(GameState.AfterBattle);
                }
                break;
        }
    }
    
    private void Awake()
    {
        if (Instance != null){
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if (PlayerPrefs.GetString("SeedText", "") == "")
        {
            var g = Guid.NewGuid();
            _seedText = g.ToString("N")[..8];
            _seed = _seedText.GetHashCode();
            Debug.Log("random seed: " + _seedText);
        }
        else
        {
            _seedText = PlayerPrefs.GetString("SeedText", "");
            _seed = _seedText.GetHashCode();
            Debug.Log("fixed seed: " + _seedText);
        }
        _random = new System.Random(_seed);
        DOTween.SetTweensCapacity(tweenersCapacity: 800, sequencesCapacity: 800);

        Player = playerObj.GetComponent<Player>();
        
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public void Start()
    {
        if (PlayerPrefs.GetInt("IsDoubleSpeed", 0) == 1)
        {
            TimeScale = 3.0f;
            Time.timeScale = TimeScale;
        }
        UIManager.Instance.SetSeedText(_seedText);
        
        AddCoin(Application.isEditor ? debugCoin : 10);
        
        treasure.OpenTreasure(Treasure.TreasureType.Initial);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            UIManager.Instance.OnClickPauseButton();
        if (Input.GetKeyDown(KeyCode.M))
            UIManager.Instance.OnClickMapButton();
        if (Input.GetKeyDown(KeyCode.T))
            UIManager.Instance.OnClickSpeedButton();
        if (Input.GetKeyDown(KeyCode.Q))
            UIManager.Instance.OnClickTutorialButton();
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        // シーンがアンロードされるときに購読を解除
        SceneDisposables.Clear();
        Debug.Log($"Scene {scene.name} unloaded. Subscriptions cleared.");
    }
}
