using UnityEngine;
using System.Linq;

/// <summary>
/// 新しい安全なイベントシステムへの移行ガイド
/// 既存のレリックを新システムに移行する方法を示す
/// </summary>
public static class SafeEventMigrationGuide
{
    /// <summary>
    /// 既存のレリッククラスを新しいシステムに移行する手順：
    /// 
    /// 1. RelicBaseをSafeRelicBaseに変更
    /// 2. SubscribeEffect()をRegisterEffects()に変更
    /// 3. EventManager呼び出しをヘルパーメソッドに置き換え
    /// 4. テストとデバッグ
    /// 
    /// 例：PocketMoneyの移行
    /// 
    /// === 旧実装 ===
    /// public class PocketMoney : RelicBase
    /// {
    ///     protected override void SubscribeEffect()
    ///     {
    ///         var disposable = EventManager.OnShopEnter.Subscribe(EffectImpl).AddTo(this);
    ///         Disposables.Add(disposable);
    ///     }
    ///     
    ///     protected override void EffectImpl(Unit _)
    ///     {
    ///         GameManager.Instance.AddCoin(10);
    ///         UI?.ActivateUI();
    ///     }
    /// }
    /// 
    /// === 新実装 ===
    /// public class SafePocketMoney : SafeRelicBase
    /// {
    ///     protected override void RegisterEffects()
    ///     {
    ///         SubscribeShopEnter(() =>
    ///         {
    ///             GameManager.Instance.AddCoin(10);
    ///             ActivateUI();
    ///         });
    ///     }
    /// }
    /// 
    /// === 移行のメリット ===
    /// ✅ 競合状態の解決
    /// ✅ 処理順序の保証
    /// ✅ デバッグの容易さ
    /// ✅ より簡潔なコード
    /// ✅ 型安全性の向上
    /// 
    /// === 移行時の注意点 ===
    /// ⚠️ RelicDataのclassNameは既存のまま使用可能
    /// ⚠️ 段階的移行により既存レリックと共存可能
    /// ⚠️ SafeEventDebuggerでモディファイアの動作を監視
    /// </summary>
    
    /// <summary>
    /// コイン修正レリックの移行例
    /// </summary>
    public static void CoinModificationExample()
    {
        /* 
        === 旧実装（DoubleCoinsWhenNearFullHealth） ===
        protected override void EffectImpl(Unit _)
        {
            var health = GameManager.Instance.Player.Health.Value;
            var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
            if(health > maxHealth * 0.8f)
            {
                var x = EventManager.OnCoinGain.GetValue();
                EventManager.OnCoinGain.SetValue(x * 2);  // 競合状態の危険！
                UI?.ActivateUI();
            }
        }
        
        === 新実装 ===
        protected override void RegisterEffects()
        {
            RegisterCoinGainMultiplier(
                multiplier: 2.0f,
                condition: PlayerHealthConditionAbove(0.8f) // HP > 80%
            );
        }
        */
    }
    
    /// <summary>
    /// 攻撃修正レリックの移行例
    /// </summary>
    public static void AttackModificationExample()
    {
        /*
        === 旧実装（AllAttackWhenWeakAttack） ===
        protected override void EffectImpl(Unit _)
        {
            var dic = EventManager.OnPlayerAttack.GetValue();
            if (dic[AttackType.Normal] <= 30)
            {
                // 全体攻撃に変換
                dic[AttackType.All] += dic[AttackType.Normal];
                dic[AttackType.Normal] = 0;
                EventManager.OnPlayerAttack.SetValue(dic);  // 競合状態の危険！
                UI?.ActivateUI();
            }
        }
        
        === 新実装 ===
        protected override void RegisterEffects()
        {
            RegisterPlayerAttackModifier(
                SafeEventSystem.ModificationPhase.Conversion,
                (original, current) =>
                {
                    if (current.ContainsKey(AttackType.Normal) && 
                        current[AttackType.Normal] > 0 && 
                        current[AttackType.Normal] <= 30)
                    {
                        if (!current.ContainsKey(AttackType.All)) current[AttackType.All] = 0;
                        current[AttackType.All] += current[AttackType.Normal];
                        current[AttackType.Normal] = 0;
                        ActivateUI();
                    }
                }
            );
        }
        */
    }
    
    /// <summary>
    /// ダメージ蓄積レリックの移行例
    /// </summary>
    public static void DamageAccumulationExample()
    {
        /*
        === 旧実装（ReverseAlchemy） ===
        protected override void EffectImpl(Unit _)    
        {
            var x = EventManager.OnPlayerDamage.GetValue();
            Count.Value += x;

            var isActivated = false;
            while (Count.Value >= 5)
            {
                Count.Value -= 5;
                GameManager.Instance.AddCoin(1);
                isActivated = true;
            }
            
            if (isActivated) UI?.ActivateUI();
        }
        
        === 新実装 ===
        protected override void RegisterEffects()
        {
            RegisterDamageAccumulator(
                threshold: 5,
                onThresholdReached: () => GameManager.Instance.AddCoin(1)
            );
        }
        */
    }
    
    /// <summary>
    /// 条件発動レリックの移行例
    /// </summary>
    public static void ConditionalTriggerExample()
    {
        /*
        === 旧実装（DoubleAttackWhenLowHealth） ===
        protected override void EffectImpl(Unit _)
        {
            if (GameManager.Instance.Player.Health.Value <= GameManager.Instance.Player.MaxHealth.Value * 0.2f　|| GameManager.Instance.Player.Health.Value <= 20)
            {
                StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Rage, 10);
                UI?.ActivateUI();
            }
        }
        
        === 新実装 ===
        protected override void RegisterEffects()
        {
            SubscribeBattleStart(() =>
            {
                if (IsLowHealth())
                {
                    StatusEffectFactory.AddStatusEffectToPlayer(StatusEffectType.Rage, 10);
                    ActivateUI();
                }
            });
        }
        
        private bool IsLowHealth()
        {
            if (!GameManager.Instance?.Player) return false;
            
            var currentHealth = GameManager.Instance.Player.Health.Value;
            var maxHealth = GameManager.Instance.Player.MaxHealth.Value;
            
            return currentHealth <= maxHealth * 0.2f || currentHealth <= 20;
        }
        */
    }
}

/// <summary>
/// デバッグ時に使用する便利なテストメソッド
/// </summary>
public static class SafeEventTestHelpers
{
    /// <summary>
    /// コイン獲得のテスト（複数のレリックの組み合わせをテスト）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void TestCoinGainWithMultipleModifiers()
    {
        if (!Application.isPlaying) return;
        
        Debug.Log("=== Testing Coin Gain with Multiple Modifiers ===");
        
        // 100コイン獲得をテスト
        var originalCoin = GameManager.Instance.Coin.Value;
        GameManager.Instance.AddCoin(100);
        var newCoin = GameManager.Instance.Coin.Value;
        var actualGain = newCoin - originalCoin;
        
        Debug.Log($"Base: 100, Actual: {actualGain}, Multiplier: {(float)actualGain / 100.0f:F2}x");
        
        // モディファイアの詳細を表示
        var modifiers = SafeEventManager.OnCoinGain.GetModifiers();
        Debug.Log($"Active coin gain modifiers: {modifiers.Count}");
        foreach (var modifier in modifiers)
        {
            Debug.Log($"- {modifier.Owner?.GetType().Name}: {modifier.Phase} (Priority: {modifier.Priority})");
        }
    }
    
    /// <summary>
    /// 攻撃修正のテスト
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void TestAttackModification()
    {
        if (!Application.isPlaying) return;
        
        Debug.Log("=== Testing Attack Modification ===");
        
        var testAttack = new System.Collections.Generic.Dictionary<AttackType, int>
        {
            { AttackType.Normal, 50 }
        };
        
        var modifiedAttack = SafeEventManager.TriggerPlayerAttack(testAttack);
        
        Debug.Log($"Original Attack: {string.Join(", ", testAttack.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
        Debug.Log($"Modified Attack: {string.Join(", ", modifiedAttack.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
        
        // モディファイアの詳細を表示
        var modifiers = SafeEventManager.OnPlayerAttack.GetModifiers();
        Debug.Log($"Active attack modifiers: {modifiers.Count}");
        foreach (var modifier in modifiers)
        {
            Debug.Log($"- {modifier.Owner?.GetType().Name}: {modifier.Phase} (Priority: {modifier.Priority})");
        }
    }
}