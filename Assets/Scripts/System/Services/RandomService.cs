/// <summary>
/// ランダム数値生成を担当するサービス実装クラス
/// GameManagerのランダム機能をラップ
/// </summary>
public class RandomService : IRandomService
{
    /// <summary>
    /// 指定された範囲内のランダムなfloat値を生成する
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    /// <returns>指定範囲内のランダムfloat値</returns>
    public float RandomRange(float min, float max)
    {
        return GameManager.Instance.RandomRange(min, max);
    }
    
    /// <summary>
    /// 指定された範囲内のランダムなint値を生成する
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値（exclusive）</param>
    /// <returns>指定範囲内のランダムint値</returns>
    public int RandomRange(int min, int max)
    {
        return GameManager.Instance.RandomRange(min, max);
    }
}