using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using VContainer;

public class InventoryUI : SingletonMonoBehaviour<InventoryUI>
{
    public enum InventoryUIState
    {
        Disabled,
        Replace,
        Upgrade,
        Swap,
        Remove,
        Add
    }
    
    [SerializeField] private GameObject ballUIPrefab;
    [SerializeField] private GameObject emptySlotPrefab;
    [SerializeField] private Shop shop;
    [SerializeField] private Vector3 inventoryPosition;
    [SerializeField] private GameObject inventoryUIContainer;
    [SerializeField] private GameObject subCursor;
    [SerializeField] private ConfirmationDialog dialog;
    [SerializeField] private AfterBattleUI afterBattleUI;
    
    private const float SIZE_COEFFICIENT = 8f;
    private List<float> BallSizes => _inventoryService?.Sizes ?? new List<float>();
    
    private IContentService _contentService;
    private IInventoryService _inventoryService;
    
    [Inject]
    public void InjectDependencies(IContentService contentService, IInventoryService inventoryService)
    {
        _contentService = contentService;
        _inventoryService = inventoryService;
    }
    
    private readonly List<GameObject> _items = new();
    private int _selectedIndex = -1;
    private int _swapIndex = -1;
    private InventoryUIState _state = InventoryUIState.Disabled;
    private BallData _targetBallData;

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
    
    public void CreateEmptySlotUI(int index)
    {
        var g = Instantiate(emptySlotPrefab, inventoryUIContainer.transform);
        
        g.transform.position = CalcInventoryPosition(index);
        g.transform.localScale = GetBallScale(index);
        
        SetEmptySlotEvent(g, index);
        if (_items.Count <= index)
        {
            _items.Add(g);
        }
        else
        {
            if (_items[index]) Destroy(_items[index]);
            _items[index] = g;
        }
    }
    
    public void InitializeEmptySlots()
    {
        // 既存の空きスロットを全て削除
        for (var i = _inventoryService.InventorySize; i < _items.Count; i++)
        {
            if (_items[i])
            {
                Destroy(_items[i]);
                _items[i] = null;
            }
        }
        
        // Replace状態またはAdd状態の時のみ空きスロットを表示
        if ((_state == InventoryUIState.Replace || _state == InventoryUIState.Add) && _inventoryService.InventorySize < 10)
        {
            CreateEmptySlotUI(_inventoryService.InventorySize);
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
        _targetBallData = ballData;
        UIManager.Instance.LockCursorToInventory(true);
        InitializeEmptySlots();
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
    
    private void SetEmptySlotEvent(GameObject g, int index)
    {
        // 空きスロットクリックでボールの追加
        var button = g.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnClickEmptySlot(index).Forget());
        g.RemoveAllEventTrigger();
        
        // ナビゲーションを手動設定
        var nav = button.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnDown = null;
        nav.selectOnUp = null;
        if (index <= 0) nav.selectOnLeft = null;
        else if (index - 1 < _items.Count && _items[index - 1]) nav.selectOnLeft = _items[index - 1].GetComponent<Button>();
        if (index >= _items.Count - 1) nav.selectOnRight = null;
        else if (index + 1 < _items.Count && _items[index + 1]) nav.selectOnRight = _items[index + 1].GetComponent<Button>();
    }

    public void CancelEdit()
    {
        _state = InventoryUIState.Disabled;
        UIManager.Instance.LockCursorToInventory(false);
        _swapIndex = -1;
        subCursor.GetComponent<Image>().enabled = false;
        
        // 編集キャンセル時に空きスロットを削除
        InitializeEmptySlots();
    }

    private async UniTaskVoid OnClickBall(int index)
    {
        if (_state != InventoryUIState.Swap || _swapIndex != -1)
            UIManager.Instance.LockCursorToInventory(false);
            
        _selectedIndex = index;
        SeManager.Instance.PlaySe("button");
        var res = false;
        switch (_state)
        {
            case InventoryUIState.Disabled:
                break;
            case InventoryUIState.Replace:
                res = await dialog.OpenDialog(InventoryUIState.Replace, _inventoryService.GetBallData(index), _inventoryService.GetBallLevel(index), _targetBallData);
                if (res)
                {
                    _inventoryService.ReplaceBall(_targetBallData, _selectedIndex + 1);
                    GameManager.Instance.SubCoin(_contentService.GetShopPrice(Shop.ShopItemType.Ball, _targetBallData.rarity));
                    
                    if (GameManager.Instance.state == GameManager.GameState.AfterBattle)
                        afterBattleUI.UnInteractableSelectedItem();
                    else
                        shop.UnInteractableSelectedItem();
                }
                CancelEdit();
                break;
            case InventoryUIState.Upgrade:
                if (_inventoryService.GetBallLevel(index) >= 2)
                {
                    SeManager.Instance.PlaySe("error");
                    NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.CantUpgradeBall);
                    return;
                }
                
                res = await dialog.OpenDialog(InventoryUIState.Upgrade, _inventoryService.GetBallData(index), _inventoryService.GetBallLevel(index), null);
                if (res)
                { 
                    _inventoryService.UpgradeBall(_selectedIndex);
                    SeManager.Instance.PlaySe("levelUp");
                    afterBattleUI.SetUpgradeButtonInteractable(false);
                    GameManager.Instance.SubCoin(_contentService.GetBallUpgradePrice());
                }
                CancelEdit();
                break;
            case InventoryUIState.Swap when _swapIndex == -1:
                // 交換元のボール選択なので、これだけ
                _swapIndex = index;
                subCursor.GetComponent<Image>().enabled = true;
                SetSubCursor(index);
                break;
            case InventoryUIState.Swap:
                if (_swapIndex == index)
                {
                    CancelEdit();
                    NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.CantSwapSameBall);
                    return;
                }
                
                res = await dialog.OpenDialog(InventoryUIState.Swap, _inventoryService.GetBallData(index), _inventoryService.GetBallLevel(index), _inventoryService.GetBallData(_selectedIndex));
                if (res)
                { 
                    subCursor.GetComponent<Image>().enabled = false;
                    GameManager.Instance.SubCoin(_contentService.GetBallRemovePrice());
                    await _inventoryService.SwapBall(_selectedIndex, _swapIndex);
                    UIManager.Instance.EnableCanvasGroup("Rest", false);
                    // Trigger organise event - no return value needed
                    EventManager.OnOrganise.OnNext(R3.Unit.Default);
                    GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
                }
                CancelEdit();
                break;
            case InventoryUIState.Remove:
                res = await dialog.OpenDialog(InventoryUIState.Remove, _inventoryService.GetBallData(index), _inventoryService.GetBallLevel(index), null);
                if (res)
                {
                    _inventoryService.RemoveAndShiftBall(_selectedIndex);
                    GameManager.Instance.SubCoin(_contentService.GetBallRemovePrice());
                    UIManager.Instance.ResetSelectedGameObject();
                    shop.SetRemoveButtonInteractable(false);
                }
                CancelEdit();
                break;
            case InventoryUIState.Add:
                // Add状態の場合、通常のボールをクリックしても何もしない（既存のボールは選択できない）
                SeManager.Instance.PlaySe("error");
                break;
        }
    }
    
    private async UniTaskVoid OnClickEmptySlot(int index)
    {
        // Replace状態またはAdd状態で動作
        if (_state != InventoryUIState.Replace && _state != InventoryUIState.Add) return;
        
        UIManager.Instance.LockCursorToInventory(false);
        _selectedIndex = index;
        SeManager.Instance.PlaySe("button");
        
        var res = await dialog.OpenDialog(InventoryUIState.Add, _targetBallData, 0, null);
        if (res)
        {
            _inventoryService.AddBall(_targetBallData);
            GameManager.Instance.SubCoin(_contentService.GetShopPrice(Shop.ShopItemType.Ball, _targetBallData.rarity));
            
            // 空きスロットを更新
            InitializeEmptySlots();
            
            if (GameManager.Instance.state == GameManager.GameState.AfterBattle)
                afterBattleUI.UnInteractableSelectedItem();
            else
                shop.UnInteractableSelectedItem();
        }
        CancelEdit();
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
