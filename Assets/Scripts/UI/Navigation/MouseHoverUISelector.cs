using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class MouseHoverUISelector : MonoBehaviour
{
    [SerializeField] private GameObject currentSelected;
    [SerializeField] private List<string> ignoreTags = new();

    private static bool _isLockToInventory;
    private EventSystem _eventSystem;
    
    public static void LockCursorToInventory(bool b) => _isLockToInventory = b;
    
    private bool IsInventoryCanvasGroup(GameObject currentSelected)
    {
        var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
        if (!currentGroup) return false;
        // PauseとTutorialは例外的に許可
        if (currentGroup.name == "Pause" || currentGroup.name == "Tutorial") return true;
        if (currentGroup.name == "InventoryUIContainer") return true;
        return false;
    }

    private void Awake()
    {
        _eventSystem = EventSystem.current;
    }
    
    private void Update()
    {

        // 仮想マウスがあればそれを優先して使用、なければ物理マウス
        var pointerPosition = InputProvider.Instance.GetMousePosition();

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = pointerPosition
        };

        var results = new List<RaycastResult>();
        _eventSystem.RaycastAll(pointerData, results);

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
            if (_eventSystem.currentSelectedGameObject != hoveredObject)
            {
                SelectionCursor.SetSelectedGameObjectSafe(hoveredObject);
                currentSelected = hoveredObject;
            }
            break; // 最初の適切なUIだけを選択する
        }
    }
}