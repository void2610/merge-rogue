using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SetMouseCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [FormerlySerializedAs("cursorType")] [SerializeField] private CursorIconType cursorIconType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(this.TryGetComponent(out Button button))
            if(!button.interactable) return;
        
        MouseCursorManager.Instance.SetCursor(cursorIconType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseCursorManager.Instance.SetCursor(CursorIconType.Default);
    }
}