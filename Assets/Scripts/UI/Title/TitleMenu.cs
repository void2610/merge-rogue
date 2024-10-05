using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class TitleMenu : MonoBehaviour
{
    [SerializeField]
    private Image fadeImage;
    [SerializeField]
    private Slider bgmSlider;
    [SerializeField]
    private Slider seSlider;
    [SerializeField]
    private TMP_InputField seedInputField;
    [SerializeField]
    private CanvasGroup credit;
    [SerializeField]
    private CanvasGroup license;

    public void StartGame()
    {
        SeManager.Instance.PlaySe("button");
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1.0f, 1.0f).OnComplete(() =>
        {
            SceneManager.LoadScene("MainScene");
        });
    }

    public void ShowCredit()
    {
        PlayButtonSe();
        credit.alpha = 1.0f;
        credit.interactable = true;
        credit.blocksRaycasts = true;
    }

    public void HideCredit()
    {
        PlayButtonSe();
        credit.alpha = 0.0f;
        credit.interactable = false;
        credit.blocksRaycasts = false;
    }

    public void ShowLicense()
    {
        PlayButtonSe();
        license.alpha = 1.0f;
        license.interactable = true;
        license.blocksRaycasts = true;
    }

    public void HideLicense()
    {
        PlayButtonSe();
        license.alpha = 0.0f;
        license.interactable = false;
        license.blocksRaycasts = false;
    }

    public void PlayButtonSe()
    {
        if (Time.time > 0.5f)
            SeManager.Instance.PlaySe("button");
    }

    private void InitPlayerPrefs()
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

    void Awake()
    {
        HideCredit();
        HideLicense();
        if (!PlayerPrefs.HasKey("BgmVolume")) InitPlayerPrefs();
    }

    void Start()
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
            SeManager.Instance.seVolume = value;
        });

        var trigger = seSlider.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>((data) =>
        {
            SeManager.Instance.PlaySe("button");
        }));
        trigger.triggers.Add(entry);

        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.DOFade(0.0f, 1.0f);
    }

    void Update()
    {
        int seed = seedInputField.text.GetHashCode();
        PlayerPrefs.SetInt("Seed", seed);
        PlayerPrefs.SetString("SeedText", seedInputField.text);
    }
}
