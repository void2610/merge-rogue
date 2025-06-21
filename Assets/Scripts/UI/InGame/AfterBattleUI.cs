using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Coffee.UIEffects;
using VContainer;

public class AfterBattleUI : MonoBehaviour
{
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private MyButton ballUpgradeButton;
    [SerializeField] private TextMeshProUGUI ballUpgradePriceText;
    [SerializeField] private Button skipButton;
    
    private const int ITEM_NUM = 3;
    private readonly List<BallData> _currentItems = new();
    private List<GameObject> _itemObjects;
    private int _selectedIndex = -1;
    
    private IContentService _contentService;
    private IRandomService _randomService;
    private IInventoryService _inventoryService;
    
    [Inject]
    public void InjectDependencies(IContentService contentService, IRandomService randomService, IInventoryService inventoryService)
    {
        _contentService = contentService;
        _randomService = randomService;
        _inventoryService = inventoryService;
    }
    
    public void UnInteractableSelectedItem() => _itemObjects[_selectedIndex].GetComponent<MyButton>().IsAvailable = false;
    public void SetUpgradeButtonInteractable(bool b) => ballUpgradeButton.IsAvailable = b;
    
    private void BuyBall(int index)
    {
        var ball = _currentItems[index];
        if (!ball) return;
        
        _selectedIndex = index;
        _inventoryService.StartEditReplace(ball);
    }
    
    private void SetBallEvent(GameObject g, BallData ball, int index)
    {
        var price = _contentService.GetShopPrice(Shop.ShopItemType.Ball, ball.rarity);
        var priceText = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        priceText.text = price.ToString();
        var icon = g.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = ball.sprite;
        icon.GetComponent<ItemUIEffect>().SetColor(ball.rarity);
        var button = g.GetComponent<MyButton>();
        if (button)
        {
            Utils.RemoveAllEventFromObject(g);
            button.onClick.AddListener(() =>
            {
                if (!ball) return;
                if (GameManager.Instance.Coin.Value >= price) BuyBall(index);
                else
                {
                    NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.NotEnoughCoin);
                    SeManager.Instance.PlaySe("error");
                }
            });
        }
        g.AddDescriptionWindowEvent(ball);
    }
    
    private void OnClickBallUpgradeButton()
    {
        _inventoryService.StartEditUpgrade();
    }
    
    private void OnClickSkipAfterBattle()
    {
        _selectedIndex = -1;
        UIManager.Instance.EnableCanvasGroup("AfterBattle", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    public void OpenAfterBattle()
    {
        var price = _contentService.GetBallUpgradePrice();
        ballUpgradePriceText.text = price.ToString();
        var interactable = GameManager.Instance.Coin.Value >= price;
        ballUpgradeButton.IsAvailable = interactable;

        _currentItems.Clear();
        
        for(var i = 0; i < ITEM_NUM; i++)
        {
            var balls = _contentService.GetBallListExceptNormal();
            var index = _randomService.RandomRange(0, balls.Count);
            _currentItems.Add(balls[index]);
            SetBallEvent(_itemObjects[i].transform.gameObject, balls[index], i);
            _itemObjects[i].GetComponent<MyButton>().IsAvailable = true;
        }
    }

    private void Awake()
    {
        _itemObjects = itemContainer.GetComponentInChildren<Transform>().Cast<Transform>().Select(x => x.gameObject).ToList();
        
        ballUpgradeButton.onClick.AddListener(OnClickBallUpgradeButton);
        skipButton.onClick.AddListener(OnClickSkipAfterBattle);
    }
}
