using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject ballUIPrefab;
    [SerializeField] private Vector3 inventoryPosition;
    [SerializeField] private GameObject inventoryUIContainer;
    [SerializeField] private GameObject cursor;
    private List<GameObject> items = new List<GameObject>();
    private const float SIZE_COEFFICIENT = 8f;
    private static List<float> ballSizes => InventoryManager.Instance.sizes;

    public void CreateBallUI(GameObject ball, int level, BallData data)
    {
        var g = Instantiate(ballUIPrefab, inventoryUIContainer.transform);
        
        g.transform.position = CalcInventoryPosition(level);
        g.transform.localScale = ball.transform.localScale * 1;
        
        var color = ball.GetComponent<SpriteRenderer>().color;
        g.GetComponent<Image>().color = color;
        
        var sprite = ball.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite;
        if (sprite != null) g.transform.Find("Icon").GetComponent<Image>().sprite = sprite;
        else g.transform.Find("Icon").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        
        SetCursorEvent(g, level);
        SetDescriptionWindowEvent(g, data);
        if (items.Count <= level)
        {
            items.Add(g);
        }
        else
        {
            Destroy(items[level]);
            items[level] = g;
        }
    }

    public void SetCursor(int index)
    {
        if (items.Count == 0 || index < 0 || index >= items.Count) return;
        cursor.transform.DOMove(items[index].transform.position, 0.3f).SetUpdate(true).SetEase(Ease.OutQuint);
        var size = items[index].transform.localScale.x * SIZE_COEFFICIENT;
        cursor.transform.DOScale(new Vector3(size, size, 1), 0.3f).SetUpdate(true).SetEase(Ease.OutQuint);
    }

    public void EnableCursor(bool b)
    {
        cursor.GetComponent<SpriteRenderer>().enabled = b;
    }
    
    private void SetCursorEvent(GameObject ball, int index)
    {
        Utils.AddEventToObject(ball, () => { SetCursor(index); }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(ball, () => { Shop.Instance.BuyBall(index); }, EventTriggerType.PointerClick);
    }

    private void SetDescriptionWindowEvent(GameObject g, BallData data)
    {
        Utils.AddEventToObject(g, () => { GameManager.Instance.uiManager.ShowBallDescriptionWindow(data, g.transform.position + new Vector3(2.5f, 0, 0)); }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(g, () => { GameManager.Instance.uiManager.HideBallDescriptionWindow(); }, EventTriggerType.PointerExit);
    }
    
    private Vector3 CalcInventoryPosition(int index)
    {
        return inventoryPosition + new Vector3(index * (0.6f + ballSizes[index] * 0.5f), 0, 0);
    }

    private void Awake()
    {
        EnableCursor(false);
    }
}
