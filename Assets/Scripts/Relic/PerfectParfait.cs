/// <summary>
/// ボールで0個のときに攻撃力を5倍にするレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class PerfectParfait : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤー攻撃時の修正を登録
        RegisterPlayerAttackModifier(
            current =>
            {
                var ballCount = MergeManager.Instance?.GetBallCount() ?? 0;
                if (ballCount == 0)
                {
                    UI?.ActivateUI();
                    return current.Multiply(5.0f);
                }
                return current;
            },
            condition: () => MergeManager.Instance?.GetBallCount() == 0
        );
    }
}
