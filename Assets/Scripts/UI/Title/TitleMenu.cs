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

public class TitleMenu : MonoBehaviour
{
    public static TitleMenu Instance { get; private set; }
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    [SerializeField] private DescriptionWindow descriptionWindow;
    [SerializeField] private GameObject virtualMouse;
    
    private readonly Dictionary<string, Sequence> _canvasGroupTween = new();

    public GameObject GetTopCanvasGroup() => canvasGroups.Find(c => c.alpha > 0)?.gameObject;
    
    public void ResetSelectedGameObject()
    {
        var topCanvas = GetTopCanvasGroup();
        if (topCanvas)
        {
            var focusSelectable = topCanvas.GetComponentInChildren<FocusSelectable>();
            if (focusSelectable.GetComponent<Selectable>().interactable == true)
                CanvasGroupNavigationLimiter.SetSelectedGameObjectSafe(focusSelectable.gameObject);
        }
        else
        {
            CanvasGroupNavigationLimiter.SetSelectedGameObjectSafe(startButton.gameObject);
        }
    }

    public void ToggleVirtualMouse()
    {
        if (virtualMouse.GetComponent<Image>().enabled)
        {
            virtualMouse.GetComponent<Image>().enabled = false;
            // 仮想マウスデバイスの入力更新を無効化
            InputSystem.DisableDevice(virtualMouse.GetComponent<MyVirtualMouseInput>().virtualMouse);
            EventSystem.current.sendNavigationEvents = true;
        }
        else
        {
            virtualMouse.GetComponent<Image>().enabled = true;
            // 仮想マウスデバイスの入力更新を有効化
            InputSystem.EnableDevice(virtualMouse.GetComponent<MyVirtualMouseInput>().virtualMouse);
            EventSystem.current.sendNavigationEvents = false;
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
        CanvasGroupNavigationLimiter.SetSelectedGameObjectSafe(null);
        
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
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
       
       
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
    }
}
