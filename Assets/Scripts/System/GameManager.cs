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

    public System.Random random { get; private set; }
    private int seed = 42;
    private bool isPaused = false;
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

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        uiManager.EnableGameOver(true);
    }

    public void NextStage()
    {
        ChangeState(GameState.StageMoving);
        stageManager.NextStage();
    }

    public void ChangeState(GameState newState)
    {
        switch (state)
        {
            case GameState.Shop:
                this.GetComponent<InventoryUI>().EnableCursor(false);
                Time.timeScale = 1;
                break;
        }
        state = newState;
        Debug.Log("State: " + state);
        switch (newState)
        {
            case GameState.BattlePreparation:
                Utils.instance.WaitAndInvoke(1.0f, () =>
                {
                    ChangeState(GameState.Battle);
                });
                break;
            case GameState.Shop:
                Shop.instance.SetItem();
                this.GetComponent<InventoryUI>().EnableCursor(true);
                Time.timeScale = 0;
                uiManager.EnableShopOptions(true);
                break;
        }
    }

    void Start()
    {
        NextStage();
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
            case GameState.Battle:
                break;
            case GameState.StageMoving:
                break;
        }
    }
}
