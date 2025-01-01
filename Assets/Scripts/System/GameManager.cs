using System;
using UnityEngine;
using R3;
using DG.Tweening;

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

            player = playerObj.GetComponent<Player>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public enum GameState
    {
        BattlePreparation,
        Merge,
        PlayerAttack,
        EnemyAttack,
        BattleResult,
        LevelUp,
        MapSelect,
        StageMoving,
        Event,
        GameOver,
        Clear,
        Other
    }
    public GameState state = GameState.Merge;
    
    [Header("オブジェクト")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] public EnemyContainer enemyContainer;
    [SerializeField] public Camera renderTextureCamera;
    [SerializeField] public Camera uiCamera;
    [SerializeField] public Canvas pixelCanvas;
    [SerializeField] public Canvas uiCanvas;

    private System.Random _random;
    public float TimeScale { get; private set; } = 1.0f;
    public bool IsGameOver { get; private set; } = false;
    public readonly ReactiveProperty<int> coin = new(0);
    private int _seed = 42;
    private bool _isPaused = false;
    private bool _isMapOpened = false;
    public Player player;
    public UIManager UIManager => this.GetComponent<UIManager>();
    public StageManager StageManager => GetComponent<StageManager>();
    public ScoreManager ScoreManager => GetComponent<ScoreManager>();

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
        coin.Value += c;
    }
    
    public void SubtractCoin(int amount)
    {
        coin.Value -= amount;
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
        ScoreManager.ShowScore(StageManager.currentStageCount.Value + 1, enemyContainer.defeatedEnemyCount.Value, coin.Value);
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
            case GameState.BattlePreparation:
                // バトル準備
                Utils.Instance.WaitAndInvoke(1.0f, () =>
                {
                    ChangeState(GameState.Merge);
                });
                break;
            case GameState.Merge:
                Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
                MergeManager.Instance.StartMerge();
                break;
            case GameState.PlayerAttack:
                Physics2D.simulationMode = SimulationMode2D.Script;
                Utils.Instance.WaitAndInvoke(1f, () =>
                {
                    MergeManager.Instance.Attack();
                });
                break;
            case GameState.EnemyAttack:
                enemyContainer.Action();
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
