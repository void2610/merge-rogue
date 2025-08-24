/// <summary>
/// ランダム数値生成を担当するサービスインターフェース
/// シード値を使用した再現可能なランダム生成をサポート
/// </summary>
public interface IRandomService
{
    string SeedText { get; }
    
    /// <summary>
    /// 指定された確率でtrueを返す
    /// </summary>
    bool Chance(float probability);
    
    /// <summary>
    /// 指定された範囲内のランダムなfloat値を生成する
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    /// <returns>指定範囲内のランダムfloat値</returns>
    float RandomRange(float min, float max);
    
    /// <summary>
    /// 指定された範囲内のランダムなint値を生成する
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値（exclusive）</param>
    /// <returns>指定範囲内のランダムint値</returns>
    int RandomRange(int min, int max);
}