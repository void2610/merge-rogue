using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DescriptionWindow : MonoBehaviour
{
    [SerializeField] private WordDictionary wordDictionary;
    [SerializeField] private GameObject subWindowPrefab;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI flavorText;
    [SerializeField] private List<TextMeshProUGUI> statusTexts;
    [SerializeField] private Vector2 rootMinPos;
    [SerializeField] private Vector2 rootMaxPos;
    [SerializeField] private Vector2 subMinPos;
    [SerializeField] private Vector2 subMaxPos;
    
    private Camera _uiCamera;
    private CanvasGroup _cg;
    private Tween _moveTween;
    private Tween _fadeTween;
    // (親オブジェクト, 単語) -> サブウィンドウオブジェクト
    private readonly Dictionary<(GameObject, string), GameObject> _subWindows = new();
    // ルートウィンドウのトリガー元のオブジェクト
    private GameObject _rootTriggerObject;
    private bool _isCheckingMouse = false;

    public void ShowWindow(object obj, GameObject rootTriggerObject, int ballRank = 0)
    {
        if (IsMouseOverWindowOrDescendants(this.gameObject)) return;
        
        foreach (var window in _subWindows.Values) Destroy(window);
        _subWindows.Clear();
        this.gameObject.SetActive(true);

        if(obj is BallData b) SetBallTexts(b, ballRank);
        else if(obj is RelicData r) SetRelicTexts(r);
        else throw new System.ArgumentException("obj is not BallData or RelicData");
        
        descriptionText.text = Utils.GetHighlightWords(descriptionText.text);

        // ワールド座標をRectTransformのローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.gameObject.transform.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(_uiCamera, rootTriggerObject.transform.position + new Vector3(2.5f, 0, 0)),
            _uiCamera,
            out var localPos
        );

        // ローカル座標で位置をクランプ
        var clampedX = Mathf.Clamp(localPos.x, rootMinPos.x, rootMaxPos.x);
        var clampedY = Mathf.Clamp(localPos.y, rootMinPos.y, rootMaxPos.y);
        
        _moveTween?.Kill();
        _fadeTween?.Kill();
        
        this.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(clampedX, clampedY, 0) + new Vector3(0, 0.3f, 0);
        _moveTween = this.gameObject.transform.DOMoveY(0.3f, 0.2f).SetRelative(true).SetUpdate(true)
            .SetEase(Ease.OutBack).SetLink(this.gameObject);
        _cg.alpha = 0;
        _fadeTween = _cg.DOFade(1, 0.15f).SetUpdate(true).SetLink(this.gameObject);
        _rootTriggerObject = rootTriggerObject;
    }

    private void ShowSubWindow(GameObject parent, string word)
    {
        // 同じ親オブジェクトに対して複数のサブウィンドウを表示しない
        if(_subWindows.ContainsKey((parent, word))) return;
        _subWindows.Where(pair => pair.Key.Item1 == parent).ToList().ForEach(pair => HideSubWindow(parent, pair.Key.Item2));
        
        var description = wordDictionary.GetWordEntry(word).description;
        var textColor = wordDictionary.GetWordEntry(word).textColor;
        
        var g = Instantiate(subWindowPrefab, parent.transform);
        g.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{word}</color>";
        g.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = Utils.GetHighlightWords(description);
        
        Utils.AddEventToObject(g, () => HideSubWindow(parent, word), EventTriggerType.PointerExit);
        
        var offset = new Vector2(25, 25);
        var rectTransform = g.GetComponent<RectTransform>();

        var localMin = rectTransform.parent.InverseTransformPoint(subMinPos);
        var localMax = rectTransform.parent.InverseTransformPoint(subMaxPos);
        var clampedX = Mathf.Clamp(offset.x, localMin.x, localMax.x);
        var clampedY = Mathf.Clamp(offset.y, localMin.y, localMax.y);

        rectTransform.localPosition = new Vector3(clampedX, clampedY, rectTransform.localPosition.z);

        g.transform.DOMoveY(0.3f, 0.2f).SetRelative(true).SetUpdate(true).SetEase(Ease.OutBack).SetLink(g);
        g.GetComponent<CanvasGroup>().DOFade(1, 0.15f).SetUpdate(true).SetLink(g);
        _subWindows[(parent, word)] = g;
    }
    
    private void SetBallTexts(BallData b, int level)
    {
        nameText.text = b.displayName;
        nameText.color = MyColors.GetRarityColor(b.rarity);
        descriptionText.text = b.descriptions[level];
        flavorText.text = b.flavorText;
        statusTexts[0].text = "level: " + (level + 1);
        statusTexts[0].alpha = 1;
        statusTexts[1].text = "attack: " + b.attacks[level];
        statusTexts[1].alpha = 1;
        statusTexts[2].text = "size: " + b.sizes[level];
        statusTexts[2].alpha = 1;
    }

    private void SetRelicTexts(RelicData r)
    {
        nameText.text = r.displayName;
        nameText.color = MyColors.GetRarityColor(r.rarity);
        descriptionText.text = r.description;
        flavorText.text = r.flavorText;
        statusTexts[0].text = "price: " + ContentProvider.GetSHopPrice(Shop.ShopItemType.Ball, r.rarity);
        statusTexts[1].alpha = 0;
        statusTexts[2].alpha = 0;
    }

    private void HideWindow()
    {
        if(!descriptionText) return;
        if(IsMouseOverWindowOrDescendants(descriptionText.gameObject)) return;
        if(_moveTween.active) return;
        
        _moveTween?.Kill();
        _fadeTween?.Kill();
        
        _fadeTween = _cg.DOFade(0, 0.15f).SetUpdate(true).OnComplete(() =>
        {
            this.gameObject.SetActive(false);
            this.transform.localPosition = new Vector3(999, 999, 0);
        }).SetLink(this.gameObject);
        foreach (var window in _subWindows.Values)
        {
            Destroy(window);
        }
        _subWindows.Clear();
    }
    
    private void HideSubWindow(GameObject parent, string word)
    {
        // 対応するサブウィンドウを取得
        if (_subWindows.TryGetValue((parent, word), out var window))
        {
            // マウスが現在のウィンドウまたはその子孫ウィンドウにいる場合は閉じない
            if (IsMouseOverWindowOrDescendants(window))
            {
                return;
            }

            // 子ウィンドウを再帰的に閉じる
            var children = _subWindows
                .Where(entry => entry.Key.Item1 == window) // 現在のウィンドウを親としているものを取得
                .Select(entry => (entry.Key.Item1, entry.Key.Item2))
                .ToList();

            foreach (var child in children)
            {
                HideSubWindow(child.Item1, child.Item2); // 再帰的に子ウィンドウを閉じる
            }

            // 現在のウィンドウを削除
            window.GetComponent<CanvasGroup>().DOFade(0, 0.15f).SetUpdate(true).OnComplete(() => Destroy(window)).SetLink(window);
            _subWindows.Remove((parent, word));
        }
    }
    
    private bool IsMouseOverWindowOrDescendants(GameObject window)
    {
        if(!window) return false;
        // マウスが現在のウィンドウ上にあるかチェック
        if (RectTransformUtility.RectangleContainsScreenPoint(
                window.GetComponent<RectTransform>(), Input.mousePosition, _uiCamera))
        {
            return true;
        }

        foreach (var entry in _subWindows)
        {
            // 自分の子孫ウィンドウにマウスがあるか確認
            if (entry.Key.Item1 == window) // 現在のウィンドウの子孫ウィンドウの場合
            {
                // 再帰的に子孫ウィンドウをチェック
                if (IsMouseOverWindowOrDescendants(entry.Value))
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private bool IsMouseOverAnyWindow()
    {
        if (!this) return false;

        // 全てのコライダーをチェック
        var allWindows = new List<GameObject>(_subWindows.Values) { this.gameObject, _rootTriggerObject };
        foreach (var window in allWindows)
        {
            if(!window) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    window.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    _uiCamera))
            {
                return true;
            }
        }
        return false;
    }
    
    private async UniTaskVoid StartMouseCheck()
    {
        if (_isCheckingMouse) return;

        _isCheckingMouse = true;

        // マウスが1秒間連続してウィンドウ外にあるかチェック
        while (true)
        {
            if (!await CheckMouseOutsideForSeconds(0.15f))
            {
                _isCheckingMouse = false;
                return;
            }

            // ウィンドウを隠す処理を実行
            HideWindow();
        }
    }

    private async UniTask<bool> CheckMouseOutsideForSeconds(float duration)
    {
        var cancelToken = this.GetCancellationTokenOnDestroy();
        var timer = 0f;
        while (timer < duration)
        {
            // マウスがウィンドウ内に戻った場合は中断
            if (IsMouseOverAnyWindow()) return false;
            // 経過時間を加算
            timer += Time.deltaTime / Time.timeScale;
            // フレームの終了まで待機
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);
        }
        return true;
    }

    private void Awake()
    {
        this.gameObject.SetActive(false);
        _cg = this.gameObject.GetComponent<CanvasGroup>();
        _uiCamera = Camera.main;
    }
    
    private void Update()
    {
        // マウスがウィンドウ外に出た場合にチェックを開始
        if (!IsMouseOverAnyWindow() && !_isCheckingMouse)
        {
            StartMouseCheck().Forget();
        }
        
        // マウスがホバーしているリンクを取得
        var windows = new List<GameObject>(_subWindows.Values) { this.gameObject };
        var linkIndices = windows.Select(w => TMP_TextUtilities.FindIntersectingLink(w.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>(), Input.mousePosition, _uiCamera)).ToList();
        var enumerable = linkIndices.Where(i => i != -1).ToList();
        if (!enumerable.Any()) return;
        var link = enumerable.First();
        var index = linkIndices.IndexOf(link);
        if (link == -1) return;
        
        // マウスがリンクにホバーしている場合はサブウィンドウを表示
        var linkInfo = windows[index].transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().textInfo.linkInfo[link];
        var parent = windows[index] == this.gameObject ? descriptionText.gameObject : windows[index];
        // 最前面のウィンドウ以外のリンクは無視
        var topWindow = _subWindows.Count == 0 ? descriptionText.gameObject : _subWindows.Values.Last();
        if (parent != topWindow) return;
        ShowSubWindow(parent, linkInfo.GetLinkID());
    }
}