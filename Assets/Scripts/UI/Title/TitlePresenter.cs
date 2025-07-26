using System.Collections.Generic;
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
    [SerializeField] private Image fadeImage;
    [SerializeField] private Button twitterButton;
    [SerializeField] private Button steamButton;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    
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
    }
    
    private void Awake()
    {
        _canvasGroupSwitcher = new CanvasGroupSwitcher(canvasGroups);
        
        // ボタンデータを初期化
        _titleButtons = new List<TitleButtonData>
        {
            new TitleButtonData("Start Game", TitleFunctions.StartGame),
            new TitleButtonData("Encyclopedia", () => _canvasGroupSwitcher.EnableCanvasGroup("Encyclopedia", true)),
            new TitleButtonData("Settings", () => _canvasGroupSwitcher.EnableCanvasGroup("Settings", true)),
            new TitleButtonData("Credits", () => _canvasGroupSwitcher.EnableCanvasGroup("Credit", true)),
            new TitleButtonData("Licenses", () => _canvasGroupSwitcher.EnableCanvasGroup("License", true)),
            new TitleButtonData("Exit", TitleFunctions.ExitGame)
        };
        
        SetUpTitleButtons();
        
        // フェードイン
        fadeImage.DOFade(0, 0.5f).SetUpdate(true).OnComplete(() =>
        {
            fadeImage.gameObject.SetActive(false);
        }).Play();
    }

    private void Start()
    {
        ToggleVirtualMouse();
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
