using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MergeAreaCursorSetter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsMergeArea { get; private set; }
    public void OnPointerEnter(PointerEventData eventData) => IsMergeArea = true;
    public void OnPointerExit(PointerEventData eventData) => IsMergeArea = false;
}