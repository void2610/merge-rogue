using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIHoverSelection : MonoBehaviour
{
    [SerializeField] private GameObject currentSelectedGameObject;
    [SerializeField] private List<string> ignoreTags = new();

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

            // 最初に適切なUIを見つけたら、それを選択
            if (EventSystem.current.currentSelectedGameObject != hoveredObject)
            {
                CanvasGroupNavigationLimiter.SetSelectedGameObjectSafe(hoveredObject);
                currentSelectedGameObject = hoveredObject;
            }
            break; // 最初の適切なUIだけを選択する
        }
    }
}