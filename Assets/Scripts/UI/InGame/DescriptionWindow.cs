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
    public static DescriptionWindow Instance { get; private set; }
    
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
    [SerializeField] private Camera uiCamera;
    [SerializeField] private GameObject windowContainer;
    
    private CanvasGroup _cg;
    private Tween _moveTween;
    private Tween _fadeTween;
    private readonly Vector3 _disablePos = new (999, 999, 0);
    // (親オブジェクト, 単語) -> サブウィンドウオブジェクト
    private readonly Dictionary<(GameObject, string), GameObject> _subWindows = new();
    // その他のウィンドウ対象オブジェクト
    private readonly List<GameObject> _otherTriggerObjects = new();
    // ルートウィンドウのトリガー元のオブジェクト
    private GameObject _rootTriggerObject;
    private bool _isCheckingMouse = false;
    
    public void AddTextToObservation(GameObject text) => _otherTriggerObjects.Add(text);
    public void RemoveTextFromObservation(GameObject text) => _otherTriggerObjects.Remove(text);

    public void ShowWindow(object obj, GameObject rootTriggerObject, int ballLevel = 0)
    {
        if (IsMouseOverWindowOrDescendants(this.gameObject)) return;
        
        foreach (var window in _subWindows.Values) Destroy(window);
        _subWindows.Clear();
        this.gameObject.SetActive(true);

        if(obj is BallData b) SetBallTexts(b, ballLevel);
        else if(obj is RelicData r) SetRelicTexts(r);
        else throw new System.ArgumentException("obj is not BallData or RelicData");
        
        descriptionText.text = Utils.GetHighlightWords(descriptionText.text);

        // ワールド座標をRectTransformのローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.gameObject.transform.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(uiCamera, rootTriggerObject.transform.position + new Vector3(2.25f, 0, 0)),
            uiCamera,
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
    
    public void ShowSubWindow(GameObject parent, string word)
    {
        //ウィンドウのリストを更新
        for(var i = _subWindows.Count - 1; i >= 0; i--)
        {
            var window = _subWindows.ElementAt(i);
            if (!window.Value) _subWindows.Remove(window.Key);
        }
        
        // 最前面のウィンドウ以外のリンクは無視
        if (_subWindows.Count >= 1)
            if (parent != _subWindows.Values.Last()) return;
        
        var containerRect = windowContainer.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRect,
            RectTransformUtility.WorldToScreenPoint(uiCamera, parent.transform.position),
            uiCamera,
            out var localLinkPos
        );

        ShowSubWindowImpl(parent, word, localLinkPos);
    }

    private void ShowSubWindowImpl(GameObject parent, string word, Vector2 basePosition)
    {
        // 同じ親オブジェクトに対して複数のサブウィンドウを表示しない
        if(_subWindows.ContainsKey((parent, word))) return;
        _subWindows.Where(pair => pair.Key.Item1 == parent).ToList().ForEach(pair => HideSubWindow(parent, pair.Key.Item2));
        
        var description = wordDictionary.GetWordEntry(word).description;
        var textColor = wordDictionary.GetWordEntry(word).textColor;
        
        var g = Instantiate(subWindowPrefab, windowContainer.transform);
        g.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{word}</color>";
        g.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = Utils.GetHighlightWords(description);
        
        Utils.AddEventToObject(g, () => HideSubWindow(parent, word), EventTriggerType.PointerExit);
        
        // --- 位置計算の開始 ---
        // ① 対象オブジェクト(parent)の位置を取得し、スクリーン座標へ変換
        var parentRect = parent.GetComponent<RectTransform>();
        var parentScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, parentRect.position);
        // ② 専用コンテナ(windowContainer)のRectTransformを取得し、
        //     スクリーン座標からローカル座標へ変換（※これにより、windowContainer内での位置が得られる）
        var containerRect = windowContainer.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRect, 
            parentScreenPos, 
            uiCamera, 
            out var localParentPos
        );
        // ③ 固定のオフセットを付与（必要に応じてオフセット値は変更してください）
        var offset = new Vector2(100f, 25f);
        var desiredPosition = basePosition + offset;
        // ④ サブウィンドウが画面外に出ないように、クランプ処理
        var clampedPosition = new Vector2(
            Mathf.Clamp(desiredPosition.x, subMinPos.x, subMaxPos.x),
            Mathf.Clamp(desiredPosition.y, subMinPos.y, subMaxPos.y)
        );
        // ⑤ 位置を適用（UIの場合、anchoredPositionの利用が望ましい）
        var subWindowRect = g.GetComponent<RectTransform>();
        subWindowRect.anchoredPosition = clampedPosition;
        // --- 位置計算の終了 ---
        
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
        if(_moveTween != null && _moveTween.active) return;
        
        _moveTween?.Kill();
        _fadeTween?.Kill();
        
        _fadeTween = _cg.DOFade(0, 0.15f).SetUpdate(true).OnComplete(() =>
        {
            this.transform.position = _disablePos;
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
    
    private bool IsParentWindowIsThis(GameObject window)
    {
        if (window == this.gameObject) return true;
        var g = window.transform.parent;
        while (true)
        {
            if (g == this.gameObject.transform) return true;
            if (!g) return false;
            g = g.parent;
        }
    }
    
    private bool IsMouseOverWindowOrDescendants(GameObject window)
    {
        if(!window) return false;
        // マウスが現在のウィンドウ上にあるかチェック
        if (RectTransformUtility.RectangleContainsScreenPoint(
                window.GetComponent<RectTransform>(), Input.mousePosition, uiCamera))
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
        // subWindowの親も追加
        foreach (var k in _subWindows.Keys)
            allWindows.Add(k.Item1);
        
        foreach (var window in allWindows)
        {
            if(!window) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    window.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    uiCamera))
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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        this.transform.position = _disablePos;
        this.transform.parent = windowContainer.transform;
        _cg = this.gameObject.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        // マウスがウィンドウ外に出た場合にチェックを開始
        if (!IsMouseOverAnyWindow() && !_isCheckingMouse)
        {
            StartMouseCheck().Forget();
        }

        // すべてのウィンドウ(サブウィンドウ + this.gameObject +その他の対象オブジェクト)を収集
        var windows = new List<GameObject>(_subWindows.Values) { this.gameObject };
        windows.AddRange(_otherTriggerObjects);

        // 各ウィンドウのDescriptionText内のリンク検出
        var linkIndices = windows.Select(w =>
            TMP_TextUtilities.FindIntersectingLink(
                w.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>(),
                Input.mousePosition, uiCamera)
        ).ToList();

        var validLinks = linkIndices.Where(i => i != -1).ToList();
        if (!validLinks.Any()) return;
        int linkIndex = validLinks.First();
        int windowIndex = linkIndices.IndexOf(linkIndex);
        if (linkIndex == -1) return;

        // 対象テキストコンポーネント取得
        var textComponent = windows[windowIndex].transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        TMP_TextInfo textInfo = textComponent.textInfo;
        TMP_LinkInfo linkInfo = textInfo.linkInfo[linkIndex];

        // 対象ウィンドウ（親）の決定
        // this.gameObjectならdescriptionText、そうでなければそのウィンドウ
        var parent = windows[windowIndex] == this.gameObject ? descriptionText.gameObject : windows[windowIndex];

        // 最前面ウィンドウ以外のリンクは無視（必要に応じて）
        if (!_otherTriggerObjects.Contains(parent))
        {
            var topWindow = _subWindows.Count == 0 ? descriptionText.gameObject : _subWindows.Values.Last();
            if (parent != topWindow) return;
        }

        // --- リンクの文字位置を計算 ---
        // リンク内の最初の文字の情報を利用する例
        var charIndex = linkInfo.linkTextfirstCharacterIndex;
        var charInfo = textInfo.characterInfo[charIndex];
        // ここでは底辺の中央を基準とする（必要に応じて変更してください）
        var charMidLocal = (charInfo.bottomLeft + charInfo.bottomRight) / 2;
        // テキストコンポーネントのTransformでワールド座標に変換
        var worldPos = textComponent.transform.TransformPoint(charMidLocal);

        // 専用コンテナ(windowContainer)のローカル座標に変換
        var containerRect = windowContainer.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRect,
            RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos),
            uiCamera,
            out var localLinkPos
        );

        // サブウィンドウの表示：リンクIDと、基準位置(localLinkPos)を渡す
        ShowSubWindowImpl(parent, linkInfo.GetLinkID(), localLinkPos);
    }
}