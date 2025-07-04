using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using Coffee.UIEffects;
using VContainer;

public class Shop : MonoBehaviour
{
    public enum ShopItemType
    {
        Ball,
        Relic,
        Remove
    }
    
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private GameObject removeButton;
    
    private readonly List<object> _currentItems = new();
    private readonly List<int> _currentItemPrices = new();
    private const int ITEM_NUM = 6;
    private List<GameObject> _itemObjects;
    private int _selectedIndex = -1;
    
    private IContentService _contentService;
    private IRandomService _randomService;
    private IRelicService _relicService;
    private IInventoryService _inventoryService;
    
    [Inject]
    public void InjectDependencies(IContentService contentService, IRandomService randomService, IRelicService relicService, IInventoryService inventoryService)
    {
        _contentService = contentService;
        _randomService = randomService;
        _relicService = relicService;
        _inventoryService = inventoryService;
    }
    
    public void UnInteractableSelectedItem() => _itemObjects[_selectedIndex].GetComponent<Button>().interactable = false;
    public void SetRemoveButtonInteractable(bool b) => removeButton.GetComponent<Button>().interactable = b;
    
    public void OpenShop(int count = 6)
    {
        if (count > ITEM_NUM) throw new System.Exception("Invalid count");
        
        for (var i = 0; i < ITEM_NUM; i++) _itemObjects[i].GetComponent<Button>().interactable = true;

        removeButton.GetComponent<Button>().interactable = true;
        removeButton.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = _contentService.GetBallRemovePrice().ToString();
        
        _currentItems.Clear();
        _currentItemPrices.Clear();
        _currentItemPrices.AddRange(Enumerable.Repeat(0, ITEM_NUM));
        
        for(var i = 0; i < ITEM_NUM; i++)
        {
            var balls = _contentService.GetBallListExceptNormal();
            var isBall = _randomService.RandomRange(0.0f, 1.0f) > 0.5f;
            if (isBall)
            {
                var index = _randomService.RandomRange(0, balls.Count);
                _currentItems.Add(balls[index]);
                SetBallEvent(_itemObjects[i].transform.gameObject, balls[index], i);
            }
            else
            {
                var r = _contentService.GetRandomRelic();
                _currentItems.Add(r);
                SetRelicEvent(_itemObjects[i].transform.gameObject, r, i);
            }
        }
    }

    private void CloseShop()
    {
        for (var i = 0; i < ITEM_NUM; i++)
        {
            _itemObjects[i].transform.DOScale(3, 0.1f).SetUpdate(true);
        }
        _selectedIndex = -1;
        
        UIManager.Instance.EnableCanvasGroup("Shop", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }

    private void BuyBall(int index)
    {
        var ball = _currentItems[index] as BallData;
        if (!ball) return;
        
        _selectedIndex = index;
        _inventoryService.StartEditReplace(ball);
        UIManager.Instance.ResetSelectedGameObject();
    }

    private void BuyRelic(int index)
    {
        var relic = _currentItems[index] as RelicData;
        if (!relic) return;
        _relicService.AddRelic(relic);
        _itemObjects[index].GetComponent<Button>().interactable = false;
        GameManager.Instance.SubCoin(_currentItemPrices[index]);
        UIManager.Instance.ResetSelectedGameObject();
    }

    private void SetBallEvent(GameObject g, BallData ball, int index)
    {
        var price = _contentService.GetShopPrice(ShopItemType.Ball, ball.rarity);
        _currentItemPrices[index] = price;
        var priceText = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        priceText.text = price.ToString();
        var icon = g.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = ball.sprite;
        icon.GetComponent<ItemUIEffect>().SetColor(ball.rarity);
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
                    NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.NotEnoughCoin);
                    SeManager.Instance.PlaySe("error");
                    SelectionCursor.SetSelectedGameObjectSafe(g);
                }
            });
        }
        
        g.AddDescriptionWindowEvent(ball);
    }
    
    private void SetRelicEvent(GameObject g, RelicData relic, int index)
    {
        Utils.RemoveAllEventFromObject(g);
        
        var price = _contentService.GetShopPrice(ShopItemType.Relic, relic.rarity);
        _currentItemPrices[index] = price;
        var priceText = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        priceText.text = price.ToString();
        
        var icon = g.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = relic.sprite;
        icon.GetComponent<ItemUIEffect>().SetColor(relic.rarity);
        // ボールの画像を透明にする
        var ballImage = g.transform.Find("BallBase").GetComponent<Image>();
        ballImage.color = new Color(1, 1, 1, 0);
        var button = g.GetComponent<Button>();
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                if (!relic) return;
                if (GameManager.Instance.Coin.Value >= price) BuyRelic(index);
                else
                {
                    NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.NotEnoughCoin);
                    SeManager.Instance.PlaySe("error");
                    SelectionCursor.SetSelectedGameObjectSafe(g);
                }
            });
        }

        g.AddDescriptionWindowEvent(relic);
    }
    
    private void OnClickRemoveButton()
    {
        var price = _contentService.GetBallRemovePrice();
        if (GameManager.Instance.Coin.Value < price)
        {
            NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.NotEnoughCoin);
            SeManager.Instance.PlaySe("error");
            SelectionCursor.SetSelectedGameObjectSafe(removeButton);
            return;
        }
        
        // ボール削除イベント（現在は未実装）
        _inventoryService.StartEditRemove();
    }
    
    private void OnClickSkipButton() => CloseShop();

    private void Awake()
    {
        _itemObjects = itemContainer.GetComponentInChildren<Transform>().Cast<Transform>().Select(x => x.gameObject).ToList();
        
        skipButton.GetComponent<Button>().onClick.AddListener(OnClickSkipButton);
        removeButton.GetComponent<Button>().onClick.AddListener(OnClickRemoveButton);
    }
}