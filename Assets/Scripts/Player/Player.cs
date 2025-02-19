using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine.Serialization;

public class Player : MonoBehaviour, IEntity
{
    [SerializeField] private StatusEffectUI statusEffectUI;
    public readonly ReactiveProperty<int> Exp = new(0);
    public readonly ReactiveProperty<int> Health = new(100);
    public readonly ReactiveProperty<int> MaxHealth = new(100);
    public int MaxExp { get; private set; } = 20;
    public int Level { get; set; } = 1;
    public List<StatusEffectBase> StatusEffects { get; } = new();
    public const int MAX_LEVEL = 22;

    private readonly List<int> _levelUpExp = new() { 20, 40, 80, 100, 150, 200, 250, 300, 350, 400, 500, 600, 700, 800, 900, 1000, 1200, 1400, 1600, 1800, 2000 };
    private Material _material;
    
    public void AddStatusEffect(StatusEffectBase effect)
    {
        var existingEffect = StatusEffects.Find(e => e.Type == effect.Type);
        if (existingEffect != null)
            existingEffect.AddStack(effect.StackCount);
        else
        {
            effect.SetEntityPosition(this.transform.position, true);
            StatusEffects.Add(effect);
        }
        statusEffectUI.UpdateUI(StatusEffects);
    }
    
    public async UniTask UpdateStatusEffects()
    {
        for (var i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffects[i].OnTurnEnd(this);
            if (StatusEffects[i].ReduceStack()) StatusEffects.RemoveAt(i);
            await UniTask.Delay((int)(500 * GameManager.Instance.TimeScale));
        }
        statusEffectUI.UpdateUI(StatusEffects);
    }
    
    public int ModifyIncomingDamage(int amount)
    {
        var v = StatusEffects.Aggregate(amount, (current, effect) => effect.ModifyDamage(this, current));
        statusEffectUI.UpdateUI(StatusEffects);
        return v;
    }
    
    public int ModifyOutgoingAttack(int amount)
    {
        var v = StatusEffects.Aggregate(amount, (current, effect) => effect.ModifyAttack(this, current));
        statusEffectUI.UpdateUI(StatusEffects);
        return v;
    }
    
    public void OnBattleEnd()
    {
        for (var i = StatusEffects.Count - 1; i >= 0; i--)
        {
            if (StatusEffects[i].OnBattleEnd(this)) StatusEffects.RemoveAt(i);
        }
        statusEffectUI.UpdateUI(StatusEffects);
    }
    
    public void Damage(int d)
    {
        if(Health.Value <= 0) return;
        
        // 状態異常でダメージを更新
        var damage = ModifyIncomingDamage(d);
        
        // イベントでダメージを更新
        EventManager.OnPlayerDamage.Trigger(damage);
        damage = EventManager.OnPlayerDamage.GetValue();
        
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

        if (CheckAndLevelUp())
        {
            SeManager.Instance.PlaySe("levelUp");
            GameManager.Instance.ChangeState(GameManager.GameState.LevelUp);
        }
        else
        {        
            GameManager.Instance.ChangeState(GameManager.GameState.AfterBattle);
        }
        Exp.ForceNotify();
    }

    public bool CanLevelUp() => Level <= MAX_LEVEL;
    
    private bool CheckAndLevelUp()
    {
        if(Level >= MAX_LEVEL) return false;
        if (Exp.Value < _levelUpExp[Level - 1]) return false;

        Exp.Value -= _levelUpExp[Level - 1];
        Level++;
        MaxExp = _levelUpExp[Level - 1];
        UIManager.Instance.remainingLevelUps++;
        
        //最大レベルを超えたらレベルアップしない
        if (Level >= MAX_LEVEL) return true;
        
        if (Exp.Value >= _levelUpExp[Level - 1])
            CheckAndLevelUp();
        return true;
    }

    private void Start()
    {
        Health.Value = MaxHealth.Value;
        
        var spriteRenderer = this.transform.GetComponentsInChildren<SpriteRenderer>()[0];
        _material = spriteRenderer.material;
    }
}
