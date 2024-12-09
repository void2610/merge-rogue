using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class AddElasticityToWall : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    private PhysicsMaterial2D pm;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;
        pm = MergeManager.Instance.GetWallMaterial();

        Effect(Unit.Default);
    }

    public void RemoveEffect()
    {
        pm.bounciness = 0;
    }
    
    private void Effect(Unit _)
    {
        pm.bounciness = 0.8f;
        ui?.AlwaysActive();
    }
    
    private void OnDestroy()
    {
        RemoveEffect();
    }
}
