using UnityEngine;
using UnityEngine.EventSystems;

public class SetMouseCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CursorType cursorType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseCursorManager.Instance.SetCursor(cursorType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseCursorManager.Instance.SetCursor(CursorType.Default);
    }
}