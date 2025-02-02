using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfterBattleUI : MonoBehaviour
{
    [SerializeField] private Button ballUpgradeButton;
    [SerializeField] private TextMeshProUGUI ballUpgradePriceText;
    [SerializeField] private Button skipButton;
    
    private void OnClickBallUpgradeButton()
    {
        SeManager.Instance.PlaySe("button");
        GameManager.Instance.SubCoin(ContentProvider.GetBallUpgradePrice());
        ballUpgradeButton.interactable = false;
        InventoryManager.Instance.InventoryUI.StartEdit(InventoryUI.InventoryUIState.Upgrade);
    }
    
    private void OnClickSkipAfterBattle()
    {
        SeManager.Instance.PlaySe("button");
        UIManager.Instance.EnableCanvasGroup("AfterBattle", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    public void UpdateBallUpgradeButton()
    {
        var price = ContentProvider.GetBallUpgradePrice();
        ballUpgradePriceText.text = price.ToString();
        var interactable = GameManager.Instance.Coin.Value >= price;
        ballUpgradeButton.interactable = interactable;
    }

    private void Awake()
    {
        ballUpgradeButton.onClick.AddListener(OnClickBallUpgradeButton);
        skipButton.onClick.AddListener(OnClickSkipAfterBattle);
    }
}
