using R3;

public class AllAttackByTenCoin : RelicBase
{
    protected override void SubscribeEffect()
    {
        var disposable = EventManager.OnPlayerAttack.Subscribe(EffectImpl).AddTo(this);
        Disposables.Add(disposable);

        EffectImpl(Unit.Default);
    }
    
    protected override void EffectImpl(Unit _)
    {
        var coin = GameManager.Instance.Coin.Value;
        var e = GameManager.Instance.EnemyContainer.GetCurrentEnemyCount();
        var x = EventManager.OnPlayerAttack.GetValue();

        // 消費するコインが存在し、単体攻撃力が1以上、敵が2体以上いる場合
        // TODO: 賢者の石に依存している部分を修正する
        if ((coin >= 10 && x.Item1 > 0 && e >= 2) || RelicManager.Instance.HasRelic(typeof(NoConsumeCoinDuringBattle)))
        {
            GameManager.Instance.SubCoin(10);
            EventManager.OnPlayerAttack.SetValue((0, (int)((x.Item1 + x.Item2) * 1.5f)));
            UI?.ActivateUI();
        }
    }
}
