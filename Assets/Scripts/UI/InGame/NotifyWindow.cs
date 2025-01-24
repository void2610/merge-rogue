using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotifyWindow : MonoBehaviour
{
    public enum NotifyIconType
    {
        Setting,
        Error,
        Achievement,
        Other
    }
    
    public static NotifyWindow Instance;
    
    [SerializeField] private GameObject windowPrefab;
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float waitDuration = 3f;
    [SerializeField] private float closeDuration = 1f;
    [SerializeField] private float rightMoveDistance = 100f;
    [SerializeField] private float upMoveDistance = 100f;
    [SerializeField] private SerializableDictionary<NotifyIconType, Sprite> iconSprites;

    private UniTaskCompletionSource _currentTask;
    
    public async UniTaskVoid Notice(string message, NotifyIconType iconType = NotifyIconType.Other)
    {
        // 現在の動作が完了するまで待機
        if (_currentTask != null) await _currentTask.Task;
        
        var window = Instantiate(windowPrefab, this.transform, false);
        window.transform.localPosition = Vector3.zero;
        window.transform.Find("Icon").GetComponent<Image>().sprite = iconSprites[iconType];
        window.transform.Find("Message").GetComponent<TextMeshProUGUI>().text = message;
        window.transform.Find("Gauge").GetComponent<Image>().fillAmount = 0;

        // 新しいタスクの開始
        _currentTask = new UniTaskCompletionSource();
        await NoticeAsync(window);
        _currentTask.TrySetResult();
    }
    
    private async UniTask NoticeAsync(GameObject window)
    {
        var rectTransform = window.GetComponent<RectTransform>();
        var canvasGroup = window.GetComponent<CanvasGroup>();
        var gauge = window.transform.Find("Gauge").GetComponent<Image>();
        
        await rectTransform.DOMoveX(rightMoveDistance, showDuration).SetEase(Ease.OutSine).SetRelative().ToUniTask();
        gauge.DOFillAmount(1, waitDuration).SetEase(Ease.Linear).ToUniTask().Forget();
        await UniTask.Delay((int)(waitDuration * 1000));
        
        rectTransform.DOMoveY(upMoveDistance, closeDuration).SetEase(Ease.InSine).SetRelative().ToUniTask().Forget();
        await canvasGroup.DOFade(0, closeDuration).SetEase(Ease.InSine).ToUniTask();

        // Reset
        await rectTransform.DOMoveX(-rightMoveDistance, 0).SetRelative();
        await rectTransform.DOMoveY(-upMoveDistance, 0).SetRelative();
        Destroy(window);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
