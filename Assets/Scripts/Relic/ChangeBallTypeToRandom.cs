using R3;

public class ChangeBallTypeToRandom : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnBallCreate.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);
    }

    protected override void EffectImpl(Unit _)
    {   
        var type = (BallShapeType)(GameManager.Instance.RandomRange(0, BallShapeType.GetValues(typeof(BallShapeType)).Length));
        var data = EventManager.OnBallCreate.GetValue();
        data.shapeType = type;
        EventManager.OnBallCreate.SetValue(data);
        
        UI?.ActivateUI();
    }
}
