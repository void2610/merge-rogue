using System.Collections.Generic;

public interface IEntity
{
    public List<StatusEffectBase> StatusEffects { get; }
    public void Damage(int damage); 
    public void Heal(int healAmount);
    public void AddStatusEffect(StatusEffectBase effect);
    int ModifyIncomingDamage(int amount);
    public void UpdateStatusEffects();
    public void OnBattleEnd();
}
