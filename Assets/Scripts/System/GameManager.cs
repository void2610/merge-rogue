using System;
using System.Numerics;
using UnityEngine;
using R3;
using DG.Tweening;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (PlayerPrefs.GetString("SeedText", "") == "")
            {
                _seed = (int)DateTime.Now.Ticks;
                // Debug.Log("random seed: " + seed);
            }
            else
            {
                _seed = PlayerPrefs.GetInt("Seed", _seed);
                // Debug.Log("fixed seed: " + seed);
            }
            _random = new System.Random(_seed);
            DOTween.SetTweensCapacity(tweenersCapacity: 800, sequencesCapacity: 800);

            Player = playerObj.GetComponent<Player>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public enum GameState
    {
        Merge,
        PlayerAttack,
        EnemyAttack,
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
    [SerializeField] private EnemyContainer enemyContainer;
    [SerializeField] private Camera renderTextureCamera;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Canvas pixelCanvas;
    [SerializeField] private Canvas uiCanvas;

    public float TimeScale { get; private set; } = 1.0f;
    public bool IsGameOver { get; private set; } = false;
    public Player Player { get; private set; }
    public UIManager UIManager => this.GetComponent<UIManager>();
    public StageManager StageManager => GetComponent<StageManager>();
    public ScoreManager ScoreManager => GetComponent<ScoreManager>();
    public EnemyContainer EnemyContainer => enemyContainer;
    public Camera UICamera => uiCamera;
    
    public readonly ReactiveProperty<BigInteger> Coin = new(0);
    private int _seed = 42;
    private bool _isPaused = false;
    private bool _isMapOpened = false;
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
    
    public void SubtractCoin(int amount)
    {
        Coin.Value -= amount;
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
        ScoreManager.ShowScore(StageManager.currentStageCount.Value + 1, EnemyContainer.defeatedEnemyCount.Value, Coin.Value);
    }
    
    public void ChangeState(GameState newState)
    {
        state = newState;
        switch (newState)
        {
            case GameState.StageMoving:
                // レベルアップが残っている場合はレベルアップ画面を表示
                if (UIManager.remainingLevelUps > 0)
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
                MergeManager.Instance.Attack();
                break;
            case GameState.EnemyAttack:
                EnemyContainer.Action();
                break;
            case GameState.MapSelect:
                StageManager.SetNextNodeActive();
                UIManager.EnableCanvasGroup("Map", true);
                break;
            case GameState.Event:
                break;
            case GameState.LevelUp:
                UIManager.EnableCanvasGroup("LevelUp", true);
                break;
        }
    }

    public void Start()
    {
        if (PlayerPrefs.GetInt("IsDoubleSpeed", 0) == 1)
        {
            TimeScale = 3.0f;
            Time.timeScale = TimeScale;
        }
        
        AddCoin(Application.isEditor ? 9999 : 10);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_isPaused)
            {
                _isPaused = false;
                UIManager.OnClickResume();
            }
            else
            {
                _isPaused = true;
                UIManager.OnClickPause();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_isMapOpened)
            {
                _isMapOpened = false;
                UIManager.CloseMap();
            }
            else
            {
                _isMapOpened = true;
                UIManager.OpenMap();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            UIManager.OnClickSpeed();
        }
    }
}
