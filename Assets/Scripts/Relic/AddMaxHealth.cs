using UnityEngine;

/// <summary>
/// プレイヤーの最大HPを10増加させるレリック
/// </summary>
public class AddMaxHealth : RelicBase
{
    public override void RegisterEffects()
    {
        // 最大HP増加
        if (GameManager.Instance?.Player != null)
        {
            GameManager.Instance.Player.MaxHealth.Value += 10;
            UI?.ActiveAlways();
        }
    }

    public override void RemoveAllEffects()
    {
        base.RemoveAllEffects();
        
        // 最大HP減少
        if (GameManager.Instance?.Player != null)
        {
            GameManager.Instance.Player.MaxHealth.Value -= 10;
        }
    }
}
