using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IEntity
{
    public List<StatusEffectBase> StatusEffects { get; }
    public void Damage(int damage); 
    public void Heal(int healAmount);
    public void AddStatusEffect(StatusEffectBase effect);
    int ModifyIncomingDamage(int amount);
    int ModifyOutgoingAttack(int amount);
    public UniTask UpdateStatusEffects();
    public void OnBattleEnd();
}
