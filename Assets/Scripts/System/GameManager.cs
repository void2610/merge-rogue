using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using unityroom.Api;


public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
        PlayerTurn,
        PlayerAttack,
        EnemyAttack,
        LevelUp,
        StageMoving,
        Shop,
        GameOver,
        Clear,
        Other
    }
    public GameState state = GameState.PlayerTurn;


    [SerializeField]
    private GameObject playerObj;
    [SerializeField]
    public PlayerAnimation playerAnimation;
    [SerializeField]
    public EnemyContainer enemyContainer;
    [SerializeField]
    public Shop shop;

    public System.Random random { get; private set; }
    private int seed = 42;
    private bool isPaused = false;
    public int turnCount = 0;
    public Player player => playerObj.GetComponent<Player>();
    public UIManager uiManager => GetComponent<UIManager>();
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

    public void ChangeState(GameState newState)
    {
        switch (state)
        {
            case GameState.PlayerTurn:
                uiManager.EnablePlayerActions(false);
                break;
            case GameState.PlayerAttack:
                break;
            case GameState.EnemyAttack:
                player.mirror.NormalPos();
                break;
            case GameState.LevelUp:
                uiManager.EnableLevelUpOptions(false);
                break;
            case GameState.Shop:
                shop.ResetItem();
                break;
            case GameState.GameOver:
                break;
            case GameState.Other:
                break;
        }
        switch (newState)
        {
            // 敵を倒したら次のステージへ
            case GameState.EnemyAttack:
                if (enemyContainer.GetEnemyCount() == 0)
                {
                    //ボスならクリア
                    if (stageManager.GetCurrentStageType() == StageManager.StageType.boss)
                    {
                        newState = GameState.Clear;
                    }
                    else
                    {
                        newState = GameState.StageMoving;
                    }
                }
                break;
            // HPが0になったらゲームオーバー
            case GameState.PlayerTurn:
                if (player.health <= 0)
                {
                    newState = GameState.GameOver;
                }
                break;
        }
        state = newState;
        Debug.Log("State: " + state);
        switch (newState)
        {
            case GameState.PlayerTurn:
                turnCount++;
                uiManager.UpdateTurnText(turnCount);
                uiManager.EnablePlayerActions(true);
                player.EnableSave(false);
                playerAnimation.ChangeAnimation("stand");
                break;
            case GameState.PlayerAttack:
                break;
            case GameState.EnemyAttack:
                playerAnimation.ChangeAnimation("stand");
                enemyContainer.AttackPlayer(player);
                break;
            case GameState.LevelUp:
                playerAnimation.ChangeAnimation("stand");
                uiManager.EnableLevelUpOptions(true);
                break;
            case GameState.StageMoving:
                stageManager.NextStage();
                break;
            case GameState.Shop:
                playerAnimation.ChangeAnimation("stand");
                shop.SetItem(3);
                uiManager.EnableShopOptions(true);
                break;
            case GameState.GameOver:
                playerAnimation.ChangeAnimation("stand");
                uiManager.EnableGameOver(true);
                break;
            case GameState.Clear:
                playerAnimation.ChangeAnimation("stand");
                if(PlayerPrefs.GetString("SeedText", "") == "")
                    UnityroomApiClient.Instance.SendScore(1, turnCount, ScoreboardWriteMode.HighScoreAsc);
                uiManager.EnableClear(true);
                break;
            case GameState.Other:
                playerAnimation.ChangeAnimation("stand");
                break;
        }
    }

    void Start()
    {
        ChangeState(GameState.StageMoving);
    }

    void Update()
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
            case GameState.PlayerTurn:
                break;
            case GameState.PlayerAttack:
                break;
            case GameState.EnemyAttack:
                break;
            case GameState.LevelUp:
                break;
            case GameState.Shop:
                break;
            case GameState.GameOver:
                break;
            case GameState.Other:
                break;
        }
    }
}
