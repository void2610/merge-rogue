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
    public static Shop Instance;
    
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private GameObject removeButton;
    
    private readonly List<object> _currentItems = new();
    private const int ITEM_NUM = 6;
    private List<GameObject> _itemObjects;
    private readonly List<Vector3> _itemPositions = new();
    private readonly Vector3 _disabledPosition = new (100, 100, 0);
    private static RelicDataList AllRelics => RelicManager.Instance.allRelicDataList;
    private static BallDataList AllBalls => InventoryManager.Instance.allBallDataList;


    public void OpenShop(int count = 6)
    {
        if (count > ITEM_NUM) return;
        
        for (var i = 0; i < ITEM_NUM; i++)
        {
            _itemObjects[i].transform.position = _itemPositions[i];
        }
        removeButton.transform.position = _itemPositions[ITEM_NUM];
        
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
        GameManager.Instance.SubCoin(ball.price);
        SeManager.Instance.PlaySe("coin");
    }

    private void BuyRelic(int index)
    {
        var relic = _currentItems[index] as RelicData;
        if (!relic) return;
        var itemPrice = relic.price;
    
        RelicManager.Instance.AddRelic(relic);
        _itemObjects[index].transform.position = _disabledPosition;
        GameManager.Instance.SubCoin(itemPrice);
        SeManager.Instance.PlaySe("coin");
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
        if (button)
        {
            Utils.RemoveAllEventFromObject(g);
            button.onClick.AddListener(() =>
            {
                if (!ball) return;
                if (GameManager.Instance.Coin.Value >= ball.price) BuyBall(index);
                else SeManager.Instance.PlaySe("error");
            });
        }
        
        Utils.AddEventToObject(g, () => { 
            UIManager.Instance.ShowBallDescriptionWindow(ball, g);
        }, EventTriggerType.PointerEnter);
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
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                if (!relic) return;
                if (GameManager.Instance.Coin.Value >= relic.price) BuyRelic(index);
                else SeManager.Instance.PlaySe("error");
            });
        }

        Utils.AddEventToObject(g, () => { 
            UIManager.Instance.ShowRelicDescriptionWindow(relic, g);
        }, EventTriggerType.PointerEnter);
    }
    
    private void OnClickRemoveButton()
    {
        if (GameManager.Instance.Coin.Value < 25)
        {
            SeManager.Instance.PlaySe("error");
            return;
        }
        
        EventManager.OnBallRemove.Trigger(0);
        removeButton.transform.position = _disabledPosition;
        GameManager.Instance.SubCoin(25);
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

    private void Update()
    {
        if(GameManager.Instance.state != GameManager.GameState.Event) return;
    }
}