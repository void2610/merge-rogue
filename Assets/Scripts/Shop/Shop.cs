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
    
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private GameObject removeButton;
    
    private readonly List<object> _currentItems = new();
    private readonly List<int> _currentItemPrices = new();
    private const int ITEM_NUM = 6;
    private List<GameObject> _itemObjects;
    private int _selectedIndex = -1;
    
    public void UnInteractableSelectedItem() => _itemObjects[_selectedIndex].GetComponent<Button>().interactable = false;
    public void SetRemoveButtonInteractable(bool b) => removeButton.GetComponent<Button>().interactable = b;
    
    public void OpenShop(int count = 6)
    {
        if (count > ITEM_NUM) throw new System.Exception("Invalid count");
        
        for (var i = 0; i < ITEM_NUM; i++) _itemObjects[i].GetComponent<Button>().interactable = true;

        removeButton.GetComponent<Button>().interactable = true;
        removeButton.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = ContentProvider.GetBallRemovePrice().ToString();
        
        _currentItems.Clear();
        _currentItemPrices.Clear();
        _currentItemPrices.AddRange(Enumerable.Repeat(0, ITEM_NUM));
        
        for(var i = 0; i < ITEM_NUM; i++)
        {
            var balls = ContentProvider.Instance.GetBallListExceptNormal();
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
        InventoryUI.Instance.StartEditReplace(ball);
        UIManager.Instance.ResetSelectedGameObject();
    }

    private void BuyRelic(int index)
    {
        var relic = _currentItems[index] as RelicData;
        if (!relic) return;
        RelicManager.Instance.AddRelic(relic);
        _itemObjects[index].GetComponent<Button>().interactable = false;
        GameManager.Instance.SubCoin(_currentItemPrices[index]);
        UIManager.Instance.ResetSelectedGameObject();
    }

    private void SetBallEvent(GameObject g, BallData ball, int index)
    {
        var price = ContentProvider.GetSHopPrice(ShopItemType.Ball, ball.rarity);
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
                    SelectionCursor.SetSelectedGameObjectSafe(g);
                }
            });
        }
        
        g.AddDescriptionWindowEvent(ball);
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
        image.color = new Color(1, 1, 1, 1);
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
                    SelectionCursor.SetSelectedGameObjectSafe(g);
                }
            });
        }

        g.AddDescriptionWindowEvent(relic);
    }
    
    private void OnClickRemoveButton()
    {
        var price = ContentProvider.GetBallRemovePrice();
        if (GameManager.Instance.Coin.Value < price)
        {
            NotifyWindow.Instance.Notify("コインが足りません！", NotifyWindow.NotifyIconType.Error);
            SeManager.Instance.PlaySe("error");
            SelectionCursor.SetSelectedGameObjectSafe(removeButton);
            return;
        }
        
        EventManager.OnBallRemove.Trigger(0);
        InventoryUI.Instance.StartEdit(InventoryUI.InventoryUIState.Remove);
    }
    
    private void OnClickSkipButton() => CloseShop();

    private void Awake()
    {
        _itemObjects = itemContainer.GetComponentInChildren<Transform>().Cast<Transform>().Select(x => x.gameObject).ToList();
        
        skipButton.GetComponent<Button>().onClick.AddListener(OnClickSkipButton);
        removeButton.GetComponent<Button>().onClick.AddListener(OnClickRemoveButton);
    }
}