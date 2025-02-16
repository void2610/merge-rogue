using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfterBattleUI : MonoBehaviour
{
    [SerializeField] private Button ballUpgradeButton;
    [SerializeField] private TextMeshProUGUI ballUpgradePriceText;
    [SerializeField] private Button skipButton;
    
    public void SetInteractable(bool b) => ballUpgradeButton.interactable = b;
    
    private void OnClickBallUpgradeButton()
    {
        InventoryManager.Instance.InventoryUI.StartEdit(InventoryUI.InventoryUIState.Upgrade);
        ballUpgradeButton.interactable = false;
    }
    
    private void OnClickSkipAfterBattle()
    {
        InventoryManager.Instance.InventoryUI.EnableCursor(false);
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
