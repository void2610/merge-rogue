using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    Dictionary<StatusEffectType, int> StatusEffectStacks { get; }
    Transform transform { get; }
    void Damage(AttackType type, int damage); 
    void Heal(int healAmount);
}
