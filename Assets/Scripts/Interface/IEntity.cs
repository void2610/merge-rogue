using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IEntity
{
    public List<StatusEffectBase> StatusEffects { get; }
    public void Damage(AttackType type, int damage); 
    public void Heal(int healAmount);
    public void AddStatusEffect(StatusEffectBase effect);
    public void RemoveStatusEffect(StatusEffectType type, int stack);
    int ModifyIncomingDamage(int amount);
    int ModifyOutgoingAttack(AttackType type, int amount);
    public UniTask UpdateStatusEffects();
    public void OnBattleEnd();
}
