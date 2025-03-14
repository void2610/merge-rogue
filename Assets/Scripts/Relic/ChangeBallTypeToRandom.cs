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
        var originalData = EventManager.OnBallCreate.GetValue();
        var newData = Instantiate(originalData);
        newData.shapeType = type;
        EventManager.OnBallCreate.SetValue(newData);
        
        UI?.ActivateUI();
    }
}
