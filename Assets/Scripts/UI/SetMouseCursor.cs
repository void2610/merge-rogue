using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetMouseCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CursorType cursorType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(this.TryGetComponent(out Button button))
            if(!button.interactable) return;
        
        MouseCursorManager.Instance.SetCursor(cursorType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseCursorManager.Instance.SetCursor(CursorType.Default);
    }
}