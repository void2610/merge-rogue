/// <summary>
/// プレイヤーの攻撃時に通常攻撃力に+5する
/// </summary>
public class AddOneToAllAttack : RelicBase
{
    protected override void RegisterEffects()
    {
        // 攻撃力に+5する
        RegisterAttackAddition(5);
    }
}