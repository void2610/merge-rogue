using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.LowLevel;
using VContainer;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    [SerializeField] private DescriptionWindow descriptionWindow;
    [SerializeField] private GameObject virtualMouse;
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private RectTransform creditContent;
    [SerializeField] private TextMeshProUGUI licenseText;
    [SerializeField] private RectTransform licenseContent;
    [SerializeField] private TextMeshProUGUI versionText;
    
    private readonly Dictionary<string, Sequence> _canvasGroupTween = new();
    
    // Service dependencies
    private ICreditService _creditService;
    private ILicenseService _licenseService;
    private IVersionService _versionService;
    private IGameSettingsService _gameSettingsService;
    private IVirtualMouseService _virtualMouseService;
    private IInputProvider _inputProvider;
    
    [Inject]
    public void InjectDependencies(
        ICreditService creditService,
        ILicenseService licenseService,
        IVersionService versionService,
        IGameSettingsService gameSettingsService,
        IVirtualMouseService virtualMouseService,
        IInputProvider inputProvider)
    {
        this._creditService = creditService;
        this._licenseService = licenseService;
        this._versionService = versionService;
        this._gameSettingsService = gameSettingsService;
        this._virtualMouseService = virtualMouseService;
        this._inputProvider = inputProvider;
    }

    private GameObject GetTopCanvasGroup() => canvasGroups.Find(c => c.alpha > 0)?.gameObject;
    private bool IsVirtualMouseActive() => _virtualMouseService?.IsVirtualMouseActive() ?? false;

    // private void ResetSelectedGameObject()
    // {
    //     var topCanvas = GetTopCanvasGroup();
    //     if (topCanvas)
    //     {
    //         var focusSelectable = topCanvas.GetComponentInChildren<FocusSelectable>();
    //         if (focusSelectable.GetComponent<Selectable>().interactable == true)
    //             SelectionCursor.SetSelectedGameObjectSafe(focusSelectable.gameObject);
    //     }
    //     else
    //     {
    //         SelectionCursor.SetSelectedGameObjectSafe(startButton.gameObject);
    //     }
    // }

    private ScrollRect GetActiveScrollRect()
    {
        var topCanvas = GetTopCanvasGroup();
        if (topCanvas)
        {
            var scrollRect = topCanvas.GetComponentInChildren<ScrollRect>();
            if (scrollRect)
            {
                return scrollRect;
            }
        }
        return null;
    }

    /// <summary>
    /// 仮想マウスを中央に移動します
    /// </summary>
    private void MoveVirtualMouseToCenter()
    {
        _virtualMouseService?.MoveVirtualMouseToCenter();
    }

    /// <summary>
    /// 仮想マウスの有効/無効を切り替えます
    /// </summary>
    // private void ToggleVirtualMouse()
    // {
    //     _virtualMouseService?.ToggleVirtualMouse();
    // }
    
    // private async UniTaskVoid EnableCanvasGroupAsync(string canvasName, bool e)
    // {
    //     var cg = canvasGroups.Find(c => c.name == canvasName);
    //     if (!cg) return;
    //     if (_canvasGroupTween[canvasName].IsActive()) return;
    //     
    //     // アニメーション中は操作をブロック
    //     cg.interactable = false;
    //     cg.blocksRaycasts = false;
    //     
    //     var seq = DOTween.Sequence();
    //     seq.SetUpdate(true).Forget();
    //     if (e)
    //     {
    //         seq.Join(cg.transform.DOMoveY(-0.45f, 0).SetRelative(true)).Forget();
    //         seq.Join(cg.transform.DOMoveY(0.45f, 0.2f).SetRelative(true).SetEase(Ease.OutBack)).Forget();
    //         seq.Join(cg.DOFade(1, 0.2f)).Forget();
    //     }
    //     else
    //     {
    //         seq.Join(cg.DOFade(0, 0.2f)).Forget();
    //     }
    //     
    //     _canvasGroupTween[canvasName] = seq;
    //     SelectionCursor.SetSelectedGameObjectSafe(null);
    //     
    //     await seq.AsyncWaitForCompletion();
    //     
    //     // inputGuide.UpdateText(IsAnyCanvasGroupEnabled() ? InputGuide.InputGuideType.Navigate : InputGuide.InputGuideType.Merge);
    //     _canvasGroupTween[canvasName] = null;
    //     cg.interactable = e;
    //     cg.blocksRaycasts = e;
    //     
    //     // FocusSelectableがアタッチされているオブジェクトがあればフォーカス
    //     ResetSelectedGameObject();
    // }
    
    public void StartGame()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1.0f, 1.0f).OnComplete(() =>
        {
            SceneManager.LoadScene("MainScene");
        });
    }
    
    // public void ShowEncyclopedia() => EnableCanvasGroupAsync("Encyclopedia", true).Forget();
    // public void HideEncyclopedia() => EnableCanvasGroupAsync("Encyclopedia", false).Forget();
    // public void ShowCredit() => EnableCanvasGroupAsync("Credit", true).Forget();
    // public void HideCredit() => EnableCanvasGroupAsync("Credit", false).Forget();
    // public void ShowLicense() => EnableCanvasGroupAsync("License", true).Forget();
    // public void HideLicense() => EnableCanvasGroupAsync("License", false).Forget();
    
    // public void ShowDescriptionWindow(object o, GameObject g)
    // {
    //     descriptionWindow.ShowWindowWithHoverCheck(o, g).Forget();
    // }
    
    public void OpenTwitter()
    {
        Application.OpenURL("https://x.com/void2610");
    }
    
    public void OpenSteam()
    {
        Application.OpenURL("https://store.steampowered.com/app/3646540/Merge_Rogue/?beta=1");
    }

    public void ExitGame()
    {
        // エディタ上での動作確認用
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        
        Application.Quit();
    }
    /// <summary>
    /// 設定をリセットしてUIに反映します
    /// </summary>
    public void ResetSetting()
    {
        _gameSettingsService.ResetAudioSettings();
        
        // UIスライダーに反映
        var audioSettings = _gameSettingsService.GetAudioSettings();
        bgmSlider.value = audioSettings.bgmVolume;
        seSlider.value = audioSettings.seVolume;
    }

    private void Awake()
    {        
        // ScrollRectの初期化
        foreach(var scrollRect in FindObjectsByType<ScrollRect>(FindObjectsSortMode.None))
        {
            var sr = scrollRect.GetComponentInChildren<ScrollRect>();
            if (sr)
            {
                sr.verticalNormalizedPosition = 1.0f;
                sr.horizontalNormalizedPosition = 0.0f;
            }
        }
        
        // CanvasGroupの初期化
        // foreach (var canvasGroup in canvasGroups)
        // {
        //     _canvasGroupTween.Add(canvasGroup.name, null);
        //     EnableCanvasGroupAsync(canvasGroup.name, false).Forget();
        // }
    }

    private void Start()
    {
        Debug.Log("TitleMenu Start");
        Time.timeScale = 1.0f;
        
        InitializeSettings();
        SetupUIListeners();
        // ToggleVirtualMouse();
        InitializeTitleContent();

        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.DOFade(0.0f, 1.0f);
    }

    private void Update()
    {
        // シード設定をリアルタイムで保存
        if (_gameSettingsService != null)
        {
            _gameSettingsService.GenerateAndSaveSeed(seedInputField.text);
        }
        
        // if (_inputProvider?.UI.ResetCursor.triggered == true)
        //     ResetSelectedGameObject();
        // if (_inputProvider?.UI.ToggleVirtualMouse.triggered == true)
            // ToggleVirtualMouse();
        
        // スクロール操作
        var sr = GetActiveScrollRect();
        if (sr && _inputProvider != null)
        {
            var speed = _inputProvider.GetScrollSpeed();
            var newPos = sr.verticalNormalizedPosition + speed.y * Time.unscaledDeltaTime;
            sr.verticalNormalizedPosition = Mathf.Clamp01(newPos);
        }
    }
    
    /// <summary>
    /// 設定サービスを初期化し、UIに設定値を反映する
    /// </summary>
    private void InitializeSettings()
    {
        // 現在の設定をUIに反映
        var audioSettings = _gameSettingsService.GetAudioSettings();
        bgmSlider.value = audioSettings.bgmVolume;
        seSlider.value = audioSettings.seVolume;
        
        var seedSettings = _gameSettingsService.GetSeedSettings();
        seedInputField.text = seedSettings.seedText;
    }
    
    /// <summary>
    /// UIイベントリスナーを設定する
    /// </summary>
    private void SetupUIListeners()
    {
        // BGM音量変更リスナー
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            _gameSettingsService?.SaveBgmVolume(value);
            BgmManager.Instance.BgmVolume = value;
        });

        // SE音量変更リスナー
        seSlider.onValueChanged.AddListener((value) =>
        {
            _gameSettingsService?.SaveSeVolume(value);
            SeManager.Instance.SeVolume = value;
        });

        // SE音量スライダーのポインターアップイベント（テスト音再生）
        var trigger = seSlider.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entry.callback.AddListener(_ => SeManager.Instance.PlaySe("button"));
        trigger.triggers.Add(entry);
    }
    
    private void InitializeTitleContent()
    {
        // バージョンテキストの初期化
        if (_versionService != null && versionText)
        {
            var versionString = _versionService.GetVersionText();
            versionText.text = versionString;
            
            #if DEMO_PLAY
            Debug.Log("Demo Build");
            #else
            Debug.Log("Full Build");
            #endif
        }
        
        creditText.text = _creditService.GetCreditText();
        UpdateContentSize(creditText, creditContent);
        licenseText.text = _licenseService.GetLicenseText();
        UpdateContentSize(licenseText, licenseContent);
    }
    
    private void UpdateContentSize(TextMeshProUGUI text, RectTransform content)
    {
        var preferredHeight = text.GetPreferredValues().y;
        content.sizeDelta = new Vector2(content.sizeDelta.x, preferredHeight);
    }
    
    /// <summary>
    /// クレジットリンククリックハンドラー（UIイベントから呼び出し可能）
    /// </summary>
    /// <param name="position">クリック位置</param>
    public void OnCreditLinkClick(Vector2 position)
    {
        if (!creditText) return;
        
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(creditText, position, Camera.main);
        if (linkIndex != -1)
        {
            var linkInfo = creditText.textInfo.linkInfo[linkIndex];
            var url = linkInfo.GetLinkID();
            Application.OpenURL(url);
        }
    }
}
