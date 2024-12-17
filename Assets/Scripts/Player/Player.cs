using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using R3;

public class Player : MonoBehaviour
{
    public readonly ReactiveProperty<int> exp = new(0);
    public readonly ReactiveProperty<int> health = new(100);
    public readonly ReactiveProperty<int> maxHealth = new(100);
    public int maxExp { get; private set; } = 25;
    public int level { get; private set; } = 1;
    private readonly List<int> levelUpExp = new() { 20, 40, 80, 100, 150, 200, 250, 300, 350, 400, 500 };
    private Material material;

    public void TakeDamage(int d)
    {
        if(health.Value <= 0) return;
        
        EventManager.OnPlayerDamage.Trigger(d);
        var damage = EventManager.OnPlayerDamage.GetValue();
        
        if (health.Value <= 0) return;
        
        SeManager.Instance.PlaySe("enemyAttack");
        CameraMove.Instance.ShakeCamera(0.5f, 0.15f);
        ParticleManager.Instance.DamageText(damage, this.transform.position.x);
        material.DOColor(Color.red, 0).OnComplete(() =>
        {
            material.DOColor(new Color(0.7f,0.7f,0.7f), 0.3f);
        });
        
        health.Value -= damage;
        if (health.Value <= 0)
        {
            health.Value = 0;
            GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        if(health.Value <= 0) return;
        
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
    
    public void HealToFull()
    {
        if(health.Value <= 0) return;
        
        ParticleManager.Instance.HealParticleToPlayer();
        SeManager.Instance.PlaySe("heal");

        health.Value = maxHealth.Value;
    }

    public void AddExp(int amount)
    {
        exp.Value += amount;
        if(!CheckAndLevelUp()) GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
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
        
        var spriteRenderer = this.transform.GetComponentsInChildren<SpriteRenderer>()[0];
        material = spriteRenderer.material;
    }
}
