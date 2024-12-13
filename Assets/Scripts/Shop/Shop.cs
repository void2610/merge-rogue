using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Shop : MonoBehaviour
{
    private enum ShopState
    {
        NotSelected,
        Selected,
        Closed
    }
    public static Shop Instance;
    
    [SerializeField] private RelicDataList allRelicDataList;
    [SerializeField] private BallDataList allBallDataList;
    [SerializeField] private GameObject itemContainer;
    
    private readonly List<object> currentItems = new();
    private const int ITEM_NUM = 6;
    private List<GameObject> itemObjects;
    private ShopState state = ShopState.Closed;
    private int selectedItem = -1;
    private float defaultScale = 1.0f;
    private List<Vector3> itemPositions = new();
    private Vector3 disabledPosition = new Vector3(100, 100, 0);

    public void OpenShop(int count = 6)
    {
        if (count > ITEM_NUM) return;
        state = ShopState.NotSelected;
        
        for (var i = 0; i < ITEM_NUM; i++)
        {
            itemObjects[i].transform.position = itemPositions[i];
        }
        currentItems.Clear();
        
        for(var i = 0; i < ITEM_NUM; i++)
        {
            var balls = allBallDataList.GetBallListExceptNormal();
            var isBall = GameManager.Instance.RandomRange(0.0f, 1.0f) > 0.5f;
            var index = GameManager.Instance.RandomRange(0, isBall ? balls.Count : allRelicDataList.list.Count);
            if (isBall)
            {
                currentItems.Add(balls[index]);
                SetBallEvent(itemObjects[i].transform.gameObject, balls[index], i);
            }
            else
            {
                currentItems.Add(allRelicDataList.list[index]);
                SetRelicEvent(itemObjects[i].transform.gameObject, allRelicDataList.list[index], i);
            }
        }
    }

    public void CloseShop()
    {
        state = ShopState.Closed;
        selectedItem = -1;
        for (var i = 0; i < ITEM_NUM; i++)
        {
            itemObjects[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            itemObjects[i].transform.DOScale(1, 0.1f).SetUpdate(true);
        }
        InventoryManager.Instance.inventoryUI.EnableCursor(false);
    }

    public void BuyBall(int inventoryIndex)
    {
        if(selectedItem == -1) return;
        
        var ball = currentItems[selectedItem] as BallData;
        if (ball == null) return;
        var itemPrice = ball.price;
        
        if (GameManager.Instance.coin.Value >= itemPrice)
        {
            InventoryManager.Instance.SetBall(ball, inventoryIndex + 1);
            itemObjects[selectedItem].transform.DOScale(defaultScale, 0.1f).SetUpdate(true);
            InventoryManager.Instance.inventoryUI.EnableCursor(false);
            InventoryManager.Instance.inventoryUI.SetCursor(0);
            itemObjects[selectedItem].transform.position = disabledPosition;
            
            GameManager.Instance.SubtractCoin(itemPrice);
            SeManager.Instance.PlaySe("coin");
            selectedItem = -1;
        }
        else
        {
            SeManager.Instance.PlaySe("error");
        }
        state = ShopState.NotSelected;
    }

    private void BuyRelic(int shopItemIndex)
    {
        if(selectedItem == -1) return;

        var relic = currentItems[selectedItem] as RelicData;
        if (!relic) return;
        var itemPrice = relic.price;
        
        if (GameManager.Instance.coin.Value >= itemPrice)
        {
            RelicManager.Instance.AddRelic(relic);
            itemObjects[selectedItem].transform.DOScale(defaultScale, 0.1f).SetUpdate(true);
            itemObjects[selectedItem].transform.position = disabledPosition;
            
            GameManager.Instance.SubtractCoin(itemPrice);
            SeManager.Instance.PlaySe("coin");
            selectedItem = -1;
        }
        else
        {
            SeManager.Instance.PlaySe("error");
        }
        state = ShopState.NotSelected;
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
        defaultScale = g.transform.localScale.x;
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                if (!ball) return;
                if (GameManager.Instance.coin.Value >= ball.price)
                {
                    state = ShopState.Selected;
                    selectedItem = index;
                    g.transform.DOScale(defaultScale * 1.2f, 0.1f).SetUpdate(true);
                    InventoryManager.Instance.inventoryUI.EnableCursor(true);
                    SeManager.Instance.PlaySe("button");
                }
                else
                {
                    SeManager.Instance.PlaySe("error");
                }
            });
        }
        
        Utils.AddEventToObject(g, () => { 
            GameManager.Instance.uiManager.ShowBallDescriptionWindow(ball,
                g.transform.position + new Vector3(3f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(g, () => { 
            GameManager.Instance.uiManager.HideBallDescriptionWindow();
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
        defaultScale = g.transform.localScale.x;
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                InventoryManager.Instance.inventoryUI.EnableCursor(false);
                if (!relic) return;
                
                if (selectedItem == index && state == ShopState.Selected)
                {
                    BuyRelic(index);
                }
                else
                {
                    if (GameManager.Instance.coin.Value >= relic.price)
                    {
                        state = ShopState.Selected;
                        selectedItem = index;
                        g.transform.DOScale(defaultScale * 1.2f, 0.1f).SetUpdate(true);
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
            GameManager.Instance.uiManager.ShowRelicDescriptionWindow(relic,
            g.transform.position + new Vector3(3f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(g, () => { 
            GameManager.Instance.uiManager.HideRelicDescriptionWindow();
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
        
        itemObjects = itemContainer.GetComponentInChildren<Transform>().Cast<Transform>().Select(x => x.gameObject).ToList();
        itemObjects.ForEach(x => itemPositions.Add(x.transform.position));
    }

    private void Update()
    {
        if (state == ShopState.NotSelected) return;

        for (var i = 0; i < ITEM_NUM; i++)
        {
            if (i == selectedItem)
            {
                itemObjects[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
            else
            {
                itemObjects[i].GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1);
                if (itemObjects[i].transform.localScale.x > defaultScale)
                    itemObjects[i].transform.DOScale(defaultScale, 0.1f).SetUpdate(true);
            }
        }
    }
}