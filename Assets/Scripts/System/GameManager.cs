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
                seed = (int)DateTime.Now.Ticks;
                Debug.Log("random seed: " + seed);
            }
            else
            {
                seed = PlayerPrefs.GetInt("Seed", seed);
                Debug.Log("fixed seed: " + seed);
            }
            random = new System.Random(seed);
            DOTween.SetTweensCapacity(tweenersCapacity: 800, sequencesCapacity: 800);
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
    
    [Header("デバッグ")]
    [SerializeField] private int debugCoin = 10;
    
    [Header("オブジェクト")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] public EnemyContainer enemyContainer;
    [SerializeField] public Shop shop;
    [SerializeField] public Canvas pixelCanvas;
    [SerializeField] public Canvas uiCanvas;

    private System.Random random { get; set; }
    public readonly ReactiveProperty<int> coin = new(0);
    private int seed = 42;
    private bool isPaused;
    public Player player => playerObj.GetComponent<Player>();
    public UIManager uiManager => this.GetComponent<UIManager>();
    public StageManager stageManager => GetComponent<StageManager>();

    public float RandomRange(float min, float max)
    {
        var randomValue = (float)(this.random.NextDouble() * (max - min) + min);
        return randomValue;
    }

    public int RandomRange(int min, int max)
    {
        var randomValue = this.random.Next(min, max);
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

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        uiManager.EnableCanvasGroup("GameOver", true);
    }

    public void ChangeState(GameState newState)
    {
        // switch (state)
        // {
        // }
        state = newState;
        Debug.Log("State: " + state);
        switch (newState)
        {
            case GameState.StageMoving:
                // レベルアップが残っている場合はレベルアップ画面を表示
                if (uiManager.remainingLevelUps > 0)
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
                MergeManager.Instance.ResetRemainingBalls();
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
                uiManager.EnableCanvasGroup("Map", true);
                break;
            case GameState.Event:
                break;
            case GameState.LevelUp:
                uiManager.EnableCanvasGroup("LevelUp", true);
                break;
        }
    }

    public void Start()
    {
        ChangeState(GameState.StageMoving);

        AddCoin(Application.isEditor ? debugCoin : 10);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("pause");
            if (isPaused)
            {
                isPaused = false;
                uiManager.EnableCanvasGroup("Pause", false);
            }
            else
            {
                isPaused = true;
                uiManager.EnableCanvasGroup("Pause", true);
            }
        }
    }
}
