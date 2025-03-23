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
    [SerializeField] private float shiftUpDistance = 100f; // 通知1件分の高さ
    [SerializeField] private SerializableDictionary<NotifyIconType, Sprite> iconSprites;

    // 現在表示中の通知を管理するリスト（最新の通知をリスト先頭に配置）
    private readonly List<GameObject> _activeNotifications = new ();

    public void Notify(string message, NotifyIconType iconType = NotifyIconType.Other)
    {
        // 新しい通知ウィンドウの生成
        var window = Instantiate(windowPrefab, this.transform, false);
        var rectTransform = window.GetComponent<RectTransform>();
        // 初期位置を(0,0)に設定
        rectTransform.anchoredPosition = Vector2.zero;
        window.transform.Find("Icon").GetComponent<Image>().sprite = iconSprites[iconType];
        window.transform.Find("Message").GetComponent<TextMeshProUGUI>().text = message;
        window.transform.Find("Gauge").GetComponent<Image>().fillAmount = 0;

        // 新しい通知をリストの先頭に追加（これにより、既存の通知は上にずれる）
        _activeNotifications.Insert(0, window);

        // 全通知の位置を再計算して更新
        UpdateNotificationPositions();

        // 非同期で通知の表示・終了アニメーションを開始
        ShowNotificationAsync(window).Forget();
    }

    // 各通知の位置を、リストのインデックスに応じた位置にアニメーションで更新する
    private void UpdateNotificationPositions()
    {
        for (var i = 1; i < _activeNotifications.Count; i++)
        {
            var rectTransform = _activeNotifications[i].GetComponent<RectTransform>();
            rectTransform.DOAnchorPosY(shiftUpDistance * 2, 0.3f).SetEase(Ease.OutSine).SetRelative().SetUpdate(true);
        }
    }

    private async UniTask ShowNotificationAsync(GameObject window)
    {
        var rectTransform = window.GetComponent<RectTransform>();
        var canvasGroup = window.GetComponent<CanvasGroup>();
        var gauge = window.transform.Find("Gauge").GetComponent<Image>();

        // 右にスライドして表示
        await rectTransform.DOAnchorPosX(rightMoveDistance, showDuration)
                           .SetEase(Ease.OutSine)
                           .SetRelative()
                           .SetUpdate(true);
        // ゲージの進行アニメーション
        gauge.DOFillAmount(1, waitDuration)
             .SetEase(Ease.Linear)
             .SetUpdate(true)
             .Forget();

        // 待機時間後にフェードアウトと上移動で通知を閉じる
        await UniTask.Delay((int)(waitDuration * 1000), ignoreTimeScale: true);
        // 通知が閉じる際に、さらに上に移動
        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + shiftUpDistance, closeDuration)
                     .SetEase(Ease.InSine)
                     .SetUpdate(true)
                     .Forget();
        await canvasGroup.DOFade(0, closeDuration)
                         .SetEase(Ease.InSine)
                         .SetUpdate(true);

        // 管理リストから削除し、再配置を更新
        _activeNotifications.Remove(window);
        if(window) Destroy(window);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
