using UnityEngine;
using R3;

public class AddElasticityToWall : RelicBase
{
    private PhysicsMaterial2D _pm;
    protected override void SubscribeEffect()
    {
        _pm = MergeManager.Instance.GetWallMaterial();

        _pm.bounciness = 0.8f;
        UI?.ActiveAlways();
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        _pm.bounciness = 0;
    }
    
    protected override void EffectImpl(Unit _) {}
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
