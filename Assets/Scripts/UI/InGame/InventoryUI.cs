using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public enum InventoryUIState
    {
        Disabled,
        Upgrade,
        Swap,
        Remove
    }
    
    [SerializeField] private GameObject ballUIPrefab;
    [SerializeField] private Vector3 inventoryPosition;
    [SerializeField] private GameObject inventoryUIContainer;
    [SerializeField] private GameObject cursor;
    [SerializeField] private GameObject subCursor;
    [SerializeField] private UpgradeConfirmPanel upgradeConfirmPanel;
    private const float SIZE_COEFFICIENT = 8f;
    private static List<float> BallSizes => InventoryManager.Instance.Sizes;
    private readonly List<GameObject> _items = new();
    private int _swapIndex = -1;
    private InventoryUIState _state = InventoryUIState.Disabled;

    public void CreateBallUI(GameObject ball, int level, BallBase ballBase)
    {
        var g = Instantiate(ballUIPrefab, inventoryUIContainer.transform);
        
        g.transform.position = CalcInventoryPosition(level);
        g.transform.localScale = new Vector3(BallSizes[level], BallSizes[level], 1);
        
        var color = ball.GetComponent<SpriteRenderer>().color;
        g.GetComponent<Image>().color = color;
        
        var sprite = ball.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite;
        if (sprite != null) g.transform.Find("Icon").GetComponent<Image>().sprite = sprite;
        else g.transform.Find("Icon").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        
        SetEvent(g, level, ballBase);
        if (_items.Count <= level)
        {
            _items.Add(g);
        }
        else
        {
            Destroy(_items[level]);
            _items[level] = g;
        }
    }
    
    public void RemoveBallUI(int level)
    {
        if (level < 0 || level >= _items.Count) return;
        Destroy(_items[level]);
        _items.RemoveAt(level);
    }

    public void SetCursor(int index)
    {
        if (_items.Count == 0 || index < 0 || index >= _items.Count) return;
        cursor.transform.DOMove(_items[index].transform.position, 0.3f).SetUpdate(true).SetEase(Ease.OutQuint);
        var size = _items[index].transform.localScale.x * SIZE_COEFFICIENT;
        cursor.transform.DOScale(new Vector3(size, size, 1), 0.3f).SetUpdate(true).SetEase(Ease.OutQuint);
    }
    
    private void SetSubCursor(int index)
    {
        if (_items.Count == 0 || index < 0 || index >= _items.Count) return;
        subCursor.transform.position = _items[index].transform.position;
        var size = _items[index].transform.localScale.x * SIZE_COEFFICIENT;
        subCursor.transform.localScale = new Vector3(size, size, 1);
    }

    public void EnableCursor(bool b)
    {
        cursor.GetComponent<SpriteRenderer>().enabled = b;
    }
    
    public void StartEdit(InventoryUIState s)
    {
        if (s == InventoryUIState.Disabled) return;
        EnableCursor(true);
        SetCursor(0);
        _state = s;
    }
    
    private void SetEvent(GameObject ballObj, int index, BallBase ballBase)
    {
        // クリックでボールの選択、入れ替え、削除
        Utils.AddEventToObject(ballObj, () => OnClickBall(index), EventTriggerType.PointerClick);
        
        // マウスオーバーでカーソル移動とウィンドウ表示 (とアニメーション)
        Utils.AddEventToObject(ballObj, () => { 
            var rt = ballObj.GetComponent<RectTransform>();
            rt.DORotate(new Vector3(0, 0, 15), 0.75f).SetEase(Ease.Flash, 10, 0.9f).OnComplete(() => rt.DORotate(Vector3.zero, 0.1f));
            
            SetCursor(index);
            UIManager.Instance.ShowBallDescriptionWindow(ballBase.Data, ballObj, ballBase.Rank); 
        }, EventTriggerType.PointerEnter);
    }

    private void OnClickBall(int index)
    {
        switch (_state)
        {
            case InventoryUIState.Disabled:
                break;
            case InventoryUIState.Upgrade:
                upgradeConfirmPanel.OpenUpgradeConfirmPanel(index);
                EnableCursor(false);
                _state = InventoryUIState.Disabled;
                break;
            case InventoryUIState.Swap when _swapIndex == -1:
                _swapIndex = index;
                subCursor.GetComponent<SpriteRenderer>().enabled = true;
                SetSubCursor(index);
                break;
            case InventoryUIState.Swap:
                InventoryManager.Instance.SwapBall(_swapIndex, index);
                
                GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
                EnableCursor(false);
                subCursor.GetComponent<SpriteRenderer>().enabled = false;
                _swapIndex = -1;
                _state = InventoryUIState.Disabled;
                break;
            case InventoryUIState.Remove:
                InventoryManager.Instance.RemoveAndShiftBall(index);
                EnableCursor(false);
                _state = InventoryUIState.Disabled;
                break;
        }
    }
    
    private Vector3 CalcInventoryPosition(int index)
    {
        return inventoryPosition + new Vector3(index * (0.6f + BallSizes[index] * 0.4f), 0, 0);
    }

    private void Awake()
    {
        EnableCursor(false);
        subCursor.GetComponent<SpriteRenderer>().enabled = false;
    }
}
