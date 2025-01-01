using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Shop : MonoBehaviour
{
    private enum ShopState
    {
        NotSelected,
        Selected,
        Closed
    }
    public static Shop Instance;
    
    [SerializeField] private GameObject itemContainer;
    
    private readonly List<object> _currentItems = new();
    private const int ITEM_NUM = 6;
    private List<GameObject> _itemObjects;
    private ShopState _state = ShopState.Closed;
    private int _selectedItem = -1;
    private float _defaultScale = 1.0f;
    private readonly List<Vector3> _itemPositions = new();
    private readonly Vector3 _disabledPosition = new (100, 100, 0);
    private static RelicDataList AllRelics => RelicManager.Instance.allRelicDataList;
    private static BallDataList AllBalls => InventoryManager.Instance.allBallDataList;


    public void OpenShop(int count = 6)
    {
        if (count > ITEM_NUM) return;
        _state = ShopState.NotSelected;
        
        for (var i = 0; i < ITEM_NUM; i++)
        {
            _itemObjects[i].transform.position = _itemPositions[i];
        }
        _currentItems.Clear();
        
        for(var i = 0; i < ITEM_NUM; i++)
        {
            var balls = AllBalls.GetBallListExceptNormal();
            var isBall = GameManager.Instance.RandomRange(0.0f, 1.0f) > 0.5f;
            var index = GameManager.Instance.RandomRange(0, isBall ? balls.Count : AllRelics.list.Count);
            if (isBall)
            {
                _currentItems.Add(balls[index]);
                SetBallEvent(_itemObjects[i].transform.gameObject, balls[index], i);
            }
            else
            {
                _currentItems.Add(AllRelics.list[index]);
                SetRelicEvent(_itemObjects[i].transform.gameObject, AllRelics.list[index], i);
            }
        }
    }

    public void CloseShop()
    {
        _state = ShopState.Closed;
        _selectedItem = -1;
        for (var i = 0; i < ITEM_NUM; i++)
        {
            _itemObjects[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            _itemObjects[i].transform.DOScale(1, 0.1f).SetUpdate(true);
        }
        InventoryManager.Instance.InventoryUI.EnableCursor(false);
    }

    public void BuyBall(int inventoryIndex)
    {
        if(_selectedItem == -1) return;
        
        var ball = _currentItems[_selectedItem] as BallData;
        if (!ball) return;
        var itemPrice = ball.price;
        
        if (GameManager.Instance.coin.Value >= itemPrice)
        {
            InventoryManager.Instance.SetBall(ball, inventoryIndex + 1);
            _itemObjects[_selectedItem].transform.DOScale(_defaultScale, 0.1f).SetUpdate(true);
            InventoryManager.Instance.InventoryUI.EnableCursor(false);
            InventoryManager.Instance.InventoryUI.SetCursor(0);
            _itemObjects[_selectedItem].transform.position = _disabledPosition;
            
            GameManager.Instance.SubtractCoin(itemPrice);
            SeManager.Instance.PlaySe("coin");
            _selectedItem = -1;
        }
        else
        {
            SeManager.Instance.PlaySe("error");
        }
        _state = ShopState.NotSelected;
    }

    private void BuyRelic(int shopItemIndex)
    {
        if(_selectedItem == -1) return;

        var relic = _currentItems[_selectedItem] as RelicData;
        if (!relic) return;
        var itemPrice = relic.price;
        
        if (GameManager.Instance.coin.Value >= itemPrice)
        {
            RelicManager.Instance.AddRelic(relic);
            _itemObjects[_selectedItem].transform.DOScale(_defaultScale, 0.1f).SetUpdate(true);
            _itemObjects[_selectedItem].transform.position = _disabledPosition;
            
            GameManager.Instance.SubtractCoin(itemPrice);
            SeManager.Instance.PlaySe("coin");
            _selectedItem = -1;
        }
        else
        {
            SeManager.Instance.PlaySe("error");
        }
        _state = ShopState.NotSelected;
    }

    private void SetBallEvent(GameObject g, BallData ball, int index)
    {
        var price = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        price.text = ball.price.ToString();
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = ball.sprite;
        var ballImage = g.transform.Find("BallBase").GetComponent<Image>();
        ballImage.color = new Color(0.6f, 0.6f, 0.6f, 1);
        var button = g.GetComponent<Button>();
        _defaultScale = g.transform.localScale.x;
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                if (!ball) return;
                if (GameManager.Instance.coin.Value >= ball.price)
                {
                    _state = ShopState.Selected;
                    _selectedItem = index;
                    g.transform.DOScale(_defaultScale * 1.2f, 0.1f).SetUpdate(true);
                    InventoryManager.Instance.InventoryUI.EnableCursor(true);
                    SeManager.Instance.PlaySe("button");
                }
                else
                {
                    SeManager.Instance.PlaySe("error");
                }
            });
        }
        
        Utils.AddEventToObject(g, () => { 
            GameManager.Instance.UIManager.ShowBallDescriptionWindow(ball,
                g.transform.position + new Vector3(3f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(g, () => { 
            GameManager.Instance.UIManager.HideBallDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }
    
    private void SetRelicEvent(GameObject g, RelicData relic, int index)
    {
        Utils.RemoveAllEventFromObject(g);
        var price = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        price.text = relic.price.ToString();
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = relic.sprite;
        var ballImage = g.transform.Find("BallBase").GetComponent<Image>();
        ballImage.color = new Color(1, 1, 1, 0);
        var button = g.GetComponent<Button>();
        _defaultScale = g.transform.localScale.x;
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                InventoryManager.Instance.InventoryUI.EnableCursor(false);
                if (!relic) return;
                
                if (_selectedItem == index && _state == ShopState.Selected)
                {
                    BuyRelic(index);
                }
                else
                {
                    if (GameManager.Instance.coin.Value >= relic.price)
                    {
                        _state = ShopState.Selected;
                        _selectedItem = index;
                        g.transform.DOScale(_defaultScale * 1.2f, 0.1f).SetUpdate(true);
                        SeManager.Instance.PlaySe("button");
                    }
                    else
                    {
                        SeManager.Instance.PlaySe("error");
                    }
                }
            });
        }

        Utils.AddEventToObject(g, () => { 
            GameManager.Instance.UIManager.ShowRelicDescriptionWindow(relic,
            g.transform.position + new Vector3(3f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(g, () => { 
            GameManager.Instance.UIManager.HideRelicDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        
        _itemObjects = itemContainer.GetComponentInChildren<Transform>().Cast<Transform>().Select(x => x.gameObject).ToList();
        _itemObjects.ForEach(x => _itemPositions.Add(x.transform.position));
    }

    private void Update()
    {
        if(GameManager.Instance.state != GameManager.GameState.Event) return;
        if (_state == ShopState.NotSelected) return;

        for (var i = 0; i < ITEM_NUM; i++)
        {
            if (i != _selectedItem)
            {
                _itemObjects[i].transform.DOScale(_defaultScale, 0.1f).SetUpdate(true);
            }
        }
    }
}