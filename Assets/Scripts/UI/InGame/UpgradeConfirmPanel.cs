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
    
    private void Upgrade()
    {
        InventoryManager.Instance.UpgradeBall(_currentBallIndex);
        GameManager.Instance.SubCoin(ContentProvider.GetBallUpgradePrice());
        afterBattleUI.SetInteractable(false);
        SeManager.Instance.PlaySe("levelUp");
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
