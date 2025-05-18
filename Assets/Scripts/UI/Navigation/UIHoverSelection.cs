using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIHoverSelection : MonoBehaviour
{
    [SerializeField] private GameObject currentSelectedGameObject;
    [SerializeField] private List<string> ignoreTags = new();

    private bool _isLockToInventory;
    
    public void LockCursorToInventory(bool b) => _isLockToInventory = b;
    
    private bool IsInventoryCanvasGroup(GameObject currentSelected)
    {
        var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
        if (!currentGroup) return false;
        // PauseとTutorialは例外的に許可
        if (currentGroup.name == "Pause" || currentGroup.name == "Tutorial")
            return true;
        
        if (currentGroup.name == "InventoryUIContainer") return true;
        return false;
    }
    
    private void Update()
    {
        if (!EventSystem.current) return;

        // 仮想マウスがあればそれを優先して使用、なければ物理マウス
        var pointerPosition = InputProvider.Instance.GetMousePosition();

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = pointerPosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            var hoveredObject = result.gameObject;

            // 特定のタグを持つオブジェクトは無視
            if (ignoreTags.Contains(hoveredObject.tag))
                break;
            // ロック中で、InventoryUI以外のUIは無視
            if (_isLockToInventory && !IsInventoryCanvasGroup(hoveredObject))
                break;

            // 最初に適切なUIを見つけたら、それを選択
            if (EventSystem.current.currentSelectedGameObject != hoveredObject)
            {
                SelectionCursor.SetSelectedGameObjectSafe(hoveredObject);
                currentSelectedGameObject = hoveredObject;
            }
            break; // 最初の適切なUIだけを選択する
        }
    }
}