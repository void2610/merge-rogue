using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIHoverSelection : MonoBehaviour
{
    private void Update()
    {
        if (!EventSystem.current) return;

        // マウス位置から UI の Raycast を取得
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            var hoveredObject = results[0].gameObject;

            // 現在選択されているオブジェクトが異なる場合のみ更新
            if (EventSystem.current.currentSelectedGameObject != hoveredObject)
            {
                EventSystem.current.SetSelectedGameObject(hoveredObject);
            }
        }
    }
}