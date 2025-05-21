using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class InventoryUI : SingletonMonoBehaviour<InventoryUI>
{
    public enum InventoryUIState
    {
        Disabled,
        Replace,
        Upgrade,
        Swap,
        Remove
    }
    
    [SerializeField] private GameObject ballUIPrefab;
    [SerializeField] private Shop shop;
    [SerializeField] private Vector3 inventoryPosition;
    [SerializeField] private GameObject inventoryUIContainer;
    [SerializeField] private GameObject subCursor;
    [SerializeField] private ConfirmationDialog dialog;
    private const float SIZE_COEFFICIENT = 8f;
    private static List<float> BallSizes => InventoryManager.Instance.Sizes;
    private readonly List<GameObject> _items = new();
    private int _selectedIndex = -1;
    private int _swapIndex = -1;
    private InventoryUIState _state = InventoryUIState.Disabled;
    private BallData _replaceBallData;
    
    public void ConductRelace() => InventoryManager.Instance.ReplaceBall(_replaceBallData, _selectedIndex);
    
    public async UniTask ConductSwap() => await InventoryManager.Instance.SwapBall(_selectedIndex, _swapIndex);
    
    public void ConductRemove()
    {
        InventoryManager.Instance.RemoveAndShiftBall(_selectedIndex);
        shop.EnableSkipButton(true);
    }

    public void ConductUpgrade() => InventoryManager.Instance.UpgradeBall(_selectedIndex);

    public void CreateBallUI(GameObject ball, int rank, BallBase ballBase)
    {
        var g = Instantiate(ballUIPrefab, inventoryUIContainer.transform);
        
        g.transform.position = CalcInventoryPosition(rank);
        g.transform.localScale = GetBallScale(rank);
        
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
        g.transform.localScale = GetBallScale(startRank);
        
        g.GetComponent<Image>().sprite = ballBase.Data.sprite;
        
        SetEvent(g, endRank, ballBase);
        Destroy(_items[endRank]);
        _items[endRank] = g;

        g.transform.DOMove(CalcInventoryPosition(endRank), 2.0f).SetEase(Ease.OutQuint).Forget();
        await g.transform.DOScale(GetBallScale(endRank), 2.0f).SetEase(Ease.OutQuint);
    }
    
    private Vector3 GetBallScale(int index)
    {
        return new Vector3(BallSizes[index], BallSizes[index], 1) * 0.5f;
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
        if (s == InventoryUIState.Replace) throw new System.ArgumentException("Replace state should be set with StartEditReplace");
        _state = s;
        UIManager.Instance.LockCursorToInventory(true);
    }
    
    public void StartEditReplace(BallData ballData)
    {
        _state = InventoryUIState.Replace;
        _replaceBallData = ballData;
        UIManager.Instance.LockCursorToInventory(true);
    }
    
    private void SetEvent(GameObject g, int index, BallBase ballBase)
    {
        // クリックでボールの選択、入れ替え、削除
        g.GetComponent<Button>().onClick.AddListener(() => OnClickBall(index));
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
        UIManager.Instance.LockCursorToInventory(false);
    }

    private void OnClickBall(int index)
    {
        _selectedIndex = index;
        SeManager.Instance.PlaySe("button");
        switch (_state)
        {
            case InventoryUIState.Disabled:
                break;
            case InventoryUIState.Replace:
                CancelEdit();
                dialog.OpenDialog(InventoryUIState.Replace, InventoryManager.Instance.GetBallData(index), _replaceBallData);
                break;
            case InventoryUIState.Upgrade:
                CancelEdit();
                if (InventoryManager.Instance.GetBallLevel(index) >= 2)
                {
                    SeManager.Instance.PlaySe("error");
                    NotifyWindow.Instance.Notify("これ以上ボールを強化できません", NotifyWindow.NotifyIconType.Error);
                    return;
                }
                dialog.OpenDialog(InventoryUIState.Upgrade, InventoryManager.Instance.GetBallData(index), null);
                break;
            case InventoryUIState.Swap when _swapIndex == -1:
                _swapIndex = index;
                subCursor.GetComponent<Image>().enabled = true;
                SetSubCursor(index);
                break;
            case InventoryUIState.Swap:
                CancelEdit();
                dialog.OpenDialog(InventoryUIState.Swap, InventoryManager.Instance.GetBallData(index), InventoryManager.Instance.GetBallData(_swapIndex));
                _swapIndex = -1;
                subCursor.GetComponent<Image>().enabled = false;
                break;
            case InventoryUIState.Remove:
                dialog.OpenDialog(InventoryUIState.Remove, InventoryManager.Instance.GetBallData(index), null);
                CancelEdit();
                break;
        }
    }
    
    private Vector3 CalcInventoryPosition(int index)
    {
        return inventoryPosition + new Vector3(index * (0.6f + BallSizes[index] * 0.3f), 0, 0);
    }

    protected override void Awake()
    {
        base.Awake();
        subCursor.GetComponent<Image>().enabled = false;
    }
}
