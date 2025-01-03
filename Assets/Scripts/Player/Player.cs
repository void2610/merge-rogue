using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using R3;

public class Player : MonoBehaviour
{
    public readonly ReactiveProperty<int> Exp = new(0);
    public readonly ReactiveProperty<int> Health = new(100);
    public readonly ReactiveProperty<int> MaxHealth = new(100);
    public int MaxExp { get; private set; } = 25;
    public int Level { get; private set; } = 1;
    private readonly List<int> _levelUpExp = new() { 20, 40, 80, 100, 150, 200, 250, 300, 350, 400, 500 };
    private Material _material;

    public void TakeDamage(int d)
    {
        if(Health.Value <= 0) return;
        
        EventManager.OnPlayerDamage.Trigger(d);
        var damage = EventManager.OnPlayerDamage.GetValue();
        
        if (Health.Value <= 0) return;
        
        SeManager.Instance.PlaySe("enemyAttack");
        CameraMove.Instance.ShakeCamera(0.5f, 0.15f);
        ParticleManager.Instance.DamageText(damage, this.transform.position.x);
        _material.DOColor(Color.red, 0).OnComplete(() =>
        {
            _material.DOColor(new Color(0.7f,0.7f,0.7f), 0.3f);
        });
        
        Health.Value -= damage;
        if (Health.Value <= 0)
        {
            Health.Value = 0;
            GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        if(Health.Value <= 0) return;
        
        ParticleManager.Instance.HealParticleToPlayer();
        SeManager.Instance.PlaySe("heal");

        if (Health.Value >= MaxHealth.Value)
        {
            return;
        }

        Health.Value += amount;
        if (Health.Value > MaxHealth.Value)
        {
            Health.Value = MaxHealth.Value;
        }
    }
    
    public void HealToFull()
    {
        if(Health.Value <= 0) return;
        
        ParticleManager.Instance.HealParticleToPlayer();
        SeManager.Instance.PlaySe("heal");

        Health.Value = MaxHealth.Value;
    }

    public void AddExp(int amount)
    {
        Exp.Value += amount;
        if(!CheckAndLevelUp()) GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    private bool CheckAndLevelUp()
    {
        if (Exp.Value < _levelUpExp[Level - 1])
        {
            return false;
        }

        Exp.Value -= _levelUpExp[Level - 1];
        MaxExp = _levelUpExp[Level];
        Level++;
        SeManager.Instance.PlaySe("levelUp");
        GameManager.Instance.UIManager.remainingLevelUps++;
        GameManager.Instance.UIManager.EnableCanvasGroup("LevelUp", true);
        GameManager.Instance.ChangeState(GameManager.GameState.LevelUp);

        if (Exp.Value >= _levelUpExp[Level - 1])
        {
            CheckAndLevelUp();
        }
        Exp.ForceNotify();
        return true;
    }

    private void Start()
    {
        Health.Value = MaxHealth.Value;
        
        var spriteRenderer = this.transform.GetComponentsInChildren<SpriteRenderer>()[0];
        _material = spriteRenderer.material;
    }
}
