using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Slider bgmSlider;
    [SerializeField]
    private Slider seSlider;
    [SerializeField]
    private Image fadeImage;
    [SerializeField]
    private CanvasGroup pauseMenu;
    [SerializeField]
    private CanvasGroup gameOver;
    [SerializeField]
    private CanvasGroup clear;
    [SerializeField]
    private CanvasGroup levelUpOptions;
    [SerializeField]
    private CanvasGroup shopOptions;
    [SerializeField]
    private TextMeshProUGUI coinText;
    [SerializeField]
    private TextMeshProUGUI stageText;
    [SerializeField]
    private TextMeshProUGUI expText;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    public Slider hpSlider;
    [SerializeField]
    public TextMeshProUGUI hpText;

    public int remainingLevelUps;
    // private Player player => GameManager.Instance.player;

    public void EnablePauseMenu(bool e)
    {
        if (e)
        {
            pauseMenu.alpha = 1;
            pauseMenu.interactable = true;
            pauseMenu.blocksRaycasts = true;
            Time.timeScale = 0;
        }
        else
        {
            pauseMenu.alpha = 0;
            pauseMenu.interactable = false;
            pauseMenu.blocksRaycasts = false;
            Time.timeScale = 1;
        }
    }

    public void EnableGameOver(bool e)
    {
        if (e)
        {
            gameOver.alpha = 1;
            gameOver.interactable = true;
            gameOver.blocksRaycasts = true;
        }
        else
        {
            gameOver.alpha = 0;
            gameOver.interactable = false;
            gameOver.blocksRaycasts = false;
        }
    }

    public void EnableClear(bool e)
    {
        if (e)
        {
            clear.alpha = 1;
            clear.interactable = true;
            clear.blocksRaycasts = true;
        }
        else
        {
            clear.alpha = 0;
            clear.interactable = false;
            clear.blocksRaycasts = false;
        }
    }

    public void EnableLevelUpOptions(bool e)
    {
        if (e)
        {
            levelUpOptions.alpha = 1;
            levelUpOptions.interactable = true;
            levelUpOptions.blocksRaycasts = true;
        }
        else
        {
            levelUpOptions.alpha = 0;
            levelUpOptions.interactable = false;
            levelUpOptions.blocksRaycasts = false;
        }
    }

    public void EnableShopOptions(bool e)
    {
        if (e)
        {
            shopOptions.alpha = 1;
            shopOptions.interactable = true;
            shopOptions.blocksRaycasts = true;
        }
        else
        {
            shopOptions.alpha = 0;
            shopOptions.interactable = false;
            shopOptions.blocksRaycasts = false;
        }
    }

    public void UpdateCoinText(int amount)
    {
        coinText.text = "coin: " + amount.ToString();
    }

    public void UpdateExpText(int now, int max)
    {
        expText.text = "exp: " + now + "/" + max;
    }

    public void UpdateLevelText(int level)
    {
        levelText.text = "level: " + level;
    }

    public void UpdateStageText(int stage)
    {
        int s = Mathf.Max(1, stage);
        stageText.text = "stage: " + s;
    }

    public void OnClickShopExit()
    {
        SeManager.Instance.PlaySe("button");
        EnableShopOptions(false);
        Time.timeScale = 1;
        GameManager.Instance.ChangeState(GameManager.GameState.StageMoving);
    }

    public void OnClickPause()
    {
        SeManager.Instance.PlaySe("button");
        EnablePauseMenu(true);
    }

    public void OnClickResume()
    {
        SeManager.Instance.PlaySe("button");
        EnablePauseMenu(false);
    }

    public void OnClickTitle()
    {
        SeManager.Instance.PlaySe("button");
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("TitleScene"));
    }

    public void OnClickRetry()
    {
        SeManager.Instance.PlaySe("button");
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("MainScene"));
    }

    void Awake()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        seSlider.value = PlayerPrefs.GetFloat("SeVolume", 1.0f);

        EnablePauseMenu(false);
        EnableGameOver(false);
        EnableClear(false);
        EnableLevelUpOptions(false);
        EnableShopOptions(false);
    }

    private void Start()
    {
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            BgmManager.Instance.BgmVolume = value;
        });

        seSlider.onValueChanged.AddListener((value) =>
        {
            SeManager.Instance.seVolume = value;
        });

        var trigger = seSlider.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entry.callback.AddListener(_ => SeManager.Instance.PlaySe("button"));
        trigger.triggers.Add(entry);

        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.DOFade(0, 2f);
    }
}
