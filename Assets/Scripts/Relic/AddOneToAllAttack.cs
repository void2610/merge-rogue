using UnityEngine;

public class AddOneToAllAttack : MonoBehaviour, IRelicBehavior
{
    public IRelicBehavior.EffectTiming timing => IRelicBehavior.EffectTiming.OnPlayerAttack;
    public void ApplyEffect()
    {
    }

    public void RemoveEffect()
    {
    }
}