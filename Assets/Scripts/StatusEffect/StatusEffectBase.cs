using System;
using UnityEngine;

public abstract class StatusEffectBase
{
    public enum EffectTiming
    {
        OnTurnEnd,
        OnBattleEnd,
        OnAttack,
        OnDamage,
        Random,
    }
    
    public StatusEffectData Data { get; private set; }
    public StatusEffectType Type => Data.type;
    public int StackCount { get; protected set; }
    protected IEntity Owner { get; private set; }
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(StatusEffectData data, int initialStack)
    {
        Data = data;
        StackCount = initialStack;
    }
    
    /// <summary>
    /// 所有者を設定
    /// </summary>
    public void SetOwner(IEntity owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// スタック数を追加
    /// </summary>
    public void AddStack(int count)
    {
        StackCount += count;
    }

    /// <summary>
    /// ターン終了時にスタック数を1減らす
    /// </summary>
    /// <returns>スタック数が0になったらtrue</returns>
    public bool ReduceStack()
    {
        if (Data.isPermanent && StackCount > 0) return false;
        StackCount--;
        return StackCount <= 0;
    }
    
    /// <summary>
    /// その他の要因で指定したスタック数を減らす
    /// </summary>
    /// <returns>スタック数が0になったらtrue</returns>
    public bool ReduceStack(int count)
    {
        StackCount -= count;
        return StackCount <= 0;
    }
    
    /// <summary>
    /// ターン終了時の処理
    /// </summary>
    public void OnTurnEnd(IEntity target)
    {
        if (Data.timing == EffectTiming.OnTurnEnd)
        {
            ShowEffectText();
            OnTurnEndEffect();
        }
    }
    
    /// <summary>
    /// ダメージ受ける時の処理
    /// </summary>
    public int ModifyDamage(IEntity target, int incomingDamage)
    {
        if (Data.timing == EffectTiming.OnDamage)
        {
            ShowEffectText(1);
            return ModifyDamageEffect(incomingDamage);
        }
        return incomingDamage;
    }
    
    /// <summary>
    /// 攻撃時の処理
    /// </summary>
    public int ModifyAttack(IEntity target, AttackType type, int outgoingAttack)
    {
        if (Data.timing == EffectTiming.OnAttack)
        {
            ShowEffectText();
            return ModifyAttackEffect(type, outgoingAttack);
        }
        return outgoingAttack;
    }
    
    /// <summary>
    /// 戦闘終了時の処理
    /// </summary>
    public virtual bool OnBattleEnd(IEntity target)
    {
        if (Data.timing == EffectTiming.OnBattleEnd)
        {
            ShowEffectText();
            OnBattleEndEffect();
        }
        StackCount = 0;
        return true; 
    }
    
    // 派生クラスで実装する効果メソッド
    protected virtual void OnTurnEndEffect() { }
    protected virtual int ModifyDamageEffect(int damage) => damage;
    protected virtual int ModifyAttackEffect(AttackType type, int attack) => attack;
    protected virtual void OnBattleEndEffect() { }
    
    /// <summary>
    /// エフェクトテキストを表示
    /// </summary>
    protected void ShowEffectText(int priority = 0)
    {
        var position = Owner is Player player 
            ? player.transform.position 
            : (Owner as EnemyBase)?.transform.position ?? Vector3.zero;
            
        var isPlayer = Owner is Player;
        StatusEffectManager.Instance.ShowEffectText(Type, position, isPlayer, priority);
    }
    
    /// <summary>
    /// サウンドエフェクトを再生
    /// </summary>
    protected void PlaySoundEffect()
    {
        if (!string.IsNullOrEmpty(Data.soundEffectName))
        {
            SeManager.Instance.PlaySe(Data.soundEffectName);
        }
    }
}