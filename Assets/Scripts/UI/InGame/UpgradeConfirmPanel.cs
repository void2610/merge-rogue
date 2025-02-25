using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeConfirmPanel : MonoBehaviour
{
    [SerializeField] private AfterBattleUI afterBattleUI;
    [SerializeField] private GameObject leftWindow;
    [SerializeField] private GameObject rightWindow;
    [SerializeField] private Image ballImage;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button upgradeButton;
    
    private int _currentBallIndex;
    private Color _defaultTextColor;

    public void OpenUpgradeConfirmPanel(int index)
    {
        _currentBallIndex = index;
        var ball = InventoryManager.Instance.GetBallData(index);
        var rank = InventoryManager.Instance.GetBallLevel(index);
        
        SetBallTexts(leftWindow, ball, rank);
        SetBallTexts(rightWindow, ball, rank + 1, true);
        ballImage.sprite = ball.sprite;
        if(!ball.sprite) ballImage.color = new Color(0, 0, 0, 0);
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
    
    public static string GetColoredDifference(string beforeText, string afterText)
    {
        if (string.IsNullOrEmpty(beforeText)) return $"<color=green>{afterText}</color>";
        if (string.IsNullOrEmpty(afterText)) return beforeText;

        // **初期容量を指定してStringBuilderを最適化**
        StringBuilder result = new StringBuilder(afterText.Length + 50);

        int beforeIndex = 0, afterIndex = 0;

        while (beforeIndex < beforeText.Length && afterIndex < afterText.Length)
        {
            if (beforeText[beforeIndex] == afterText[afterIndex])
            {
                // 一致部分はそのまま追加
                result.Append(afterText[afterIndex]);
                beforeIndex++;
                afterIndex++;
            }
            else
            {
                // 変更部分を収集
                int startIndex = afterIndex;
                while (afterIndex < afterText.Length && 
                       (beforeIndex >= beforeText.Length || beforeText[beforeIndex] != afterText[afterIndex]))
                {
                    afterIndex++;
                }

                // 変更部分を緑色にして追加
                result.Append("<color=green>");
                result.Append(afterText.Substring(startIndex, afterIndex - startIndex));
                result.Append("</color>");
            }
        }

        // 残りの部分を追加
        if (afterIndex < afterText.Length)
        {
            result.Append(afterText.Substring(afterIndex));
        }

        return result.ToString();
    }
    
    private void Upgrade()
    {
        InventoryManager.Instance.UpgradeBall(_currentBallIndex);
        GameManager.Instance.SubCoin(ContentProvider.GetBallUpgradePrice());
        SeManager.Instance.PlaySe("levelUp");
        InventoryManager.Instance.InventoryUI.EnableCursor(false);
        UIManager.Instance.EnableCanvasGroup("Upgrade", false);
    }
    
    private void Cancel()
    {
        UIManager.Instance.EnableCanvasGroup("Upgrade", false);
    }

    private void Awake()
    {
        _defaultTextColor = leftWindow.transform.Find("Status").Find("Status1").GetComponent<TextMeshProUGUI>().color;
        cancelButton.onClick.AddListener(Cancel);
        upgradeButton.onClick.AddListener(Upgrade);
    }
}
