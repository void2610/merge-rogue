using System.Numerics;
using unityroom.Api;
using UnityEngine;

/// <summary>
/// スコア計算とスコア送信を管理する純粋なC#サービス
/// </summary>
public class ScoreService : IScoreService
{
    private const float STAGE_COEFFICIENT = 5f;
    private const float ENEMY_COEFFICIENT = 3f;
    private const int COIN_COEFFICIENT = 1;
    
    // スコアキャッシュ用フィールド
    private ulong _cachedTotalScore;

    /// <summary>
    /// ステージ数、敵撃破数、コイン数からスコアを計算する
    /// </summary>
    public (int stageScore, int enemyScore, BigInteger coinScore) CalcScore(int stageCount, int enemyCount, BigInteger coinCount)
    {
        var stageScore = (int)(stageCount * STAGE_COEFFICIENT);
        var enemyScore = (int)(enemyCount * ENEMY_COEFFICIENT);
        var coinScore = coinCount * COIN_COEFFICIENT;
        return (stageScore, enemyScore, coinScore);
    }

    /// <summary>
    /// unityroomにスコアを送信する
    /// </summary>
    public void SubmitScore(ulong totalScore)
    {
        if (UnityroomApiClient.Instance != null)
        {
            UnityroomApiClient.Instance.SendScore(1, totalScore, ScoreboardWriteMode.HighScoreDesc);
        }
    }

    /// <summary>
    /// 計算されたスコアをキャッシュしてunityroomに送信する
    /// </summary>
    public void CalculateAndSubmitScore(int stageCount, int enemyCount, BigInteger coinCount)
    {
        var (stageScore, enemyScore, coinScore) = CalcScore(stageCount, enemyCount, coinCount);
        _cachedTotalScore = (ulong)(stageScore + enemyScore + coinScore);
        
        SubmitScore(_cachedTotalScore);
    }

    /// <summary>
    /// キャッシュされたスコアをTwitterでシェアする
    /// </summary>
    public void TweetScore()
    {
        var text = $"Merge Rogueでスコア: {_cachedTotalScore}を獲得しました！\n" +
                   $"#MergeRogue #unityroom\n" +
                   $"https://unityroom.com/games/mergerogue";
        
        var url = "https://twitter.com/intent/tweet?text=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(text);
        Application.OpenURL(url);
    }
}