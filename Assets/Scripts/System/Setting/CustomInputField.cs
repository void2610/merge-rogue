using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// カスタムInputField - フォーカス時に自動的に編集モードに入らない
/// Enterキーまたはダブルクリックで編集モードに入る
/// </summary>
public class CustomInputField : TMP_InputField
{
    private bool _preventAutoActivation = true;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        _preventAutoActivation = true;
    }
    
    public override void OnSelect(BaseEventData eventData)
    {
        // Selectableとしてのフォーカスは受け取るが、編集モードには入らない
        if (_preventAutoActivation)
        {
            // フォーカスのみ設定して編集モードには入らない
            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);
            }
            // ActivateInputFieldを呼ばない
        }
        else
        {
            // 手動アクティベーション時は通常通り
            base.OnSelect(eventData);
        }
    }
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;
            
        // ダブルクリックで編集モードに入る
        if (eventData.clickCount >= 2)
        {
            EnterEditMode();
        }
        else
        {
            // シングルクリックではフォーカスのみ
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
        }
    }
    
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;
            
        // Enterキーで編集モードに入る/抜ける
        if (!isFocused)
        {
            EnterEditMode();
        }
        else
        {
            // 編集モード中の場合は通常のSubmit処理（編集完了）
            base.OnSubmit(eventData);
        }
    }
    
    /// <summary>
    /// 編集モードに手動で入る
    /// </summary>
    private void EnterEditMode()
    {
        _preventAutoActivation = false;
        ActivateInputField();
        _preventAutoActivation = true;
    }
    
    public override void OnDeselect(BaseEventData eventData)
    {
        // 編集モードから抜ける時の処理
        _preventAutoActivation = true;
        base.OnDeselect(eventData);
    }
}