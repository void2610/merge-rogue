using UnityEngine;

/// <summary>
/// スタック数*10%の確率で行動不能、行動不能時にスタック数が半分になる
/// </summary>
public class FreezeEffect : StatusEffectBase
{
    public bool IsFrozen()
    {
        if (StackCount <= 0) return false;
        
        var rand = GameManager.Instance.RandomRange(0.0f, 100.0f);
        if (rand < StackCount * 10)
        {
            StackCount /= 2;
            ShowEffectText();
            PlaySoundEffect();
            return true;
        }
        return false;
    }
}