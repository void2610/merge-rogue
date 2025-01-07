public interface IEntity
{
    public void Damage(int damage); 
    public void Heal(int healAmount);
    public void AddStatusEffect(IStatusEffect effect);
    public void UpdateStatusEffects();
}
