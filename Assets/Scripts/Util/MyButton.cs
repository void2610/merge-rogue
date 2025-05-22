using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class MyButton : Button
{
    private bool _isAvailable = true;

    /// <summary>
    /// 独自の使用可否（interactableとは別に制御）
    /// </summary>
    public bool IsAvailable
    {
        get => _isAvailable;
        set
        {
            if (_isAvailable == value) return;
            _isAvailable = value;
            UpdateVisualState();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        UpdateVisualState();
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        if (!IsAvailable) return;
        base.OnSubmit(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!IsAvailable) return;
        base.OnPointerClick(eventData);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        // 選択は常に許可（interactable=false とは違う）
    }

    /// <summary>
    /// 見た目を更新する
    /// </summary>
    private void UpdateVisualState()
    {
        // interactable は true にしておく（ナビゲーションのため）
        base.interactable = true;

        if (IsAvailable)
        {
            DoStateTransition(SelectionState.Normal, true);
        }
        else
        {
            DoStateTransition(SelectionState.Disabled, true);
        }
    }

    // 外部からは Button.onClick をそのまま使える
}