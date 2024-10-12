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
                Debug.Log("Random");
            }
            else
            {
                seed = PlayerPrefs.GetInt("Seed", seed);
                Debug.Log("Seed: " + seed);
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
        StageMoving,
        Shop,
        GameOver,
        Clear,
        Other
    }
    public GameState state = GameState.Merge;
    
    
    [SerializeField] private GameObject playerObj;
    [SerializeField] public EnemyContainer enemyContainer;
    [SerializeField] public Shop shop;
    [SerializeField] public Canvas mainCanvas;
    [SerializeField] private GameObject damageTextPrefab;

    private System.Random random { get; set; }
    public readonly ReactiveProperty<int> coin = new(0);
    private int seed = 42;
    private bool isPaused;
    public Player player => playerObj.GetComponent<Player>();
    public UIManager uiManager => this.GetComponent<UIManager>();
    public StageManager stageManager => GetComponent<StageManager>();

    public float RandomRange(float min, float max)
    {
        float randomValue = (float)(this.random.NextDouble() * (max - min) + min);
        return randomValue;
    }

    public int RandomRange(int min, int max)
    {
        int randomValue = this.random.Next(min, max);
        return randomValue;
    }

    public void ShowDamage(int damage, Vector3 pos)
    {
        var damageText = Instantiate(damageTextPrefab, pos, Quaternion.identity, mainCanvas.transform);
        damageText.GetComponent<DamageText>().SetUp(damage);
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        uiManager.EnableCanvasGroup("GameOver", true);
    }

    public void ChangeState(GameState newState)
    {
        switch (state)
        {
            case GameState.StageMoving:
                Time.timeScale = 1;
                break;
            case GameState.Shop:
                Shop.Instance.CloseShop();
                Time.timeScale = 1;
                break;
        }
        state = newState;
        Debug.Log("State: " + state);
        switch (newState)
        {
            case GameState.StageMoving:
                Time.timeScale = 0;
                // レベルアップが残っている場合はレベルアップ画面を表示
                if (uiManager.remainingLevelUps > 0)
                     ChangeState(GameState .LevelUp);
                else
                    stageManager.NextStage();
                break;
            case GameState.BattlePreparation:
                // バトル準備
                Utils.Instance.WaitAndInvoke(1.0f, () =>
                {
                    ChangeState(GameState.Merge);
                });
                break;
            case GameState.Merge:
                MergeManager.Instance.ResetRemainingBalls();
                break;
            case GameState.PlayerAttack:
                // 攻撃して、敵の攻撃に移る
                Utils.Instance.WaitAndInvoke(1f, () =>
                {
                    MergeManager.Instance.Attack();
                });
                break;
            case GameState.EnemyAttack:
                enemyContainer.Action();
                break;
            case GameState.Shop:
                Shop.Instance.OpenShop();
                Time.timeScale = 0;
                uiManager.EnableCanvasGroup("Shop", true);
                break;
            case GameState.LevelUp:
                Time.timeScale = 0;
                // レベルアップ後にtimescaleを戻さない
                uiManager.EnableCanvasGroup("LevelUp", true);
                break;
        }
    }

    public void Start()
    {
        if (Application.isEditor) coin.Value += 1000;
        ChangeState(GameState.StageMoving);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
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
