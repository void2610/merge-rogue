using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.LowLevel;
using VContainer;
using SyskenTLib.LicenseMaster;

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
    
    // Title content UI elements
    [SerializeField] private TextAsset creditTextAsset;
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private RectTransform creditContent;
    [SerializeField] private LicenseManager licenseManager;
    [SerializeField] private TextMeshProUGUI licenseText;
    [SerializeField] private RectTransform licenseContent;
    [SerializeField] private TextMeshProUGUI versionText;
    
    private readonly Dictionary<string, Sequence> _canvasGroupTween = new();
    
    // Service dependencies
    private ICreditService _creditService;
    private ILicenseService _licenseService;
    private IVersionService _versionService;
    
    [Inject]
    public void InjectDependencies(
        ICreditService creditService,
        ILicenseService licenseService,
        IVersionService versionService)
    {
        this._creditService = creditService;
        this._licenseService = licenseService;
        this._versionService = versionService;
    }

    private GameObject GetTopCanvasGroup() => canvasGroups.Find(c => c.alpha > 0)?.gameObject;
    private bool IsVirtualMouseActive() => virtualMouse.GetComponent<MyVirtualMouseInput>().isActive;

    private void ResetSelectedGameObject()
    {
        var topCanvas = GetTopCanvasGroup();
        if (topCanvas)
        {
            var focusSelectable = topCanvas.GetComponentInChildren<FocusSelectable>();
            if (focusSelectable.GetComponent<Selectable>().interactable == true)
                SelectionCursor.SetSelectedGameObjectSafe(focusSelectable.gameObject);
        }
        else
        {
            SelectionCursor.SetSelectedGameObjectSafe(startButton.gameObject);
        }
    }

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
    /// 仮想マウスを任意の位置へ移動させるメソッド
    /// </summary>
    /// <param name="newPosition">移動先のスクリーン座標（ピクセル単位）</param>
    private void MoveVirtualMouseToCenter()
    {
        var vm = virtualMouse.GetComponent<MyVirtualMouseInput>();
        var centerPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        InputState.Change(vm.virtualMouse.position, centerPos);
        // ソフトウェアカーソル（UI上の表示）の位置も更新（存在する場合）
        // もしCanvasのスケール等を考慮する必要があるなら、ここで座標変換を行う
        virtualMouse.transform.position = Vector2.zero;
    }

    private void ToggleVirtualMouse()
    {
        if (IsVirtualMouseActive())
        {
            virtualMouse.GetComponent<MyVirtualMouseInput>().isActive = false;
            EventSystem.current.sendNavigationEvents = true;
            virtualMouse.transform.position = new Vector2(-1000, -1000);
        }
        else
        {
            virtualMouse.GetComponent<MyVirtualMouseInput>().isActive = true;
            EventSystem.current.sendNavigationEvents = false;
            MoveVirtualMouseToCenter();
        }
    }
    
    private async UniTaskVoid EnableCanvasGroupAsync(string canvasName, bool e)
    {
        var cg = canvasGroups.Find(c => c.name == canvasName);
        if (!cg) return;
        if (_canvasGroupTween[canvasName].IsActive()) return;
        
        // アニメーション中は操作をブロック
        cg.interactable = false;
        cg.blocksRaycasts = false;
        
        var seq = DOTween.Sequence();
        seq.SetUpdate(true).Forget();
        if (e)
        {
            seq.Join(cg.transform.DOMoveY(-0.45f, 0).SetRelative(true)).Forget();
            seq.Join(cg.transform.DOMoveY(0.45f, 0.2f).SetRelative(true).SetEase(Ease.OutBack)).Forget();
            seq.Join(cg.DOFade(1, 0.2f)).Forget();
        }
        else
        {
            seq.Join(cg.DOFade(0, 0.2f)).Forget();
        }
        
        _canvasGroupTween[canvasName] = seq;
        SelectionCursor.SetSelectedGameObjectSafe(null);
        
        await seq.AsyncWaitForCompletion();
        
        // inputGuide.UpdateText(IsAnyCanvasGroupEnabled() ? InputGuide.InputGuideType.Navigate : InputGuide.InputGuideType.Merge);
        _canvasGroupTween[canvasName] = null;
        cg.interactable = e;
        cg.blocksRaycasts = e;
        
        // FocusSelectableがアタッチされているオブジェクトがあればフォーカス
        ResetSelectedGameObject();
    }
    
    public void StartGame()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1.0f, 1.0f).OnComplete(() =>
        {
            SceneManager.LoadScene("MainScene");
        });
    }
    
    public void ShowEncyclopedia() => EnableCanvasGroupAsync("Encyclopedia", true).Forget();
    public void HideEncyclopedia() => EnableCanvasGroupAsync("Encyclopedia", false).Forget();
    public void ShowCredit() => EnableCanvasGroupAsync("Credit", true).Forget();
    public void HideCredit() => EnableCanvasGroupAsync("Credit", false).Forget();
    public void ShowLicense() => EnableCanvasGroupAsync("License", true).Forget();
    public void HideLicense() => EnableCanvasGroupAsync("License", false).Forget();
    
    public void ShowDescriptionWindow(object o, GameObject g)
    {
        descriptionWindow.ShowWindowWithHoverCheck(o, g).Forget();
    }
    
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
    private static void InitPlayerPrefs()
    {
        PlayerPrefs.SetFloat("BgmVolume", 1.0f);
        PlayerPrefs.SetFloat("SeVolume", 1.0f);

        PlayerPrefs.SetInt("Seed", 0);
        PlayerPrefs.SetString("SeedText", "");
    }

    public void ResetSetting()
    {
        PlayerPrefs.SetFloat("BgmVolume", 1.0f);
        PlayerPrefs.SetFloat("SeVolume", 1.0f);
        bgmSlider.value = 1.0f;
        seSlider.value = 1.0f;
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
        foreach (var canvasGroup in canvasGroups)
        {
            _canvasGroupTween.Add(canvasGroup.name, null);
            EnableCanvasGroupAsync(canvasGroup.name, false).Forget();
        }
        
        if (!PlayerPrefs.HasKey("BgmVolume")) InitPlayerPrefs();
    }

    private void Start()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        seSlider.value = PlayerPrefs.GetFloat("SeVolume", 1.0f);

        seedInputField.text = PlayerPrefs.GetString("SeedText", "");

        bgmSlider.onValueChanged.AddListener((value) =>
        {
            BgmManager.Instance.BgmVolume = value;
        });

        seSlider.onValueChanged.AddListener((value) =>
        {
            SeManager.Instance.SeVolume = value;
        });

        var trigger = seSlider.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>((data) =>
        {
            SeManager.Instance.PlaySe("button");
        }));
        trigger.triggers.Add(entry);
        
        Time.timeScale = 1.0f;

        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.DOFade(0.0f, 1.0f);
        
        // 仮想マウスを無効化
        ToggleVirtualMouse();
        
        // Initialize title content using services
        InitializeTitleContent();
        
        Debug.Log("TitleMenu Start");
    }

    private void Update()
    {
        var seed = seedInputField.text.GetHashCode();
        PlayerPrefs.SetInt("Seed", seed);
        PlayerPrefs.SetString("SeedText", seedInputField.text);
        
        if (InputProvider.Instance.UI.ResetCursor.triggered)
            ResetSelectedGameObject();
        if (InputProvider.Instance.UI.ToggleVirtualMouse.triggered)
            ToggleVirtualMouse();
        
        // スクロール操作
        var sr = GetActiveScrollRect();
        if (sr)
        {
            var speed = InputProvider.Instance.GetScrollSpeed();
            var newPos = sr.verticalNormalizedPosition + speed.y * Time.unscaledDeltaTime;
            sr.verticalNormalizedPosition = Mathf.Clamp01(newPos);
        }
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
        
        // クレジットテキストの初期化
        if (_creditService != null && creditTextAsset != null && creditText)
        {
            var creditString = _creditService.GetCreditText(creditTextAsset);
            creditText.text = creditString;
            UpdateContentSize(creditText, creditContent);
        }
        
        // ライセンステキストの初期化
        if (_licenseService != null && licenseManager != null && licenseText)
        {
            var licenseString = _licenseService.GetLicenseText(licenseManager);
            licenseText.text = licenseString;
            UpdateContentSize(licenseText, licenseContent);
        }
    }
    
    private void UpdateContentSize(TextMeshProUGUI text, RectTransform content)
    {
        if (!text || !content) return;
        
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
