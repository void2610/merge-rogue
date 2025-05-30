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
    [SerializeField] private AfterBattleUI afterBattleUI;
    
    private const float SIZE_COEFFICIENT = 8f;
    private static List<float> BallSizes => InventoryManager.Instance.Sizes;
    private readonly List<GameObject> _items = new();
    private int _selectedIndex = -1;
    private int _swapIndex = -1;
    private InventoryUIState _state = InventoryUIState.Disabled;
    private BallData _replaceBallData;

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
        UIManager.Instance.LockCursorToInventory(false);
        _swapIndex = -1;
        subCursor.GetComponent<Image>().enabled = false;
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
                res = await dialog.OpenDialog(InventoryUIState.Replace, InventoryManager.Instance.GetBallData(index), InventoryManager.Instance.GetBallLevel(index), _replaceBallData);
                if (res)
                {
                    InventoryManager.Instance.ReplaceBall(_replaceBallData, _selectedIndex);
                    GameManager.Instance.SubCoin(ContentProvider.GetSHopPrice(Shop.ShopItemType.Ball, _replaceBallData.rarity));
                    
                    if (GameManager.Instance.state == GameManager.GameState.AfterBattle)
                        afterBattleUI.UnInteractableSelectedItem();
                    else
                        shop.UnInteractableSelectedItem();
                }
                CancelEdit();
                break;
            case InventoryUIState.Upgrade:
                if (InventoryManager.Instance.GetBallLevel(index) >= 2)
                {
                    SeManager.Instance.PlaySe("error");
                    NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.CantUpgradeBall);
                    return;
                }
                
                res = await dialog.OpenDialog(InventoryUIState.Upgrade, InventoryManager.Instance.GetBallData(index), InventoryManager.Instance.GetBallLevel(index), null);
                if (res)
                { 
                    InventoryManager.Instance.UpgradeBall(_selectedIndex);
                    SeManager.Instance.PlaySe("levelUp");
                    afterBattleUI.SetUpgradeButtonInteractable(false);
                    GameManager.Instance.SubCoin(ContentProvider.GetBallUpgradePrice());
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
                
                res = await dialog.OpenDialog(InventoryUIState.Swap, InventoryManager.Instance.GetBallData(index), InventoryManager.Instance.GetBallLevel(index), InventoryManager.Instance.GetBallData(_selectedIndex));
                if (res)
                { 
                    subCursor.GetComponent<Image>().enabled = false;
                    GameManager.Instance.SubCoin(ContentProvider.GetBallRemovePrice());
                    await InventoryManager.Instance.SwapBall(_selectedIndex, _swapIndex);
                    UIManager.Instance.EnableCanvasGroup("Rest", false);
                    EventManager.OnOrganise.Trigger(0);
                    GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
                }
                CancelEdit();
                break;
            case InventoryUIState.Remove:
                res = await dialog.OpenDialog(InventoryUIState.Remove, InventoryManager.Instance.GetBallData(index), InventoryManager.Instance.GetBallLevel(index), null);
                if (res)
                {
                    InventoryManager.Instance.RemoveAndShiftBall(_selectedIndex);
                    GameManager.Instance.SubCoin(ContentProvider.GetBallRemovePrice());
                    UIManager.Instance.ResetSelectedGameObject();
                    shop.SetRemoveButtonInteractable(false);
                }
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
