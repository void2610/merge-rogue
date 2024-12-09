using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class SometimeCopyDropBall : MonoBehaviour, IRelicBehavior
{
    private IDisposable disposable;
    private RelicUI ui;
    public void ApplyEffect(RelicUI relicUI)
    {
        ui = relicUI;
        disposable = EventManager.OnBallDropped.Subscribe(Effect).AddTo(this);
    }

    public void RemoveEffect()
    {
        disposable.Dispose();
    }
    
    private void Effect(Unit _)
    {
        var r = GameManager.Instance.RandomRange(0.0f, 1.0f);
        if (r < 0.5f)
        {
            var level = MergeManager.Instance.currentBall.GetComponent<BallBase>().level;
            var p = new Vector3(GameManager.Instance.RandomRange(-1f, 1f), 0.8f, 0);
            MergeManager.Instance.SpawnBall(level, p, Quaternion.identity);
            ui?.ActivateUI();
        }
    }
}
