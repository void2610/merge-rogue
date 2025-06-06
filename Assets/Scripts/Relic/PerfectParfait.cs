/// <summary>
/// ボールで0個のときに攻撃力を5倍にするレリック
/// </summary>
public class PerfectParfait : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤー攻撃時の修正を登録
        RelicHelpers.RegisterAttackMultiplier(this, 5.0f, 
            condition: () => MergeManager.Instance?.GetBallCount() == 0);
    }
}
