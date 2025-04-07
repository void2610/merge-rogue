using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using DG.Tweening;
using R3;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private SeedText pauseSeedText;
    [SerializeField] private SeedText gameoverSeedText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private DescriptionWindow descriptionWindow;
    [SerializeField] private InputGuide inputGuide;
    [SerializeField] private Volume volume;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Shop shop;
    [SerializeField] private Treasure treasure;
    [SerializeField] private GameObject mergeArea;
    [SerializeField] private GameObject virtualMouse;
    [SerializeField] private Transform ballUIContainer;
    [SerializeField] private Transform relicContainer;
    [SerializeField] private Transform playerStatusUI;
    [SerializeField] private Transform enemyStatusUIContainer;
    
    private static CursorStateType _cursorState = CursorStateType.Merge;
    private Selectable _firstStatusEffectUI;

    public bool IsPaused { get; private set; } = false;
    public bool IsMapOpened { get; private set; } = false;
    public bool IsTutorialOpened { get; set; } = false;
    public int remainingLevelUps;
    
    private readonly Dictionary<string, Sequence> _canvasGroupTween = new();
    
    public Transform PlayerStatusUI => playerStatusUI;
    public Transform EnemyStatusUIContainer => enemyStatusUIContainer;
    public Canvas GetUICanvas() => uiCanvas;
    public Camera GetUICamera() => uiCamera;
    public Transform GetEnemyUIContainer() => uiCanvas.transform.Find("EnemyStatusUIContainer");
    public bool IsVirtualMouseActive() => virtualMouse.GetComponent<MyVirtualMouseInput>().isActive;
    public void ShowRelicDescriptionWindow(RelicData r, GameObject g) => descriptionWindow.ShowWindowWithHoverCheck(r, g).Forget();

    public void ShowBallDescriptionWindow(BallData b, GameObject g, int level) =>
        descriptionWindow.ShowWindowWithHoverCheck(b, g, level).Forget();

    public void SetSeedText(string seed)
    {
        pauseSeedText.SetText(seed);
        gameoverSeedText.SetText(seed);
    }
    
    public void ToggleCursorState()
    {
        _cursorState = _cursorState.Toggle();
        ResetSelectedGameObject();
    }

    public void ToggleCursorState(CursorStateType state)
    {
        _cursorState = state;
        ResetSelectedGameObject(true);
    }
    
    public void EnableCanvasGroup(string canvasName, bool e) => EnableCanvasGroupAsync(canvasName, e).Forget();

    public bool IsEnableCanvasGroup(string canvasName)
    {
        if (canvasGroups == null || canvasGroups.Count == 0) return false;
        return canvasGroups.Find(c => c.name == canvasName).alpha > 0;
    }

    public bool IsAnyCanvasGroupEnabled()
    {
        if (canvasGroups == null || canvasGroups.Count == 0) return false;
        return canvasGroups.Exists(c => c.alpha > 0);
    }

    public GameObject GetTopCanvasGroup()
    { 
        if (!IsAnyCanvasGroupEnabled()) return null;
        return canvasGroups.Find(c => c.alpha > 0)?.gameObject;  
    }
    private void UpdateStageText(int stage) => stageText.text = "stage: " + Mathf.Max(1, stage + 1);
    private void UpdateCoinText(System.Numerics.BigInteger amount) => coinText.text = "coin: " + amount;

    public void ResetSelectedGameObject(bool isToggle = false)
    {
        var topCanvasGroup = GetTopCanvasGroup();
        if (topCanvasGroup && !isToggle && _cursorState == CursorStateType.Merge)
        {
            var focusSelectable = topCanvasGroup.GetComponentInChildren<FocusSelectable>();
            if (!focusSelectable) return;
            CanvasGroupNavigationLimiter.SetSelectedGameObjectSafe(focusSelectable.gameObject);
        }
        else
        {
            // ウィンドウがない時はcursorStateに従って選択をリセット
            if (_cursorState == CursorStateType.StatusEffect && !_firstStatusEffectUI)
            {
                ToggleCursorState(CursorStateType.Merge);
                ResetSelectedGameObject();
                return;
            }
            
            var target = _cursorState switch
            {
                CursorStateType.Merge => mergeArea,
                CursorStateType.Ball => ballUIContainer.GetChild(0).gameObject,
                CursorStateType.Relic => relicContainer.GetChild(0).gameObject,
                CursorStateType.StatusEffect => _firstStatusEffectUI.gameObject,
                _ => null,
            };

            CanvasGroupNavigationLimiter.SetSelectedGameObjectSafe(target);
        }
    }
    
    /// <summary>
    /// 仮想マウスを任意の位置へ移動させるメソッド
    /// </summary>
    /// <param name="newPosition">移動先のスクリーン座標（ピクセル単位）</param>
    public void MoveVirtualMouseToCenter()
    {
        var vm = virtualMouse.GetComponent<MyVirtualMouseInput>();
        var centerPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        InputState.Change(vm.virtualMouse.position, centerPos);
        // ソフトウェアカーソル（UI上の表示）の位置も更新（存在する場合）
        // もしCanvasのスケール等を考慮する必要があるなら、ここで座標変換を行う
        virtualMouse.transform.position = Vector2.zero;
    }

    public void ToggleVirtualMouse()
    {
        if (IsVirtualMouseActive())
        {
            virtualMouse.GetComponent<MyVirtualMouseInput>().isActive = false;
            EventSystem.current.sendNavigationEvents = true;
        }
        else
        {
            virtualMouse.GetComponent<MyVirtualMouseInput>().isActive = true;
            EventSystem.current.sendNavigationEvents = false;
            MoveVirtualMouseToCenter();
        }
    }
    
    public void SetVirtualMousePosition(Vector2 pos)
    {
        var rectTransform = virtualMouse.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
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
        
        await seq.AsyncWaitForCompletion();
        
        inputGuide.UpdateText(IsAnyCanvasGroupEnabled() ? InputGuide.InputGuideType.Navigate : InputGuide.InputGuideType.Merge);
        _canvasGroupTween[canvasName] = null;
        cg.interactable = e;
        cg.blocksRaycasts = e;
        
        // FocusSelectableがアタッチされているオブジェクトがあればフォーカス
        CanvasGroupNavigationLimiter.SetSelectedGameObjectSafe(null);
        ResetSelectedGameObject();
        descriptionWindow.HideWindowFromNavigation();
    }

    private void UpdateExpText(int now, int max)
    {
        if (GameManager.Instance.Player.Level >= Player.MAX_LEVEL)
        {
            expText.text = "max level";
            return;
        }
        expText.text = "exp: " + now + "/" + max;
    }

    public void OnClickPauseButton()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0 : GameManager.Instance.TimeScale;
        EnableCanvasGroup("Pause", IsPaused);
    }

    public void OnClickSpeedButton()
    {
        GameManager.Instance.ChangeTimeScale();
    }
    
    public void OnClickMapButton()
    {
        IsMapOpened = !IsMapOpened;
        EnableCanvasGroup("Map", IsMapOpened);
    }
    
    public void OnClickMapButtonForce(bool e)
    {
        if (IsMapOpened == e) return;
        IsMapOpened = e;
        EnableCanvasGroup("Map", e);
    }
    
    public void OnClickTutorialButton()
    {
        IsTutorialOpened = !IsTutorialOpened;
        EnableCanvasGroup("Tutorial", IsTutorialOpened);
    }

    public void OnClickTitle()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("TitleScene")).SetUpdate(true);
    }

    public void OnClickRetry()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("MainScene")).SetUpdate(true);
    }
    
    public void OpenSteam()
    {
        Application.OpenURL("https://store.steampowered.com/app/3646540/Merge_Rogue/?beta=1");
    }
    
    private void UpdateStatusEffectUINavigation()
    {
        var seUIs = new List<StatusEffectUI>(){playerStatusUI.GetComponent<StatusEffectUI>()};
        seUIs.AddRange(enemyStatusUIContainer.GetComponentsInChildren<StatusEffectUI>().ToList());
        var selectables = new List<Selectable>();
        foreach (var seUI in seUIs)
        {
           selectables.AddRange(seUI.GetStatusEffectIcons());
        }
        if (selectables.Count == 0) return;
        selectables.SetNavigation();
        _firstStatusEffectUI = selectables[0];
    }
    
    private void Fade(bool e)
    {
        if (e) fadeImage.DOFade(1, 2f).SetUpdate(true);
        else fadeImage.DOFade(0, 2f).SetUpdate(true);
    }
    
    private void SetVignette(float value)
    {
        if(!volume.profile.TryGet(out Vignette vignette)) return;
        vignette.intensity.value = value;
    }

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
        
        bgmSlider.value = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        seSlider.value = PlayerPrefs.GetFloat("SeVolume", 1.0f);

        foreach (var canvasGroup in canvasGroups)
        {
            _canvasGroupTween.Add(canvasGroup.name, null);
            if(canvasGroup.name != "Treasure")
                EnableCanvasGroup(canvasGroup.name, false);
        }
        _cursorState = CursorStateType.Merge;
    }

    private void Start()
    {
        GameManager.Instance.Coin.Subscribe(UpdateCoinText).AddTo(this);
        GameManager.Instance.StageManager.CurrentStageCount.Subscribe(UpdateStageText).AddTo(this);
        GameManager.Instance.Player.Exp.Subscribe((v) => UpdateExpText(v, GameManager.Instance.Player.MaxExp)).AddTo(this);
        GameManager.Instance.Player.Health.Subscribe((v) =>
        {
            hpSlider.value = v;
            hpText.text = v + "/" + GameManager.Instance.Player.MaxHealth;
            if (v < 30) SetVignette(((30.0f-v)/30.0f)*0.3f);
            else SetVignette(0);
        }).AddTo(this);
        GameManager.Instance.Player.MaxHealth.Subscribe((v) =>
        {
            hpSlider.maxValue = v;
            hpText.text = GameManager.Instance.Player.Health.Value + "/" + v;
        }).AddTo(this);
        
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            BgmManager.Instance.BgmVolume = value;
        });
        seSlider.onValueChanged.AddListener((value) =>
        {
            SeManager.Instance.SeVolume = value;
        });

        var trigger = seSlider.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entry.callback.AddListener(_ => SeManager.Instance.PlaySe("button"));
        trigger.triggers.Add(entry);

        fadeImage.color = new Color(0, 0, 0, 1);
        Fade(false);
        
        ToggleVirtualMouse();
    }

    private void Update()
    {
        UpdateStatusEffectUINavigation();
    }
}
