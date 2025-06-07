/// <summary>
/// ボールで0個のときに攻撃力を5倍にするレリック
/// </summary>
public class PerfectParfait : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤー攻撃時の修正を登録
        EventManager.OnPlayerAttack.AddProcessor(this, current =>
        {
            if (MergeManager.Instance?.GetBallCount() == 0)
            {
                ActivateUI();
                return (int)(current * 5.0f);
            }
            return current;
        });
    }
}
