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

    public void OpenUpgradeConfirmPanel(int index)
    {
        _currentBallIndex = index;
        var ball = InventoryManager.Instance.GetBallData(index);
        var rank = InventoryManager.Instance.GetBallLevel(index);
        
        SetBallTexts(leftWindow, ball, rank);
        SetBallTexts(rightWindow, ball, rank + 1);
        ballImage.sprite = ball.sprite;
        if(!ball.sprite) ballImage.color = new Color(0, 0, 0, 0);
        UIManager.Instance.EnableCanvasGroup("Upgrade", true);
    }
    
    private void SetBallTexts(GameObject g, BallData b, int level)
    {
        g.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = b.displayName;
        g.transform.Find("NameText").GetComponent<TextMeshProUGUI>().color = b.rarity.GetColor();
        g.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = b.descriptions[level];
        g.transform.Find("FlavorText").GetComponent<TextMeshProUGUI>().text = b.flavorText;
        g.transform.Find("Status").Find("Status1").GetComponent<TextMeshProUGUI>().text = "level: " + (level + 1);
        g.transform.Find("Status").Find("Status1").GetComponent<TextMeshProUGUI>().alpha = 1;
        g.transform.Find("Status").Find("Status2").GetComponent<TextMeshProUGUI>().text = "attack: " + b.attacks[level];
        g.transform.Find("Status").Find("Status2").GetComponent<TextMeshProUGUI>().alpha = 1;
        g.transform.Find("Status").Find("Status3").GetComponent<TextMeshProUGUI>().text = "size: " + b.sizes[level];
        g.transform.Find("Status").Find("Status3").GetComponent<TextMeshProUGUI>().alpha = 1;
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
        cancelButton.onClick.AddListener(Cancel);
        upgradeButton.onClick.AddListener(Upgrade);
    }
}
