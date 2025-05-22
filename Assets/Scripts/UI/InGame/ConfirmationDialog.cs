using System.Text;
using System.Threading;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using R3;

public class ConfirmationDialog : MonoBehaviour
{
    [SerializeField] private AfterBattleUI afterBattleUI;
    [SerializeField] private GameObject leftWindow;
    [SerializeField] private GameObject rightWindow;
    [SerializeField] private Image rightBallImage;
    [SerializeField] private Image leftBallImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    
    private int _currentBallIndex;
    private Color _defaultTextColor;
    private CancellationTokenSource _cancellationTokenSource;

    public async UniTask<bool> OpenDialog(InventoryUI.InventoryUIState state, BallData ball1, int level, [CanBeNull] BallData ball2)
    {
        // 前回のキャンセルトークンがあれば破棄
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        
        var text = state switch
        {
            InventoryUI.InventoryUIState.Replace => "ボールの置き換えを行いますか？",
            InventoryUI.InventoryUIState.Swap => "ボール位置の入れ替えを行いますか？",
            InventoryUI.InventoryUIState.Remove => "ボールを削除しますか？",
            InventoryUI.InventoryUIState.Upgrade => "ボールの強化を行いますか？",
            _ => ""
        };
        descriptionText.text = text;
        
        // ボールのプレビューを表示
        switch(state)
        {
            case InventoryUI.InventoryUIState.Replace:
                SetBallTexts(leftWindow, ball1, level);
                leftBallImage.sprite = ball1.sprite;
                SetBallTexts(rightWindow, ball2, 0, true, ball1, level);
                rightBallImage.sprite = ball2.sprite;
                break;
            case InventoryUI.InventoryUIState.Swap:
                SetBallTexts(leftWindow, ball1, level);
                leftBallImage.sprite = ball1.sprite;
                SetBallTexts(rightWindow, ball2, 0, true, ball1, level);
                rightBallImage.sprite = ball2.sprite;
                break;
            case InventoryUI.InventoryUIState.Remove:
                SetBallTexts(leftWindow, ball1, level);
                break;
            case InventoryUI.InventoryUIState.Upgrade:
                SetBallTexts(leftWindow, ball1, level);
                leftBallImage.sprite = ball1.sprite;
                SetBallTexts(rightWindow, ball1, level+1, true, ball1, level);
                rightBallImage.sprite = ball1.sprite;
                break;
        }
        
        UIManager.Instance.EnableCanvasGroup("Confirm", true);
        
        // どちらかのボタンが押されるまで待機
        var confirmResult = await Observable.Merge(
            confirmButton.OnClickAsObservable().Select(_ => true),
            cancelButton.OnClickAsObservable().Select(_ => false)
        ).FirstAsync(cancellationToken: _cancellationTokenSource.Token);
        
        // ダイアログを閉じる
        UIManager.Instance.EnableCanvasGroup("Confirm", false);
        Debug.Log($"Confirm result: {confirmResult}");
        
        return confirmResult;
    }

    private void SetBallTexts(GameObject g, BallData b, int level, bool highlightDifferences = false, BallData comparisonBall = null, int comparisonLevel = 0)
    {
        var nameText = g.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        nameText.text = b.displayName;
        nameText.color = b.rarity.GetColor();

        g.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = b.descriptions[level];
        g.transform.Find("FlavorText").GetComponent<TextMeshProUGUI>().text = b.flavorText;

        var levelText = g.transform.Find("Status").Find("Status1").GetComponent<TextMeshProUGUI>();
        var attackText = g.transform.Find("Status").Find("Status2").GetComponent<TextMeshProUGUI>();
        var sizeText = g.transform.Find("Status").Find("Status3").GetComponent<TextMeshProUGUI>();

        levelText.text = "level: " + (level + 1);
        levelText.alpha = 1;

        attackText.text = "attack: " + b.attacks[level];
        sizeText.text = "size: " + b.sizes[level];

        attackText.alpha = 1;
        sizeText.alpha = 1;
        
        // デフォルトカラーに戻す
        attackText.color = _defaultTextColor;
        sizeText.color = _defaultTextColor;

        // **フラグに基づく比較処理**
        if (highlightDifferences)
        {
            var compareAttack = 1f;
            var compareSize = 1f;
            
            // 同じボールのレベル差分を比較する場合（アップグレード時）
            if (!comparisonBall && level > 0)
            {
                compareAttack = b.attacks[level - 1];
                compareSize = b.sizes[level - 1];
            }
            // 異なるボール間の差分を比較する場合（スワップ、リプレイス時）
            else if (comparisonBall)
            {
                compareAttack = comparisonBall.attacks[comparisonLevel];
                compareSize = comparisonBall.sizes[comparisonLevel];
            }
            // 比較対象がない場合は処理しない
            else
            {
                return;
            }
            
            // 攻撃力: 上昇 → 緑色、低下 → 赤色、変化なし → 白色
            if (b.attacks[level] > compareAttack)
                attackText.color = Color.green;
            else if (b.attacks[level] < compareAttack)
                attackText.color = Color.red;
            
            // サイズ: 上昇 → 赤色、低下 → 緑色、変化なし → 白色
            if (b.sizes[level] > compareSize)
                sizeText.color = Color.red;
            else if (b.sizes[level] < compareSize)
                sizeText.color = Color.green;
        }
    }
    
    private static string GetColoredDifference(string beforeText, string afterText)
    {
        if (string.IsNullOrEmpty(beforeText)) return $"<color=green>{afterText}</color>";
        if (string.IsNullOrEmpty(afterText)) return beforeText;

        var result = new StringBuilder(afterText.Length + 50);

        var beforeLength = beforeText.Length;
        var afterLength = afterText.Length;
        var length = Mathf.Max(beforeLength, afterLength);

        for (var i = 0; i < length; i++)
        {
            var afterChar = i < afterLength ? afterText[i] : '\0';
            var beforeChar = i < beforeLength ? beforeText[i] : '\0';

            if (afterChar == beforeChar)
            {
                result.Append(afterChar);
            }
            else
            {
                // 変更された文字を緑色で表示
                result.Append("<color=green>");
                result.Append(afterChar);
                result.Append("</color>");
            }
        }
        return result.ToString();
    }
    
    private void Awake()
    {
        _defaultTextColor = leftWindow.transform.Find("Status").Find("Status1").GetComponent<TextMeshProUGUI>().color;
    }
    
    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}
