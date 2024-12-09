using System.Collections.Generic;
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

    [SerializeField] private BallDataList allBallDataList;
    [SerializeField] private RelicDataList allRelicDataList;

    private List<object> currentItems = new();
    private const int ITEM_NUM = 6;
    private Transform itemContainer => this.transform.Find("Items");
    private ShopState state = ShopState.Closed;
    private int selectedItem = -1;
    private float defaultScale = 1.0f;

    public void OpenShop(int count = 6)
    {
        if (count > ITEM_NUM) return;
        state = ShopState.NotSelected;
        currentItems.Clear();
        
        for(var i = 0; i < ITEM_NUM; i++)
        {
            var isBall = GameManager.Instance.RandomRange(0.0f, 1.0f) > 0.5f;
            var index = GameManager.Instance.RandomRange(0, isBall ? allBallDataList.ballsExceptNormal.Count : allRelicDataList.list.Count);
            if (isBall)
            {
                currentItems.Add(allBallDataList.ballsExceptNormal[index]);
                SetBallEvent(itemContainer.GetChild(i).gameObject, allBallDataList.ballsExceptNormal[index], i);
            }
            else
            {
                currentItems.Add(allRelicDataList.list[index]);
                SetRelicEvent(itemContainer.GetChild(i).gameObject, allRelicDataList.list[index], i);
            }
        }
    }

    public void CloseShop()
    {
        state = ShopState.Closed;
        selectedItem = -1;
        for (int i = 0; i < ITEM_NUM; i++)
        {
            itemContainer.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            itemContainer.GetChild(i).DOScale(1, 0.1f).SetUpdate(true);
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
            itemContainer.GetChild(selectedItem).DOScale(defaultScale, 0.1f).SetUpdate(true);
            InventoryManager.Instance.inventoryUI.EnableCursor(false);
            InventoryManager.Instance.inventoryUI.SetCursor(0);
            
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
            itemContainer.GetChild(selectedItem).DOScale(defaultScale, 0.1f).SetUpdate(true);
            
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
    }

    private void Update()
    {
        if (state == ShopState.NotSelected) return;

        for (int i = 0; i < ITEM_NUM; i++)
        {
            if (i == selectedItem)
            {
                itemContainer.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
            else
            {
                itemContainer.GetChild(i).GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1);
                if (itemContainer.GetChild(i).localScale.x > defaultScale)
                {
                    itemContainer.GetChild(i).DOScale(defaultScale, 0.1f).SetUpdate(true);
                }
            }
        }
    }
}