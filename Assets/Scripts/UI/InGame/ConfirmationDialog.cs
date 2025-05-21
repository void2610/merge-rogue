using System.Text;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class ConfirmationDialog : MonoBehaviour
{
    [SerializeField] private AfterBattleUI afterBattleUI;
    [SerializeField] private GameObject leftWindow;
    [SerializeField] private GameObject rightWindow;
    [SerializeField] private Image ballBaseImage;
    [SerializeField] private Image ballImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    
    private int _currentBallIndex;
    private Color _defaultTextColor;
    private InventoryUI.InventoryUIState _state;

    public void OpenDialog(InventoryUI.InventoryUIState state, BallData ball1, [CanBeNull] BallData ball2)
    {
        _state = state;
        
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
                SetBallTexts(leftWindow, ball1, 0);
                SetBallTexts(rightWindow, ball2, 0);
                break;
            case InventoryUI.InventoryUIState.Swap:
                SetBallTexts(leftWindow, ball1, 0);
                SetBallTexts(rightWindow, ball2, 0);
                break;
            case InventoryUI.InventoryUIState.Remove:
                SetBallTexts(leftWindow, ball1, 0);
                break;
            case InventoryUI.InventoryUIState.Upgrade:
                SetBallTexts(leftWindow, ball1, 0);
                SetBallTexts(leftWindow, ball1, 1, true);
                break;
        }
        
        UIManager.Instance.EnableCanvasGroup("Confirm", true);
    }

    public void OpenUpgradeConfirmPanel(int index)
    {
        _currentBallIndex = index;
        var ball = InventoryManager.Instance.GetBallData(index);
        var rank = InventoryManager.Instance.GetBallLevel(index);
        
        SetBallTexts(leftWindow, ball, rank);
        SetBallTexts(rightWindow, ball, rank + 1, true);
        ballImage.sprite = ball.sprite;
        if(!ball.sprite) ballImage.color = new Color(0, 0, 0, 0);
        ballBaseImage.sprite = ContentProvider.Instance.GetBallBaseImage(ball.shapeType);
        UIManager.Instance.EnableCanvasGroup("Upgrade", true);
    }
    
    private void SetBallTexts(GameObject g, BallData b, int level, bool highlightDifferences = false)
    {
        var nameText = g.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        nameText.text = b.displayName;
        nameText.color = b.rarity.GetColor();

        var description = highlightDifferences ? GetColoredDifference(b.descriptions[level - 1], b.descriptions[level]) : b.descriptions[level];
        g.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = description;
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

        // **フラグに基づく比較処理**
        if (highlightDifferences && level > 0)
        {
            // 攻撃力: 上昇 → 緑色、低下 → 赤色、変化なし → 白色
            if (b.attacks[level] > b.attacks[level - 1])
                attackText.color = Color.green;
            else if (b.attacks[level] < b.attacks[level - 1])
                attackText.color = Color.red;
            else
                attackText.color = _defaultTextColor;

            // サイズ: 上昇 → 赤色、低下 → 緑色、変化なし → 白色
            if (b.sizes[level] > b.sizes[level - 1])
                sizeText.color = Color.red;
            else if (b.sizes[level] < b.sizes[level - 1])
                sizeText.color = Color.green;
            else
                sizeText.color = _defaultTextColor;
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
    
    private async UniTaskVoid Confirm()
    {
        GameManager.Instance.SubCoin(ContentProvider.GetBallUpgradePrice());

        // stateに応じた処理を実行
        switch (_state)
        {
            case InventoryUI.InventoryUIState.Replace:
                InventoryUI.Instance.ConductRelace();
                break;
            case InventoryUI.InventoryUIState.Swap:
                await InventoryUI.Instance.ConductSwap();
                GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
                break;
            case InventoryUI.InventoryUIState.Remove:
                InventoryUI.Instance.ConductRemove();
                UIManager.Instance.ResetSelectedGameObject();
                break;
            case InventoryUI.InventoryUIState.Upgrade:
                InventoryUI.Instance.ConductUpgrade();
                SeManager.Instance.PlaySe("levelUp");
                afterBattleUI.SetInteractable(false);
                UIManager.Instance.EnableCanvasGroup("Confirm", false);
                break;
        }
    }
    
    private void Cancel()
    {
        UIManager.Instance.EnableCanvasGroup("Confirm", false);
    }

    private void Awake()
    {
        _defaultTextColor = leftWindow.transform.Find("Status").Find("Status1").GetComponent<TextMeshProUGUI>().color;
        cancelButton.onClick.AddListener(Cancel);
        confirmButton.onClick.AddListener(() => Confirm().Forget());
    }
}
