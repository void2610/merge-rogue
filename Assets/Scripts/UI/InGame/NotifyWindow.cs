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

    private readonly Queue<(string message, NotifyIconType iconType)> _notificationQueue = new();
    private bool _isProcessing = false;

    public void Notify(string message, NotifyIconType iconType = NotifyIconType.Other)
    {
        // 通知をキューに追加
        _notificationQueue.Enqueue((message, iconType));

        // 通知処理が未実行なら開始
        if (!_isProcessing)
        {
            _isProcessing = true;
            ProcessQueue().Forget();
        }
    }

    private async UniTaskVoid ProcessQueue()
    {
        while (_notificationQueue.Count > 0)
        {
            var (message, iconType) = _notificationQueue.Dequeue();

            var window = Instantiate(windowPrefab, this.transform, false);
            window.transform.localPosition = Vector3.zero;
            window.transform.Find("Icon").GetComponent<Image>().sprite = iconSprites[iconType];
            window.transform.Find("Message").GetComponent<TextMeshProUGUI>().text = message;
            window.transform.Find("Gauge").GetComponent<Image>().fillAmount = 0;

            await ShowNotificationAsync(window);
        }

        // 処理終了フラグをリセット
        _isProcessing = false;
    }

    private async UniTask ShowNotificationAsync(GameObject window)
    {
        var rectTransform = window.GetComponent<RectTransform>();
        var canvasGroup = window.GetComponent<CanvasGroup>();
        var gauge = window.transform.Find("Gauge").GetComponent<Image>();

        await rectTransform.DOMoveX(rightMoveDistance, showDuration).SetEase(Ease.OutSine).SetRelative().SetUpdate(true).ToUniTask();
        gauge.DOFillAmount(1, waitDuration).SetEase(Ease.Linear).SetUpdate(true);
        await UniTask.Delay((int)waitDuration * 1000, ignoreTimeScale: true);

        rectTransform.DOMoveY(upMoveDistance, closeDuration).SetEase(Ease.InSine).SetRelative().SetUpdate(true);
        await canvasGroup.DOFade(0, closeDuration).SetEase(Ease.InSine).SetUpdate(true).ToUniTask();

        // Reset
        await rectTransform.DOMoveX(-rightMoveDistance, 0).SetRelative().SetUpdate(true);
        await rectTransform.DOMoveY(-upMoveDistance, 0).SetRelative().SetUpdate(true);
        Destroy(window);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
