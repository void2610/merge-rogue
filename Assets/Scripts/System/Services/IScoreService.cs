using System.Numerics;

/// <summary>
/// スコア計算とスコア送信を管理するサービス
/// </summary>
public interface IScoreService
{
    /// <summary>
    /// ステージ数、敵撃破数、コイン数からスコアを計算する
    /// </summary>
    /// <param name="stageCount">ステージ数</param>
    /// <param name="enemyCount">敵撃破数</param>
    /// <param name="coinCount">コイン数</param>
    /// <returns>各要素のスコア（ステージスコア、敵スコア、コインスコア）</returns>
    (int stageScore, int enemyScore, BigInteger coinScore) CalcScore(int stageCount, int enemyCount, BigInteger coinCount);
    
    /// <summary>
    /// unityroomにスコアを送信する
    /// </summary>
    /// <param name="totalScore">送信する合計スコア</param>
    void SubmitScore(ulong totalScore);
    
    /// <summary>
    /// 計算されたスコアをキャッシュしてunityroomに送信する
    /// </summary>
    /// <param name="stageCount">ステージ数</param>
    /// <param name="enemyCount">敵撃破数</param>
    /// <param name="coinCount">コイン数</param>
    void CalculateAndSubmitScore(int stageCount, int enemyCount, BigInteger coinCount);
    
    /// <summary>
    /// キャッシュされたスコアをTwitterでシェアする
    /// </summary>
    void TweetScore();
}