using UnityEngine;

/// <summary>
/// (敵専用)毎ターン、スタック数に応じたダメージを敵全体に受ける
/// </summary>
public class ShockEffect : StatusEffectBase
{
    protected override void OnTurnEndEffect()
    {
        var damage = StackCount;
        EnemyContainer.Instance.DamageAllEnemies(damage);
        
        var position = Owner is Player player 
            ? player.transform.position 
            : (Owner as EnemyBase)?.transform.position ?? Vector3.zero;
            
        ParticleManager.Instance.ThunderParticle(position + new Vector3(0, 0.3f, 0));
        PlaySoundEffect();
    }
}