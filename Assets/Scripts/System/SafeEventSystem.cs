using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using R3;

namespace SafeEventSystem
{
    // 修正フェーズの定義（処理順序の制御）
    public enum ModificationPhase
    {
        PreProcess = 0,      // 前処理（条件チェックなど）
        Addition = 100,      // 加算修正 (+5攻撃力)
        Multiplication = 200, // 乗算修正 (x2コイン)
        Conversion = 300,    // 変換修正 (単体→全体攻撃)
        Override = 400,      // 上書き修正 (コイン消費→0)
        PostProcess = 500    // 後処理（状態異常付与など）
    }

    // モディファイアインターフェース
    public interface IModifier<T>
    {
        ModificationPhase Phase { get; }
        int Priority { get; } // 同フェーズ内での順序（低い値ほど優先）
        object Owner { get; }
        bool CanApply(T originalValue, T currentValue);
        T Apply(T originalValue, T currentValue);
        void OnApplied(T originalValue, T resultValue); // 適用後のコールバック
    }

    // 基本モディファイア抽象クラス
    public abstract class ModifierBase<T> : IModifier<T>
    {
        public ModificationPhase Phase { get; protected set; }
        public int Priority { get; protected set; }
        public object Owner { get; protected set; }

        protected ModifierBase(ModificationPhase phase, int priority, object owner)
        {
            Phase = phase;
            Priority = priority;
            Owner = owner;
        }

        public virtual bool CanApply(T originalValue, T currentValue) => true;
        public abstract T Apply(T originalValue, T currentValue);
        public virtual void OnApplied(T originalValue, T resultValue) { }
    }

    // 修正可能なイベントデータ
    public class ModifiableEvent<T>
    {
        private readonly List<IModifier<T>> _modifiers = new();
        private readonly Subject<(T original, T modified)> _onProcessed = new();
        private bool _isProcessing;

        public Observable<(T original, T modified)> OnProcessed => _onProcessed.AsObservable();

        // 修正処理を安全に実行
        public T ProcessModifications(T baseValue)
        {
            if (_isProcessing)
            {
                Debug.LogError($"Recursive modification detected for event type {typeof(T)}");
                return baseValue;
            }

            _isProcessing = true;
            var originalValue = baseValue;
            var currentValue = baseValue;

            try
            {
                // フェーズ順、優先度順でモディファイアを適用
                var sortedModifiers = _modifiers
                    .Where(m => m.CanApply(originalValue, currentValue))
                    .OrderBy(m => (int)m.Phase)
                    .ThenBy(m => m.Priority)
                    .ToList();

                foreach (var modifier in sortedModifiers)
                {
                    #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
                    var beforeValue = currentValue;
                    #endif
                    
                    currentValue = modifier.Apply(originalValue, currentValue);
                    
                    // デバッグログ（エディタでのみ）
                    #if UNITY_EDITOR && DEBUG_SAFE_EVENTS
                    Debug.Log($"[SafeEvent] {modifier.Owner?.GetType().Name}: {beforeValue} → {currentValue}");
                    #endif
                }

                // 適用後のコールバック実行
                foreach (var modifier in sortedModifiers)
                {
                    modifier.OnApplied(originalValue, currentValue);
                }

                _onProcessed.OnNext((originalValue, currentValue));
                return currentValue;
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public void AddModifier(IModifier<T> modifier)
        {
            if (_modifiers.Any(m => m.Owner == modifier.Owner && m.GetType() == modifier.GetType()))
            {
                Debug.LogWarning($"Modifier {modifier.GetType().Name} already exists for owner {modifier.Owner}");
                return;
            }
            _modifiers.Add(modifier);
        }

        public void RemoveModifier(IModifier<T> modifier)
        {
            _modifiers.Remove(modifier);
        }

        public void RemoveModifiersFor(object owner)
        {
            _modifiers.RemoveAll(m => m.Owner == owner);
        }

        public void Clear()
        {
            _modifiers.Clear();
        }

        // デバッグ用：現在のモディファイア一覧
        public List<IModifier<T>> GetModifiers() => new(_modifiers);
    }

    // 特定の型用のモディファイア実装

    // int型用の加算モディファイア
    public class AdditionModifier : ModifierBase<int>
    {
        private readonly int _amount;
        private readonly Func<bool> _condition;

        public AdditionModifier(int amount, object owner, Func<bool> condition = null, int priority = 0)
            : base(ModificationPhase.Addition, priority, owner)
        {
            _amount = amount;
            _condition = condition ?? (() => true);
        }

        public override bool CanApply(int originalValue, int currentValue) => _condition();
        public override int Apply(int originalValue, int currentValue) => currentValue + _amount;
    }

    // int型用の乗算モディファイア
    public class MultiplicationModifier : ModifierBase<int>
    {
        private readonly float _multiplier;
        private readonly Func<bool> _condition;

        public MultiplicationModifier(float multiplier, object owner, Func<bool> condition = null, int priority = 0)
            : base(ModificationPhase.Multiplication, priority, owner)
        {
            _multiplier = multiplier;
            _condition = condition ?? (() => true);
        }

        public override bool CanApply(int originalValue, int currentValue) => _condition();
        public override int Apply(int originalValue, int currentValue) => (int)(currentValue * _multiplier);
    }

    // int型用の上書きモディファイア
    public class OverrideModifier : ModifierBase<int>
    {
        private readonly int _value;
        private readonly Func<bool> _condition;

        public OverrideModifier(int value, object owner, Func<bool> condition = null, int priority = 0)
            : base(ModificationPhase.Override, priority, owner)
        {
            _value = value;
            _condition = condition ?? (() => true);
        }

        public override bool CanApply(int originalValue, int currentValue) => _condition();
        public override int Apply(int originalValue, int currentValue) => _value;
    }

    // Dictionary<AttackType, int>用の攻撃修正モディファイア
    public class AttackModifier : ModifierBase<Dictionary<AttackType, int>>
    {
        private readonly Action<Dictionary<AttackType, int>, Dictionary<AttackType, int>> _modifier;
        private readonly Func<bool> _condition;

        public AttackModifier(
            Action<Dictionary<AttackType, int>, Dictionary<AttackType, int>> modifier,
            ModificationPhase phase,
            object owner,
            Func<bool> condition = null,
            int priority = 0) : base(phase, priority, owner)
        {
            _modifier = modifier;
            _condition = condition ?? (() => true);
        }

        public override bool CanApply(Dictionary<AttackType, int> originalValue, Dictionary<AttackType, int> currentValue)
            => _condition();

        public override Dictionary<AttackType, int> Apply(Dictionary<AttackType, int> originalValue, Dictionary<AttackType, int> currentValue)
        {
            var result = new Dictionary<AttackType, int>(currentValue);
            _modifier(originalValue, result);
            return result;
        }
    }

    // コールバック専用モディファイア（値は変更せずにイベント発生を監視）
    public class CallbackModifier<T> : ModifierBase<T>
    {
        private readonly Action<T, T> _callback;
        private readonly Func<T, T, bool> _condition;

        public CallbackModifier(Action<T, T> callback, object owner, Func<T, T, bool> condition = null, int priority = 0)
            : base(ModificationPhase.PostProcess, priority, owner)
        {
            _callback = callback;
            _condition = condition ?? ((_, _) => true);
        }

        public override bool CanApply(T originalValue, T currentValue) => _condition(originalValue, currentValue);
        public override T Apply(T originalValue, T currentValue) => currentValue; // 値は変更しない
        public override void OnApplied(T originalValue, T resultValue) => _callback(originalValue, resultValue);
    }
}