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
    private Button _startButton;
    
    [Inject]
    public void Construct(IInputProvider inputProvider, IVirtualMouseService virtualMouseService)
    {
        _inputProvider = inputProvider;
        _virtualMouseService = virtualMouseService;
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
    
    private void SetUpTitleButtons()
    {
        _titleButtons = new List<TitleButtonData>
        {
            new TitleButtonData("Start Game", () => StartGame().Forget()),
            new TitleButtonData("Encyclopedia", () => EnableCanvasGroupWithReset("Encyclopedia", true).Forget()),
            new TitleButtonData("Settings", () => EnableCanvasGroupWithReset("Setting", true).Forget()),
            new TitleButtonData("Credits", () => EnableCanvasGroupWithReset("Credit", true).Forget()),
            new TitleButtonData("Licenses", () => EnableCanvasGroupWithReset("License", true).Forget()),
            new TitleButtonData("Exit", TitleFunctions.ExitGame)
        };
        
        foreach (var buttonData in _titleButtons)
        {
            var buttonObject = Instantiate(titleButtonPrefab, titleButtonContainer);
            var button = buttonObject.GetComponent<Button>();
            button.GetComponentInChildren<TextMeshProUGUI>().text = buttonData.ButtonText;
            button.onClick.AddListener(() => buttonData.OnClickAction.Invoke());
        }
        _startButton = titleButtonContainer.GetChild(0).GetComponent<Button>();
        _startButton.gameObject.AddComponent<FocusSelectable>();
        
        twitterButton.onClick.AddListener(TitleFunctions.OpenTwitter);
        steamButton.onClick.AddListener(TitleFunctions.OpenSteam);
        closeEncyclopediaButton.onClick.AddListener(() => EnableCanvasGroupWithReset("Encyclopedia", false).Forget());
        closeSettingsButton.onClick.AddListener(() => EnableCanvasGroupWithReset("Setting", false).Forget());
        closeCreditButton.onClick.AddListener(() => EnableCanvasGroupWithReset("Credit", false).Forget());
        closeLicenseButton.onClick.AddListener(() => EnableCanvasGroupWithReset("License", false).Forget());
    }

    private async UniTask StartGame()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        await fadeImage.DOFade(1.0f, 1.0f);
        TitleFunctions.StartGame();
    }
    
    private void Awake()
    {
        Time.timeScale = 1.0f;
        _canvasGroupSwitcher = new CanvasGroupSwitcher(canvasGroups);
        
        SetUpTitleButtons();
        
        // フェードイン
        fadeImage.DOFade(0, 0.5f);
    }

    private void Start()
    {
        ToggleVirtualMouse();
        ResetSelectedGameObject();
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
