using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TitlePresenter : MonoBehaviour
{
    private class TitleButtonData
    {
        public readonly string ButtonText;
        public readonly System.Action OnClickAction;
        public TitleButtonData(string buttonText, System.Action onClickAction)
        {
            ButtonText = buttonText;
            OnClickAction = onClickAction;
        }
    }
    
    [SerializeField] private GameObject titleButtonPrefab;
    [SerializeField] private Transform titleButtonContainer;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    [SerializeField] private Image fadeImage;
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private TextMeshProUGUI licenseText;
    [Header("Buttons")]
    [SerializeField] private Button twitterButton;
    [SerializeField] private Button steamButton;
    [SerializeField] private Button closeEncyclopediaButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button closeCreditButton;
    [SerializeField] private Button closeLicenseButton;
    
    private List<TitleButtonData> _titleButtons;
    private CanvasGroupSwitcher _canvasGroupSwitcher;
    private IInputProvider _inputProvider;
    private IVirtualMouseService _virtualMouseService;
    private SettingsManager _settingsManager;
    private ICreditService _creditService;
    private ILicenseService _licenseService;
    private IVersionService _versionService;
    private GameObject _startButton;
    
    [Inject]
    public void Construct(IInputProvider inputProvider, IVirtualMouseService virtualMouseService, ICreditService creditService, ILicenseService licenseService, IVersionService versionService)
    {
        _inputProvider = inputProvider;
        _virtualMouseService = virtualMouseService;
        _creditService = creditService;
        _licenseService = licenseService;
        _versionService = versionService;
    }
    
    private void ToggleVirtualMouse()
    {
        _virtualMouseService?.ToggleVirtualMouse();
    }
    
    private async UniTask EnableCanvasGroupWithReset(string canvasName, bool enable)
    {
        await _canvasGroupSwitcher.EnableCanvasGroupAsync(canvasName, enable);
        ResetSelectedGameObject();
    }
    
    private void ResetSelectedGameObject()
    {
        var topCanvas = _canvasGroupSwitcher.GetTopCanvasGroup();
        if (topCanvas)
        {
            var focusSelectable = topCanvas.GetComponentInChildren<FocusSelectable>();
            if (focusSelectable.GetComponent<Selectable>().interactable)
                SelectionCursor.SetSelectedGameObjectSafe(focusSelectable.gameObject);
        }
        else
        {
            SelectionCursor.SetSelectedGameObjectSafe(_startButton.gameObject);
        }
    }
    
    private ScrollRect GetActiveScrollRect()
    {
        var topCanvas = _canvasGroupSwitcher.GetTopCanvasGroup();
        if (topCanvas)
        {
            var scrollRect = topCanvas.GetComponentInChildren<ScrollRect>();
            if (scrollRect) return scrollRect;
        }
        return null;
    }
    
    private async UniTask StartGame()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        await fadeImage.DOFade(1.0f, 1.0f);
        TitleFunctions.StartGame();
    }
    
    private void UpdateContentSize(TextMeshProUGUI text)
    {
        var preferredHeight = text.GetPreferredValues().y;
        var content = text.GetComponentInParent<RectTransform>();
        var parentRect = content.parent.GetComponent<RectTransform>();
        content.sizeDelta = new Vector2(content.sizeDelta.x, preferredHeight);
        parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, Mathf.Max(parentRect.sizeDelta.y, preferredHeight + 20));
    }    
    
    private void SetUpTitleButtons()
    {
        _titleButtons = new List<TitleButtonData>
        {
            new("Start Game", () => StartGame().Forget()),
            new("Encyclopedia", () => EnableCanvasGroupWithReset("Encyclopedia", true).Forget()),
            new("Settings", () => EnableCanvasGroupWithReset("Setting", true).Forget()),
            new("Credits", () => EnableCanvasGroupWithReset("Credit", true).Forget()),
            new("Licenses", () => EnableCanvasGroupWithReset("License", true).Forget()),
            new("Exit", TitleFunctions.ExitGame)
        };

        var buttons = new List<Selectable>();
        
        foreach (var buttonData in _titleButtons)
        {
            var buttonObject = Instantiate(titleButtonPrefab, titleButtonContainer);
            var button = buttonObject.GetComponent<Button>();
            button.GetComponentInChildren<TextMeshProUGUI>().text = buttonData.ButtonText;
            button.onClick.AddListener(() => buttonData.OnClickAction.Invoke());
            buttons.Add(button);
            var n = button.navigation;
            n.selectOnRight = twitterButton;
            button.navigation = n;
        }
        _startButton = buttons[0].gameObject;
        _startButton.AddComponent<FocusSelectable>();
        buttons.SetVerticalNavigation(true);
        
        twitterButton.onClick.AddListener(TitleFunctions.OpenTwitter);
        steamButton.onClick.AddListener(TitleFunctions.OpenSteam);
        closeEncyclopediaButton.onClick.AddListener(() => EnableCanvasGroupWithReset("Encyclopedia", false).Forget());
        closeSettingsButton.onClick.AddListener(() => EnableCanvasGroupWithReset("Setting", false).Forget());
        closeCreditButton.onClick.AddListener(() => EnableCanvasGroupWithReset("Credit", false).Forget());
        closeLicenseButton.onClick.AddListener(() => EnableCanvasGroupWithReset("License", false).Forget());
        
        var tn = twitterButton.navigation;
        tn.selectOnLeft = _startButton.GetComponent<Selectable>();
        tn.selectOnDown = steamButton;
        twitterButton.navigation = tn;
        var sn = steamButton.navigation;
        sn.selectOnLeft = _startButton.GetComponent<Selectable>();
        sn.selectOnUp = twitterButton;
        steamButton.navigation = sn;
    }
    
    private void Awake()
    {
        Time.timeScale = 1.0f;
        _canvasGroupSwitcher = new CanvasGroupSwitcher(canvasGroups);
        
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
        
        SetUpTitleButtons();

        versionText.text = _versionService.GetVersionText();
        creditText.text = _creditService.GetCreditText();
        UpdateContentSize(creditText);
        licenseText.text = _licenseService.GetLicenseText();
        UpdateContentSize(licenseText);
        
        // フェードイン
        fadeImage.DOFade(0, 0.5f);
    }

    private async UniTaskVoid Start()
    {
        ToggleVirtualMouse();
        await UniTask.DelayFrame(1); // フェードイン後にUIを初期化
        ResetSelectedGameObject();

        BgmManager.Instance.Stop().Forget();
    }

    private void Update()
    {
        if (_inputProvider?.UI.ResetCursor.triggered == true)
            ResetSelectedGameObject();
        
        if (_inputProvider?.UI.ToggleVirtualMouse.triggered == true)
            ToggleVirtualMouse();
        
        // スクロール操作
        var sr = GetActiveScrollRect();
        if (sr && _inputProvider != null)
        {
            var speed = _inputProvider.GetScrollSpeed();
            var newPos = sr.verticalNormalizedPosition + speed.y * Time.unscaledDeltaTime;
            sr.verticalNormalizedPosition = Mathf.Clamp01(newPos);
        }
    }
}
