using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using R3;

namespace SafeEventSystem
{
    // モディファイアインターフェース（シンプル版）
    public interface IModifier<T>
    {
        object Owner { get; }
        bool CanApply(T originalValue, T currentValue);
        T Apply(T originalValue, T currentValue);
        void OnApplied(T originalValue, T resultValue); // 適用後のコールバック
    }

    // 基本モディファイア抽象クラス（シンプル版）
    public abstract class ModifierBase<T> : IModifier<T>
    {
        public object Owner { get; protected set; }

        protected ModifierBase(object owner)
        {
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

        // 修正処理を安全に実行（追加順での処理）
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
                // 追加順でモディファイアを適用
                var applicableModifiers = _modifiers
                    .Where(m => m.CanApply(originalValue, currentValue))
                    .ToList();

                foreach (var modifier in applicableModifiers)
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
                foreach (var modifier in applicableModifiers)
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

        public AdditionModifier(int amount, object owner, Func<bool> condition = null)
            : base(owner)
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

        public MultiplicationModifier(float multiplier, object owner, Func<bool> condition = null)
            : base(owner)
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

        public OverrideModifier(int value, object owner, Func<bool> condition = null)
            : base(owner)
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
            object owner,
            Func<bool> condition = null) : base(owner)
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

        public CallbackModifier(Action<T, T> callback, object owner, Func<T, T, bool> condition = null)
            : base(owner)
        {
            _callback = callback;
            _condition = condition ?? ((_, _) => true);
        }

        public override bool CanApply(T originalValue, T currentValue) => _condition(originalValue, currentValue);
        public override T Apply(T originalValue, T currentValue) => currentValue; // 値は変更しない
        public override void OnApplied(T originalValue, T resultValue) => _callback(originalValue, resultValue);
    }

    // 汎用関数型モディファイア
    public class FunctionalModifier<T> : ModifierBase<T>
    {
        private readonly Func<T, T, T> _modifier;
        private readonly Func<bool> _condition;

        public FunctionalModifier(object owner, Func<T, T, T> modifier, Func<bool> condition = null)
            : base(owner)
        {
            _modifier = modifier;
            _condition = condition ?? (() => true);
        }

        public override bool CanApply(T originalValue, T currentValue) => _condition();
        public override T Apply(T originalValue, T currentValue) => _modifier(originalValue, currentValue);
    }
}