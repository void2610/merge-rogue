/// <summary>
/// プレイヤーの攻撃時に通常攻撃力に+5する
/// </summary>
public class AddOneToAllAttack : RelicBase
{
    protected override void RegisterEffects()
    {
        // 攻撃力に+5する
        EventManager.OnPlayerAttack.AddProcessor(this, current =>
        {
            ActivateUI();
            return current + 5;
        });
    }
}