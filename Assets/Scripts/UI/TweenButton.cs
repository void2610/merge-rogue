using System;
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
    private List<Tween> tweens = new();

    private void OnClick()
    {
        var t = this.transform.DOScale(defaultScale * scale, duration).SetEase(Ease.OutElastic);
        tweens.Add(t);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tweenByPointer && this.GetComponent<Button>()?.interactable == true)
        {
            var t = this.transform.DOScale(defaultScale * scale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
            tweens.Add(t);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tweenByPointer)
        {
            var t = this.transform.DOScale(defaultScale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
            tweens.Add(t);
        }
    }

    public void ResetScale()
    {
        var t = this.transform.DOScale(defaultScale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
        tweens.Add(t);
    }

    public void CheckMouseAndTween()
    {
        if (raycaster == null || eventSystem == null)
        {
            Debug.LogError("Raycaster or EventSystem is not assigned.");
            return;
        }
        var t = this.transform.DOScale(defaultScale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
        tweens.Add(t);
        // マウスからrayを飛ばして、ボタンの上にマウスがあるかどうかを判定する
        var pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };
        var results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);
        if (results.Count == 0)
        {
            return;
        }
        // UI要素にヒットしたか確認
        if (results[0].gameObject == this.gameObject || results[0].gameObject.transform.IsChildOf(this.transform))
        {
            var t2 = this.transform.DOScale(defaultScale * scale, duration).SetEase(Ease.OutElastic).SetUpdate(true);
            tweens.Add(t2);
        }
    }

    private void Awake()
    {
        defaultScale = this.transform.localScale.x;
        if (!tweenByClick) return;
        
        if (this.GetComponent<Button>() != null)
        {
            this.GetComponent<Button>().onClick.AddListener(OnClick);
        }
    }

    private void Start()
    {
        defaultScale = this.transform.localScale.x;
    }

    private void OnDestroy()
    {
        foreach (var t in tweens)
        {
            t?.Kill();
        }
    }
}
