using UnityEngine;

/// <summary>
/// プレイヤーの最大HPを10増加させるレリック
/// </summary>
public class AddMaxHealth : RelicBase
{
    private bool _healthAdded = false;

    protected override void RegisterEffects()
    {
        // 最大HP増加
        if (GameManager.Instance?.Player != null && !_healthAdded)
        {
            GameManager.Instance.Player.MaxHealth.Value += 10;
            _healthAdded = true;
            UI?.ActiveAlways();
        }
    }

    public override void RemoveAllEffects()
    {
        base.RemoveAllEffects();
        
        // 最大HP減少
        if (GameManager.Instance?.Player != null && _healthAdded)
        {
            GameManager.Instance.Player.MaxHealth.Value -= 10;
            _healthAdded = false;
        }
    }
}
