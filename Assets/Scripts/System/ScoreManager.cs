using TMPro;
using UnityEngine;
using DG.Tweening;
using unityroom.Api;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI enemyText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI totalText;

    private const float STAGE_COEFFICIENT = 5f;
    private const float ENEMY_COEFFICIENT = 3f;
    private const float COIN_COEFFICIENT = 1.5f;

    public void ShowScore(int stageCount, int enemyCount, int coinCount)
    {
        GameManager.Instance.UIManager.EnableCanvasGroup("GameOver", true);

        // 初期状態を非表示かつスケール0に設定
        ResetTransform(stageText);
        ResetTransform(enemyText);
        ResetTransform(coinText);
        ResetTransform(totalText);

        // スコア計算
        float stageScore = (int)(stageCount * STAGE_COEFFICIENT);
        float enemyScore = (int)(enemyCount * ENEMY_COEFFICIENT);
        float coinScore = (int)(coinCount * COIN_COEFFICIENT);
        var total = (int)(stageScore + enemyScore + coinScore);
        
        if(UnityroomApiClient.Instance != null)
            UnityroomApiClient.Instance.SendScore(1, total, ScoreboardWriteMode.HighScoreDesc);

        // アニメーションの順番
        var sequence = DOTween.Sequence();

        // ステージスコア表示
        sequence.Append(stageText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    AnimateText(stageText, "Stage:", stageCount, STAGE_COEFFICIENT);
                }).SetUpdate(true);

        // 敵スコア表示
        sequence.AppendInterval(1f)
                .Append(enemyText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    AnimateText(enemyText, "Enemy:", enemyCount, ENEMY_COEFFICIENT);
                }).SetUpdate(true);

        // コインスコア表示
        sequence.AppendInterval(1f)
                .Append(coinText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    AnimateText(coinText, "Coin:", coinCount, COIN_COEFFICIENT);
                }).SetUpdate(true);

        // トータルスコア表示
        sequence.AppendInterval(1.5f)
                .Append(totalText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    AnimateTotal(totalText, total);
                }).SetUpdate(true);
    }

    private static void AnimateText(TextMeshProUGUI text, string header, int count, float coefficient)
    {
        var currentValue = 0;
        DOTween.To(() => currentValue, x => currentValue = x, count, 0.75f)
            .OnUpdate(() =>
            {
                int result = (int)(currentValue * coefficient);
                text.text = $"{header,-8} {currentValue,5} x {coefficient,3} = {result,5}";
            }).SetUpdate(true);
    }

    private static void AnimateTotal(TextMeshProUGUI text, int total)
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
