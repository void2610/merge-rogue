using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using R3;

public class Player : MonoBehaviour
{
    public readonly ReactiveProperty<int> exp = new(0);
    public readonly ReactiveProperty<int> health = new(50);
    public readonly ReactiveProperty<int> maxHealth = new(50);
    public List<int> levelUpExp = new() { 10, 20, 40, 80, 160, 320, 640, 1280, 2560, 5120 };
    public int maxExp { get; private set; } = 10;
    public int level { get; private set; } = 1;

    public void TakeDamage(int damage)
    {
        Camera.main?.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.2f);
        ParticleManager.Instance.DamageText(damage, this.transform.position.x);
        health.Value -= damage;
        if (health.Value <= 0)
        {
            health.Value = 0;
            GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        ParticleManager.Instance.HealParticleToPlayer();
        SeManager.Instance.PlaySe("heal");

        if (health.Value >= maxHealth.Value)
        {
            return;
        }

        health.Value += amount;
        if (health.Value > maxHealth.Value)
        {
            health.Value = maxHealth.Value;
        }
    }

    public void HealFromItem(int amount)
    {
        health.Value += amount;
        if (health.Value > maxHealth.Value + 5)
        {
            health.Value = maxHealth.Value + 5;
        }
    }

    public void AddExp(int amount)
    {
        exp.Value += amount;
        if(!CheckAndLevelUp()) GameManager.Instance.ChangeState(GameManager.GameState.StageMoving);
    }
    
    private bool CheckAndLevelUp()
    {
        if (exp.Value < levelUpExp[level - 1])
        {
            return false;
        }

        exp.Value -= levelUpExp[level - 1];
        maxExp = levelUpExp[level];
        level++;
        SeManager.Instance.PlaySe("levelUp");
        GameManager.Instance.uiManager.remainingLevelUps++;
        GameManager.Instance.uiManager.EnableCanvasGroup("LevelUp", true);
        GameManager.Instance.ChangeState(GameManager.GameState.LevelUp);

        if (exp.Value >= levelUpExp[level - 1])
        {
            CheckAndLevelUp();
        }
        exp.ForceNotify();
        return true;
    }

    private void Start()
    {
        health.Value = maxHealth.Value;
    }
}
