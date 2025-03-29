using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class AfterBattleUI : MonoBehaviour
{
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private Button ballUpgradeButton;
    [SerializeField] private TextMeshProUGUI ballUpgradePriceText;
    [SerializeField] private Button skipButton;
    
    private const int ITEM_NUM = 3;
    private readonly List<BallData> _currentItems = new();
    private readonly List<int> _currentItemPrices = new();
    private List<GameObject> _itemObjects;
    private static BallDataList AllBalls => InventoryManager.Instance.allBallDataList;
    
    public void SetInteractable(bool b) => ballUpgradeButton.interactable = b;
    
    private void OpenShop(int count = 3)
    {
        if (count > ITEM_NUM) throw new System.Exception("Invalid count");
        
        _currentItems.Clear();
        _currentItemPrices.Clear();
        _currentItemPrices.AddRange(Enumerable.Repeat(0, ITEM_NUM));
        
        for(var i = 0; i < ITEM_NUM; i++)
        {
            var balls = ContentProvider.Instance.GetBallListExceptNormal();
            var index = GameManager.Instance.RandomRange(0, balls.Count);
            _currentItems.Add(balls[index]);
            SetBallEvent(_itemObjects[i].transform.gameObject, balls[index], i);
        }
    }
    
    private void BuyBall(int index)
    {
        var ball = _currentItems[index] as BallData;
        if (!ball) return;
        SeManager.Instance.PlaySe("coin");
        InventoryManager.Instance.AddBall(ball);
        _itemObjects[index].GetComponent<Button>().interactable = false;
        GameManager.Instance.SubCoin(_currentItemPrices[index]);
        
        UIManager.Instance.ResetSelectedGameObject();
    }
    
    private void SetBallEvent(GameObject g, BallData ball, int index)
    {
        var price = ContentProvider.GetSHopPrice(Shop.ShopItemType.Ball, ball.rarity);
        _currentItemPrices[index] = price;
        var priceText = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        priceText.text = price.ToString();
        var baseImage = g.transform.Find("BallBase").GetComponent<Image>();
        baseImage.sprite = ContentProvider.Instance.GetBallBaseImage(ball.shapeType);
        var image = g.transform.Find("Icon").GetComponent<Image>();
        if (ball.sprite)
        {
            image.color = new Color(1, 1, 1, 1);
            image.sprite = ball.sprite;
        }
        else image.color = new Color(0, 0, 0, 0);
        var ballImage = g.transform.Find("BallBase").GetComponent<Image>();
        ballImage.color = new Color(0.6f, 0.6f, 0.6f, 1);
        var button = g.GetComponent<Button>();
        if (button)
        {
            Utils.RemoveAllEventFromObject(g);
            button.onClick.AddListener(() =>
            {
                if (!ball) return;
                if (GameManager.Instance.Coin.Value >= price) BuyBall(index);
                else
                {
                    NotifyWindow.Instance.Notify("コインが足りません！", NotifyWindow.NotifyIconType.Error);
                    SeManager.Instance.PlaySe("error");
                }
            });
        }
        
        g.AddDescriptionWindowEvent(ball);
    }
    
    private void OnClickBallUpgradeButton()
    {
        InventoryManager.Instance.InventoryUI.StartEdit(InventoryUI.InventoryUIState.Upgrade);
    }
    
    private void OnClickSkipAfterBattle()
    {
        UIManager.Instance.EnableCanvasGroup("AfterBattle", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    public void OpenAfterBattle()
    {
        var price = ContentProvider.GetBallUpgradePrice();
        ballUpgradePriceText.text = price.ToString();
        var interactable = GameManager.Instance.Coin.Value >= price;
        ballUpgradeButton.interactable = interactable;
        OpenShop();
    }

    private void Awake()
    {
        _itemObjects = itemContainer.GetComponentInChildren<Transform>().Cast<Transform>().Select(x => x.gameObject).ToList();
        
        ballUpgradeButton.onClick.AddListener(OnClickBallUpgradeButton);
        skipButton.onClick.AddListener(OnClickSkipAfterBattle);
    }
}
