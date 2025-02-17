using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class TitleMenu : MonoBehaviour
{
    public static TitleMenu Instance { get; private set; }
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private CanvasGroup credit;
    [SerializeField] private CanvasGroup license;
    [SerializeField] private CanvasGroup encyclopedia;
    [SerializeField] private DescriptionWindow descriptionWindow;

    public void StartGame()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1.0f, 1.0f).OnComplete(() =>
        {
            SceneManager.LoadScene("MainScene");
        });
    }
    
    public void ShowEncyclopedia()
    {
        encyclopedia.alpha = 1.0f;
        encyclopedia.interactable = true;
        encyclopedia.blocksRaycasts = true;
    }
    
    public void HideEncyclopedia()
    {
        encyclopedia.alpha = 0.0f;
        encyclopedia.interactable = false;
        encyclopedia.blocksRaycasts = false;
    }

    public void ShowCredit()
    {
        credit.alpha = 1.0f;
        credit.interactable = true;
        credit.blocksRaycasts = true;
    }

    public void HideCredit()
    {
        credit.alpha = 0.0f;
        credit.interactable = false;
        credit.blocksRaycasts = false;
    }

    public void ShowLicense()
    {
        license.alpha = 1.0f;
        license.interactable = true;
        license.blocksRaycasts = true;
    }

    public void HideLicense()
    {
        license.alpha = 0.0f;
        license.interactable = false;
        license.blocksRaycasts = false;
    }
    
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
        
        
        HideCredit();
        HideLicense();
        HideEncyclopedia();
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
        Debug.Log("TitleMenu Start");
    }

    private void Update()
    {
        var seed = seedInputField.text.GetHashCode();
        PlayerPrefs.SetInt("Seed", seed);
        PlayerPrefs.SetString("SeedText", seedInputField.text);
    }
}
