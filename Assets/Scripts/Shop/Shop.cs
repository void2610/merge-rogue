using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Shop : MonoBehaviour
{
    public enum ShopItemType
    {
        Ball,
        Relic,
        Remove
    }
    
    public static Shop Instance;
    
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private GameObject removeButton;
    
    private readonly List<object> _currentItems = new();
    private readonly List<int> _currentItemPrices = new();
    private const int ITEM_NUM = 6;
    private List<GameObject> _itemObjects;
    private readonly List<Vector3> _itemPositions = new();
    private readonly Vector3 _disabledPosition = new (100, 100, 0);
    private static BallDataList AllBalls => InventoryManager.Instance.allBallDataList;


    public void OpenShop(int count = 6)
    {
        if (count > ITEM_NUM) return;
        
        for (var i = 0; i < ITEM_NUM; i++)
        {
            _itemObjects[i].transform.position = _itemPositions[i];
        }
        removeButton.transform.position = _itemPositions[ITEM_NUM];
        removeButton.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = ContentProvider.GetBallRemovePrice().ToString();
        
        _currentItems.Clear();
        _currentItemPrices.Clear();
        _currentItemPrices.AddRange(Enumerable.Repeat(0, ITEM_NUM));
        
        for(var i = 0; i < ITEM_NUM; i++)
        {
            var balls = AllBalls.GetBallListExceptNormal();
            var isBall = GameManager.Instance.RandomRange(0.0f, 1.0f) > 0.5f;
            if (isBall)
            {
                var index = GameManager.Instance.RandomRange(0, balls.Count);
                _currentItems.Add(balls[index]);
                SetBallEvent(_itemObjects[i].transform.gameObject, balls[index], i);
            }
            else
            {
                var r = ContentProvider.Instance.GetRandomRelic();
                _currentItems.Add(r);
                SetRelicEvent(_itemObjects[i].transform.gameObject, r, i);
            }
        }
    }

    public void CloseShop()
    {
        for (var i = 0; i < ITEM_NUM; i++)
        {
            _itemObjects[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            _itemObjects[i].transform.DOScale(1, 0.1f).SetUpdate(true);
        }
        InventoryManager.Instance.InventoryUI.EnableCursor(false);
    }

    private void BuyBall(int index)
    {
        var ball = _currentItems[index] as BallData;
        if (!ball) return;
        
        InventoryManager.Instance.AddBall(ball);
        _itemObjects[index].transform.position = _disabledPosition;
        GameManager.Instance.SubCoin(_currentItemPrices[index]);
        SeManager.Instance.PlaySe("coin");
    }

    private void BuyRelic(int index)
    {
        var relic = _currentItems[index] as RelicData;
        if (!relic) return;
    
        RelicManager.Instance.AddRelic(relic);
        _itemObjects[index].transform.position = _disabledPosition;
        GameManager.Instance.SubCoin(_currentItemPrices[index]);
        SeManager.Instance.PlaySe("coin");
    }

    private void SetBallEvent(GameObject g, BallData ball, int index)
    {
        var price = ContentProvider.GetSHopPrice(ShopItemType.Ball, ball.rarity);
        _currentItemPrices[index] = price;
        var priceText = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        priceText.text = price.ToString();
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = ball.sprite;
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
        
        Utils.AddEventToObject(g, () => { 
            UIManager.Instance.ShowBallDescriptionWindow(ball, g);
        }, EventTriggerType.PointerEnter);
    }
    
    private void SetRelicEvent(GameObject g, RelicData relic, int index)
    {
        Utils.RemoveAllEventFromObject(g);
        
        var price = ContentProvider.GetSHopPrice(ShopItemType.Relic, relic.rarity);
        _currentItemPrices[index] = price;
        var priceText = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        priceText.text = price.ToString();
        
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = relic.sprite;
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
                    NotifyWindow.Instance.Notify("コインが足りません！", NotifyWindow.NotifyIconType.Error);
                    SeManager.Instance.PlaySe("error");
                }
            });
        }

        Utils.AddEventToObject(g, () => { 
            UIManager.Instance.ShowRelicDescriptionWindow(relic, g);
        }, EventTriggerType.PointerEnter);
    }
    
    private void OnClickRemoveButton()
    {
        var price = ContentProvider.GetBallRemovePrice();
        if (GameManager.Instance.Coin.Value < price)
        {
            NotifyWindow.Instance.Notify("コインが足りません！", NotifyWindow.NotifyIconType.Error);
            SeManager.Instance.PlaySe("error");
            return;
        }
        
        EventManager.OnBallRemove.Trigger(0);
        removeButton.transform.position = _disabledPosition;
        GameManager.Instance.SubCoin(price);
        InventoryManager.Instance.InventoryUI.StartEdit(InventoryUI.InventoryUIState.Remove);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        
        _itemObjects = itemContainer.GetComponentInChildren<Transform>().Cast<Transform>().Select(x => x.gameObject).ToList();
        _itemObjects.ForEach(x => _itemPositions.Add(x.transform.position));
        _itemPositions.Add(removeButton.transform.position);
        
        removeButton.GetComponent<Button>().onClick.AddListener(OnClickRemoveButton);
    }
}