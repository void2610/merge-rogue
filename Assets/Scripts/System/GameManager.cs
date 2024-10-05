using System;
using UnityEngine;


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
            DG.Tweening.DOTween.SetTweensCapacity(tweenersCapacity: 200, sequencesCapacity: 200);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public enum GameState
    {
        BattlePreparation,
        Battle,
        BattleResult,
        StageMoving,
        Shop,
        GameOver,
        Clear,
        Other
    }
    public GameState state = GameState.Battle;


    [SerializeField]
    private GameObject playerObj;
    [SerializeField]
    public EnemyContainer enemyContainer;
    [SerializeField]
    public Shop shop;
    [SerializeField]
    public Canvas mainCanvas;

    private System.Random random { get; set; }
    public int coin { get; private set; }
    private int seed = 42;
    private bool isPaused;
    public Player player => playerObj.GetComponent<Player>();
    public UIManager uiManager => this.GetComponent<UIManager>();
    public StageManager stageManager => GetComponent<StageManager>();

    public void AddCoin(int value)
    {
        coin += value;
        //uiManager.UpdateCoinText(coin);
    }

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

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        uiManager.EnableGameOver(true);
    }

    public void ChangeState(GameState newState)
    {
        switch (state)
        {
            case GameState.Shop:
                Shop.instance.CloseShop();
                this.GetComponent<InventoryUI>().EnableCursor(false);
                Time.timeScale = 1;
                break;
        }
        state = newState;
        Debug.Log("State: " + state);
        switch (newState)
        {
            case GameState.StageMoving:
                stageManager.NextStage();
                break;
            case GameState.BattlePreparation:
                Utils.Instance.WaitAndInvoke(1.0f, () =>
                {
                    ChangeState(GameState.Battle);
                });
                break;
            case GameState.Shop:
                Shop.instance.OpenShop();
                Time.timeScale = 0;
                uiManager.EnableShopOptions(true);
                break;
        }
    }

    public void Start()
    {
        if (Application.isEditor) coin = 1000;
        ChangeState(GameState.StageMoving);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                isPaused = false;
                uiManager.EnablePauseMenu(false);
            }
            else
            {
                isPaused = true;
                uiManager.EnablePauseMenu(true);
            }
        }
        switch (state)
        {
            case GameState.Battle:
                break;
            case GameState.StageMoving:
                break;
        }
    }
}
