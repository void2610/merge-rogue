using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class TweenButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button Settings")]
    [SerializeField]
    private bool tweenByPointer = true;
    [SerializeField]
    private bool tweenByClick = true;

    [Header("Tween Settings")]
    [SerializeField]
    private float scale = 1.1f;
    [SerializeField]
    private float duration = 0.5f;

    [Header("Raycast Settings")]
    [SerializeField]
    private GraphicRaycaster raycaster;
    [SerializeField]
    private EventSystem eventSystem;

    private float defaultScale = 1.0f;

    private CanvasGroup canvasGroup;
    private void OnClick()
    {
        this.transform.DOScale(defaultScale * scale, duration).SetEase(Ease.OutElastic);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tweenByPointer && canvasGroup.interactable && this.GetComponent<Button>()?.interactable == true)
            this.transform.DOScale(defaultScale * scale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tweenByPointer && canvasGroup.interactable)
            this.transform.DOScale(defaultScale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
    }

    public void ResetScale()
    {
        if (canvasGroup != null)
            this.transform.DOScale(defaultScale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
    }

    public void CheckMouseAndTween()
    {
        if (raycaster == null || eventSystem == null)
        {
            Debug.LogError("Raycaster or EventSystem is not assigned.");
            return;
        }
        this.transform.DOScale(defaultScale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
        // マウスからrayを飛ばして、ボタンの上にマウスがあるかどうかを判定する
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);
        if (results.Count == 0)
        {
            return;
        }
        // UI要素にヒットしたか確認
        if (results[0].gameObject == this.gameObject || results[0].gameObject.transform.IsChildOf(this.transform))
        {
            this.transform.DOScale(defaultScale * scale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
        }
    }

    private void Awake()
    {
        canvasGroup = this.transform.parent.GetComponent<CanvasGroup>();
        defaultScale = this.transform.localScale.x;
        if (tweenByClick)
        {
            if (this.GetComponent<Button>() != null)
            {
                this.GetComponent<Button>().onClick.AddListener(OnClick);
            }
        }
    }

    private void Start()
    {
        defaultScale = this.transform.localScale.x;
    }
}
