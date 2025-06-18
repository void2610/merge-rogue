using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer.Unity;

public class MouseHoverUISelector : ITickable
{
    private const string IGNORE_TAG = "IgnoreSelection";
    
    private static bool _isLockToInventory;
    private readonly IInputProvider _inputProvider;
    
    public MouseHoverUISelector(IInputProvider inputProvider)
    {
        _inputProvider = inputProvider;
    }
    
    public static void LockCursorToInventory(bool b) => _isLockToInventory = b;
    
    private bool IsInventoryCanvasGroup(GameObject currentSelected)
    {
        var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
        if (!currentGroup) return false;
        // PauseとTutorialは例外的に許可
        if (currentGroup.name is "Pause" or "Tutorial") return true;
        if (currentGroup.name == "InventoryUIContainer") return true;
        return false;
    }

    public void Tick()
    {
        var eventSystem = EventSystem.current;
        if (!eventSystem) return;

        // 仮想マウスがあればそれを優先して使用、なければ物理マウス
        var pointerPosition = _inputProvider.GetMousePosition();

        var pointerData = new PointerEventData(eventSystem)
        {
            position = pointerPosition
        };

        var results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            var hoveredObject = result.gameObject;

            // 特定のタグを持つオブジェクトは無視
            if (hoveredObject.CompareTag(IGNORE_TAG))
                break;
            // ロック中で、InventoryUI以外のUIは無視
            if (_isLockToInventory && !IsInventoryCanvasGroup(hoveredObject))
                break;

            // 最初に適切なUIを見つけたら、それを選択
            if (eventSystem.currentSelectedGameObject != hoveredObject)
            {
                SelectionCursor.SetSelectedGameObjectSafe(hoveredObject);
            }
            break; // 最初の適切なUIだけを選択する
        }
    }
}