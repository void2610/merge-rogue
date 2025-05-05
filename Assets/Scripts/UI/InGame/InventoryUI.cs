using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
    [SerializeField] private Shop shop;
    [SerializeField] private Vector3 inventoryPosition;
    [SerializeField] private GameObject inventoryUIContainer;
    [SerializeField] private GameObject subCursor;
    [SerializeField] private UpgradeConfirmPanel upgradeConfirmPanel;
    private const float SIZE_COEFFICIENT = 8f;
    private static List<float> BallSizes => InventoryManager.Instance.Sizes;
    private readonly List<GameObject> _items = new();
    private int _swapIndex = -1;
    private InventoryUIState _state = InventoryUIState.Disabled;

    public void CreateBallUI(GameObject ball, int rank, BallBase ballBase)
    {
        var g = Instantiate(ballUIPrefab, inventoryUIContainer.transform);
        
        g.transform.position = CalcInventoryPosition(rank);
        g.transform.localScale = new Vector3(BallSizes[rank], BallSizes[rank], 1);
        
        g.GetComponent<Image>().sprite = ballBase.Data.sprite;
        
        SetEvent(g, rank, ballBase);
        if (_items.Count <= rank)
        {
            _items.Add(g);
        }
        else
        {
            Destroy(_items[rank]);
            _items[rank] = g;
        }
    }
    
    public async UniTask CreateBallUITween(GameObject ball, int startRank, int endRank, BallBase ballBase)
    {
        var g = Instantiate(ballUIPrefab, inventoryUIContainer.transform);
        
        g.transform.position = CalcInventoryPosition(startRank);
        g.transform.localScale = new Vector3(BallSizes[startRank], BallSizes[startRank], 1);
        
        g.GetComponent<Image>().sprite = ballBase.Data.sprite;
        
        SetEvent(g, endRank, ballBase);
        Destroy(_items[endRank]);
        _items[endRank] = g;

        g.transform.DOMove(CalcInventoryPosition(endRank), 2.0f).SetEase(Ease.OutQuint).Forget();
        await g.transform.DOScale(new Vector3(BallSizes[endRank], BallSizes[endRank], 1), 2.0f).SetEase(Ease.OutQuint);
    }
    
    public void RemoveBallUI(int level)
    {
        if (level < 0 || level >= _items.Count) return;
        Destroy(_items[level]);
        _items.RemoveAt(level);
    }
    
    private void SetSubCursor(int index)
    {
        if (_items.Count == 0 || index < 0 || index >= _items.Count) return;
        subCursor.transform.position = _items[index].transform.position;
        var size = _items[index].transform.localScale.x * SIZE_COEFFICIENT;
        subCursor.transform.localScale = new Vector3(size, size, 1);
    }
    
    public void StartEdit(InventoryUIState s)
    {
        if (s == InventoryUIState.Disabled) return;
        _state = s;
        UIManager.Instance.ToggleCursorState(CursorStateType.Ball);
    }
    
    private void SetEvent(GameObject g, int index, BallBase ballBase)
    {
        // クリックでボールの選択、入れ替え、削除
        g.GetComponent<Button>().onClick.AddListener(() => OnClickBall(index).Forget());
        g.RemoveAllEventTrigger();
        g.AddDescriptionWindowEvent(ballBase.Data, ballBase.Level);
        
        // ナビゲーションを手動設定
        var nav = g.GetComponent<Button>().navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnDown = null;
        nav.selectOnUp = null;
        if (index <= 0) nav.selectOnLeft = null;
        else nav.selectOnLeft = _items[index - 1].GetComponent<Button>();
        if (index >= _items.Count) nav.selectOnRight = null;
        else nav.selectOnRight = _items[index].GetComponent<Button>();
    }

    public void CancelEdit()
    {
        _state = InventoryUIState.Disabled;
        UIManager.Instance.ToggleCursorState(CursorStateType.Merge);
    }

    private async UniTaskVoid OnClickBall(int index)
    {
        switch (_state)
        {
            case InventoryUIState.Disabled:
                break;
            case InventoryUIState.Upgrade:
                CancelEdit();
                if (InventoryManager.Instance.GetBallLevel(index) >= 2)
                {
                    SeManager.Instance.PlaySe("error");
                    NotifyWindow.Instance.Notify("これ以上ボールを強化できません", NotifyWindow.NotifyIconType.Error);
                    return;
                }
                SeManager.Instance.PlaySe("button");
                upgradeConfirmPanel.OpenUpgradeConfirmPanel(index);
                break;
            case InventoryUIState.Swap when _swapIndex == -1:
                SeManager.Instance.PlaySe("button");
                _swapIndex = index;
                subCursor.GetComponent<Image>().enabled = true;
                SetSubCursor(index);
                break;
            case InventoryUIState.Swap:
                SeManager.Instance.PlaySe("button");
                subCursor.GetComponent<Image>().enabled = false;
                await InventoryManager.Instance.SwapBall(_swapIndex, index);
                GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
                _swapIndex = -1;
                CancelEdit();
                break;
            case InventoryUIState.Remove:
                SeManager.Instance.PlaySe("button");
                InventoryManager.Instance.RemoveAndShiftBall(index);
                CancelEdit();
                shop.EnableSkipButton(true);
                UIManager.Instance.ResetSelectedGameObject();
                break;
        }
    }
    
    private Vector3 CalcInventoryPosition(int index)
    {
        return inventoryPosition + new Vector3(index * (0.6f + BallSizes[index] * 0.4f), 0, 0);
    }

    private void Awake()
    {
        subCursor.GetComponent<Image>().enabled = false;
    }
}
