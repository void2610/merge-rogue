using System.Numerics;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Vector3 = UnityEngine.Vector3;
using VContainer;

/// <summary>
/// スコア表示を担当するMonoBehaviourコンポーネント
/// スコア計算ロジックはIScoreServiceに移譲
/// </summary>
public class ScoreDisplayComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI enemyText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI totalText;

    private const float STAGE_COEFFICIENT = 5f;
    private const float ENEMY_COEFFICIENT = 3f;
    private const int COIN_COEFFICIENT = 1;
    
    private IScoreService _scoreService;
    
    [Inject]
    public void InjectDependencies(IScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    public void ShowScore(int stageCount, int enemyCount, BigInteger coinCount)
    {
        UIManager.Instance.EnableCanvasGroup("GameOver", true);

        // 初期状態を非表示かつスケール0に設定
        ResetTransform(stageText);
        ResetTransform(enemyText);
        ResetTransform(coinText);
        ResetTransform(totalText);

        // スコア計算とキャッシュ・送信（サービスに移譲）
        _scoreService.CalculateAndSubmitScore(stageCount, enemyCount, coinCount);
        
        // UI表示用のスコア計算
        var (stageScore, enemyScore, coinScore) = _scoreService.CalcScore(stageCount, enemyCount, coinCount);  
        var total = (ulong)(stageScore + enemyScore + coinScore);

        // アニメーションの順番
        var sequence = DOTween.Sequence();

        // ステージスコア表示
        sequence.Append(stageText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    AnimateText(stageText, "Stage:", (ulong)stageCount, STAGE_COEFFICIENT);
                }).SetUpdate(true);

        // 敵スコア表示
        sequence.AppendInterval(1f)
                .Append(enemyText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    AnimateText(enemyText, "Enemy:", (ulong)enemyCount, ENEMY_COEFFICIENT);
                }).SetUpdate(true);

        // コインスコア表示
        sequence.AppendInterval(1f)
                .Append(coinText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    AnimateText(coinText, "Coin:", (ulong)coinCount, COIN_COEFFICIENT);
                }).SetUpdate(true);

        // トータルスコア表示
        sequence.AppendInterval(1.5f)
                .Append(totalText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    AnimateTotal(totalText, total);
                }).SetUpdate(true);
    }
    
    /// <summary>
    /// スコア計算メソッド（後方互換性のため残存、内部でサービスを呼び出し）
    /// </summary>
    public (int, int, BigInteger) CalcScore(int stageCount, int enemyCount, BigInteger coinCount)
    {
        return _scoreService.CalcScore(stageCount, enemyCount, coinCount);
    }

    private static void AnimateText(TextMeshProUGUI text, string header, ulong count, float coefficient)
    {
        ulong currentValue = 0;
        DOTween.To(() => currentValue, x => currentValue = x, count, 0.75f)
            .OnUpdate(() =>
            {
                var result = (ulong)(currentValue * coefficient);
                text.text = $"{header,-8} {currentValue,5} x {coefficient,3} = {result,5}";
            }).SetUpdate(true);
    }

    private static void AnimateTotal(TextMeshProUGUI text, ulong total)
    {
        float currentValue = 0;
        DOTween.To(() => currentValue, x => currentValue = x, total, 1.5f)
            .OnUpdate(() =>
            {
                text.text = $"Score: {currentValue:F0}";
            }).SetUpdate(true);
    }

    private static void ResetTransform(TextMeshProUGUI text)
    {
        // 初期スケールを0に設定
        text.transform.localScale = Vector3.zero;
    }
}
