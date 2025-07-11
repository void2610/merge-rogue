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
    public Dictionary<StatusEffectType, int> StatusEffectStacks { get; } = new();
    public const int MAX_LEVEL = 22;

    private readonly List<int> _levelUpExp = new() { 20, 40, 80, 100, 150, 200, 250, 300, 350, 400, 500, 600, 700, 800, 900, 1000, 1200, 1400, 1600, 1800, 2000 };
    private Material _material;
    public static int RemainingLevelUps = 0;
    
    public StatusEffectUI StatusEffectUI => statusEffectUI;
    
    public void AddStatusEffect(StatusEffectType type, int stacks)
    {
        StatusEffectProcessor.AddStatusEffect(this, type, stacks);
    }
    
    public void RemoveStatusEffect(StatusEffectType type, int stacks)
    {
        StatusEffectProcessor.RemoveStatusEffect(this, type, stacks);
    }
    
    public async UniTask UpdateStatusEffects()
    {
        await StatusEffectProcessor.ProcessTurnEnd(this);
    }
    
    public int ModifyIncomingDamage(int amount)
    {
        return StatusEffectProcessor.ModifyIncomingDamage(this, amount);
    }
    
    public int ModifyOutgoingAttack(AttackType type, int attack)
    {
        return StatusEffectProcessor.ModifyOutgoingAttack(this, type, attack);
    }
    
    public void OnBattleEnd()
    {
        StatusEffectProcessor.OnBattleEnd(this);
    }
    
    public void Damage(AttackType type, int d)
    {
        if(Health.Value <= 0) return;
        
        // 状態異常でダメージを更新
        var damage = ModifyIncomingDamage(d);
        
        // イベントでダメージを更新
        damage = EventManager.OnPlayerDamage.Process(damage);
        
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
        if (Health.Value >= MaxHealth.Value) return;
        
        // イベントでヒール量を更新
        var finalAmount = EventManager.OnPlayerHeal.Process(amount);
        
        ParticleManager.Instance.HealParticleToPlayer();
        SeManager.Instance.PlaySe("heal");
        Health.Value += finalAmount;
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

        // レベルアップは無効
        // if (CheckAndLevelUp())
        //     GameManager.Instance.ChangeState(GameManager.GameState.LevelUp);
        // else
        
        GameManager.Instance.ChangeState(GameManager.GameState.AfterBattle);
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
        RemainingLevelUps++;
        
        //最大レベルを超えたらレベルアップしない
        if (Level >= MAX_LEVEL) return true;
        
        if (Exp.Value >= _levelUpExp[Level - 1])
            CheckAndLevelUp();
        return true;
    }

    private void Start()
    {
        RemainingLevelUps = 0;
        Health.Value = MaxHealth.Value;
        
        var spriteRenderer = this.transform.GetComponentsInChildren<SpriteRenderer>()[0];
        _material = spriteRenderer.material;
    }
}
