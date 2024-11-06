using UnityEngine;

public class DoubleCoinsWhenLowHealth : MonoBehaviour, IRelicBehavior
{
    public IRelicBehavior.EffectTiming timing => IRelicBehavior.EffectTiming.OnPlayerAttack;
    public void ApplyEffect()
    {
    }

    public void RemoveEffect()
    {
    }
}
