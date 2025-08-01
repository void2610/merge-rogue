using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

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
    private bool _isWindowLocked;
    private CancellationTokenSource _hoverTokenSource;
    private CancellationTokenSource _hideTokenSource;
    
    private IInputProvider _inputProvider;
    private IContentService _contentService;
    
    [Inject]
    public void InjectDependencies(IInputProvider inputProvider, IContentService contentService)
    {
        _inputProvider = inputProvider;
        _contentService = contentService;
    }
    
    public void AddTextToObservation(GameObject text) => _otherTriggerObjects.Add(text);
    public void RemoveTextFromObservation(GameObject text) => _otherTriggerObjects.Remove(text);
    
    /// <summary>
    /// 状態異常の説明を取得試行
    /// </summary>
    private bool TryGetStatusEffectDescription(string word, out string description, out Color textColor)
    {
        description = "";
        textColor = Color.white;
        
        // StatusEffectManagerが初期化されていない場合は失敗
        if (!StatusEffectManager.Instance) return false;
        
        // enum名での検索
        if (Enum.TryParse<StatusEffectType>(word, true, out var statusEffectType))
        {
            description = StatusEffects.GetDescription(statusEffectType);
            textColor = StatusEffects.GetColor(statusEffectType);
            return !string.IsNullOrEmpty(description);
        }
        
        // ローカライズされた名前での検索
        foreach (StatusEffectType type in Enum.GetValues(typeof(StatusEffectType)))
        {
            var localizedName = StatusEffects.GetLocalizedName(type);
            if (localizedName.Equals(word, StringComparison.OrdinalIgnoreCase))
            {
                description = StatusEffects.GetDescription(type);
                textColor = StatusEffects.GetColor(type);
                return !string.IsNullOrEmpty(description);
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// マウスが入った際のウィンドウ表示と、ホバー状態を待つ処理
    /// </summary>
    /// <param name="data">表示するデータオブジェクト（BallData/RelicDataなど）</param>
    /// <param name="rootTriggerObject">対象UIオブジェクト</param>
    /// <param name="ballLevel">（必要なら）ボールレベル</param>
    public async UniTaskVoid ShowWindowWithHoverCheck(object data, GameObject rootTriggerObject, int ballLevel = 0)
    {
        _hoverTokenSource?.Cancel();
        _hoverTokenSource = new CancellationTokenSource();

        Utils.AddEventToObject(rootTriggerObject, () =>
        {
            // 待機中のタスクをキャンセル
            _hoverTokenSource?.Cancel();
            _hoverTokenSource = null;
        
            // ロック状態でなければ、すぐにウィンドウを非表示にする
            if (!_isWindowLocked)
            {
                HideWindow();
            }
        }, EventTriggerType.PointerExit, false);
        
        // まずはウィンドウを表示
        ShowWindow(data, rootTriggerObject, ballLevel);
        
        // ホバー待機時間（ミリ秒）
        var totalDelay = 1500f * Time.timeScale;
        var elapsed = 0f;
        var progressImage = this.transform.Find("ProgressImage")?.GetComponent<Image>();
        if (!progressImage) return;

        // 進捗表示用のImageがあれば初期化
        progressImage.fillAmount = 0f;
        
        // ホバー状態を待つループ
        while (elapsed < totalDelay)
        {
            // 対象UI（rootTriggerObject）上にマウスが存在しなければ、ウィンドウを非表示にして終了
            if (!IsMouseOverObject(rootTriggerObject))
            {
                HideWindow();
                progressImage.fillAmount = 0f;
                return;
            }

            // 経過時間に応じた進捗を更新
            progressImage.fillAmount = elapsed / totalDelay;
            
            // フレーム待機（Updateタイミングでチェック）
            await UniTask.Yield(PlayerLoopTiming.Update);
            elapsed += Time.deltaTime * 1000f; // Time.deltaTimeは秒単位なのでミリ秒に変換
        }

        // 一定時間経過したら進捗を満タンにしてロック状態にする
        progressImage.fillAmount = 1f;
        _isWindowLocked = true;
        this.transform.Find("Window").GetComponent<Image>().raycastTarget = true;
    }

    public void ShowWindowFromNavigation(object data, GameObject rootTriggerObject, int ballLevel = 0)
    {
        _isWindowLocked = false;
        this.transform.Find("Window").GetComponent<Image>().raycastTarget = false;
        ShowWindow(data, rootTriggerObject, ballLevel);
    }
    public void HideWindowFromNavigation() => HideWindow();
    public void HideSubWindowFromNavigation(GameObject parent, string word) => HideSubWindow(parent, word);

    private void ShowWindow(object obj, GameObject rootTriggerObject, int ballLevel = 0)
    {
        foreach (var window in _subWindows.Values) Destroy(window);
        _subWindows.Clear();
        this.gameObject.SetActive(true);

        if(obj is BallData b) SetBallTexts(b, ballLevel);
        else if(obj is RelicData r) SetRelicTexts(r);
        else throw new ArgumentException("obj is not BallData or RelicData");
        
        descriptionText.text = Utils.GetHighlightWords(wordDictionary, descriptionText.text);

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
        
        string displayName;
        string description;
        Color textColor;
        
        // WordDictionaryから汎用メソッドで検索（キーまたはローカライズされた単語名で検索）
        var wordEntry = wordDictionary.GetWordEntryByAny(word);
        if (wordEntry != null)
        {
            // WordEntryが見つかった場合、ローカライズされた内容を取得
            displayName = wordEntry.GetLocalizedWord();
            description = wordEntry.GetLocalizedDescription();
            textColor = wordEntry.textColor;
        }
        // 状態異常の説明を試行（WordDictionaryに見つからなかった場合）
        else if (TryGetStatusEffectDescription(word, out description, out textColor))
        {
            // StatusEffectDataから取得成功
            displayName = word; // 状態異常は元のまま表示
        }
        else
        {
            // どこにも見つからなかった場合
            displayName = word;
            description = $"説明が見つかりません: {word}";
            textColor = Color.white;
        }
        
        var g = Instantiate(subWindowPrefab, windowContainer.transform);
        g.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{displayName}</color>";
        g.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = Utils.GetHighlightWords(wordDictionary, description);
        
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
            out _
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
        nameText.text = b.GetDisplayName();
        nameText.color = b.rarity.GetColor();
        
        #if DEMO_PLAY
            if (!b.availableDemo)
            {
                descriptionText.text = $"{b.GetDescription()} \n ({LocalizeStringLoader.Instance.Get("LOCKED_IN_DEMO")})"; 
                flavorText.text = "?????";
                statusTexts[0].text = "?????";
                statusTexts[1].text = "?????";
                statusTexts[2].text = "?????";
                return;
            }
        #endif
        
        descriptionText.text = b.GetDescription();
        flavorText.text = b.GetFlavorText();
        statusTexts[0].text = "level: " + (level + 1);
        statusTexts[0].alpha = 1;
        statusTexts[1].text = "attack: " + b.attacks[level];
        statusTexts[1].alpha = 1;
        statusTexts[2].text = "size: " + b.sizes[level];
        statusTexts[2].alpha = 1;
    }

    private void SetRelicTexts(RelicData r)
    {
        nameText.text = r.GetDisplayName();
        nameText.color = r.rarity.GetColor();
        
        #if DEMO_PLAY
            if (!r.availableDemo)
            {
                descriptionText.text = $"{r.GetDescription()} \n ({LocalizeStringLoader.Instance.Get("LOCKED_IN_DEMO")})";
                flavorText.text = "?????";
                statusTexts[0].text = "?????";
                statusTexts[1].text = "?????";
                statusTexts[2].text = "?????";
                return;
            }
        #endif

        descriptionText.text = r.GetDescription();
        flavorText.text = r.GetFlavorText();
        var price = _contentService.GetShopPrice(Shop.ShopItemType.Ball, r.rarity);
        statusTexts[0].text = "price: " + price;
        statusTexts[1].alpha = 0;
        statusTexts[2].alpha = 0;
    }

    private void HideWindow()
    {
        _moveTween?.Kill();
        _fadeTween?.Kill();
        
        _fadeTween = _cg.DOFade(0, 0.15f).SetUpdate(true).OnComplete(() =>
        {
            this.transform.position = _disablePos;
        }).SetLink(this.gameObject);
        foreach (var window in _subWindows.Values) Destroy(window);
        _subWindows.Clear();
        
        this.transform.Find("Window").GetComponent<Image>().raycastTarget = false;
    }
    
    private void HideSubWindow(GameObject parent, string word)
    {
        // 対応するサブウィンドウを取得
        if (_subWindows.TryGetValue((parent, word), out var window))
        {
            // マウスが現在のウィンドウまたはその子孫ウィンドウにいる場合は閉じない
            if (IsMouseOverWindowOrDescendants(window)) return;

            // 子ウィンドウを再帰的に閉じる
            var children = _subWindows
                .Where(entry => entry.Key.Item1 == window) // 現在のウィンドウを親としているものを取得
                .Select(entry => (entry.Key.Item1, entry.Key.Item2))
                .ToList();

            foreach (var child in children)
                HideSubWindow(child.Item1, child.Item2); // 再帰的に子ウィンドウを閉じる

            // 現在のウィンドウを削除
            window.GetComponent<CanvasGroup>().DOFade(0, 0.15f).SetUpdate(true).OnComplete(() => Destroy(window)).SetLink(window);
            _subWindows.Remove((parent, word));
        }
    }
    
    private bool IsMouseOverWindowOrDescendants(GameObject window)
    {
        if(!window || _inputProvider == null) return false;
        // マウスが現在のウィンドウ上にあるかチェック
        if (RectTransformUtility.RectangleContainsScreenPoint(
                window.GetComponent<RectTransform>(), _inputProvider.GetMousePosition(), uiCamera))
        {
            return true;
        }

        foreach (var entry in _subWindows)
        {
            // 自分の子孫ウィンドウにマウスがあるか確認
            if (entry.Key.Item1 == window) // 現在のウィンドウの子孫ウィンドウの場合
            {
                // 再帰的に子孫ウィンドウをチェック
                if (IsMouseOverWindowOrDescendants(entry.Value)) return true;
            }
        }

        return false;
    }
    
    private bool IsMouseOverObject(GameObject obj)
    {
        if (!obj || _inputProvider == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(
            obj.GetComponent<RectTransform>(),
            _inputProvider.GetMousePosition(),
            uiCamera
        );
    }
    
    private bool IsMouseOverAnyWindow()
    {
        if (!this || _inputProvider == null) return false;

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
                    _inputProvider.GetMousePosition(),
                    uiCamera))
            {
                return true;
            }
        }
        return false;
    }
    
    private static bool IsCanvasGroupAlphaLow(Transform target, float threshold = 0.5f)
    {
        while (target)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup) return canvasGroup.alpha <= threshold;
            target = target.parent; // 親オブジェクトをたどる
        }
        // CanvasGroupが見つからなかった場合は false を返す
        return false;
    }
    
    private async UniTaskVoid HideWindowAfterDelay()
    {
        try
        {
            // 500ミリ秒待機。待機中にキャンセルされた場合は OperationCanceledException が発生
            await UniTask.Delay((int)(500 * Time.timeScale), cancellationToken: _hideTokenSource.Token);

            // 待機後もマウスがウィンドウ上にないなら非表示
            if (!IsMouseOverAnyWindow())
            {
                HideWindow();
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合は何もしない
        }
        finally
        {
            // タスク終了後はトークンをリセット
            if (this) _hideTokenSource = null;
        }
    }
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
        
        this.transform.position = _disablePos;
        _cg = this.gameObject.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        // InputProviderが注入されるまで処理をスキップ
        if (_inputProvider == null) return;
        
        // マウスがウィンドウ上にない場合、非表示処理を開始
        if (!IsMouseOverAnyWindow() && _isWindowLocked)
        {
            // すでに待機中でなければ、新たにタスクを開始する
            if (_hideTokenSource == null)
            {
                _hideTokenSource = new CancellationTokenSource();
                HideWindowAfterDelay().Forget();
            }
        }
        else
        {
            // マウスがウィンドウ上にある場合、待機中のタスクがあればキャンセル
            if (_hideTokenSource != null)
            {
                _hideTokenSource.Cancel();
                _hideTokenSource = null;
            }
        }
        
        // すべてのウィンドウ(サブウィンドウ + this.gameObject +その他の対象オブジェクト)を収集
        var windows = new List<GameObject>(_subWindows.Values) { this.gameObject };
        windows.AddRange(_otherTriggerObjects);

        // 各ウィンドウのDescriptionText内のリンク検出
        var linkIndices = windows.Select(w =>
            TMP_TextUtilities.FindIntersectingLink(
                w.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>(),
                _inputProvider.GetMousePosition(), uiCamera)
        ).ToList();

        var validLinks = linkIndices.Where(i => i != -1).ToList();
        if (!validLinks.Any()) return;
        int linkIndex = validLinks.First();
        int windowIndex = linkIndices.IndexOf(linkIndex);
        if (linkIndex == -1) return;

        // 対象テキストコンポーネント取得
        var textComponent = windows[windowIndex].transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        if (IsCanvasGroupAlphaLow(textComponent.transform)) return;
        var textInfo = textComponent.textInfo;
        var linkInfo = textInfo.linkInfo[linkIndex];

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

