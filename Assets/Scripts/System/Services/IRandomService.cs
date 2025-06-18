/// <summary>
/// ランダム数値生成を担当するサービスインターフェース
/// GameManagerのランダム機能を抽象化
/// </summary>
public interface IRandomService
{
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