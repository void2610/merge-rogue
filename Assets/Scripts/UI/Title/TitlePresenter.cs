using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitlePresenter : MonoBehaviour
{
    private class TitleButtonData
    {
        public string ButtonText;
        public System.Action OnClickAction;
        public TitleButtonData(string buttonText, System.Action onClickAction)
        {
            ButtonText = buttonText;
            OnClickAction = onClickAction;
        }
    }
    
    [SerializeField] private GameObject titleButtonPrefab;
    [SerializeField] private Transform titleButtonContainer;
    [SerializeField] private Image fadeImage;
    [SerializeField] private List<CanvasGroup> canvasGroups;
    
    private List<TitleButtonData> _titleButtons = new List<TitleButtonData>
    {
        new TitleButtonData("Start Game", TitleFunctions.StartGame),
        new TitleButtonData("Encyclopedia", _canvasGroupSwitcher.EnableCanvasGroup("Encyclopedia", true)),
        new TitleButtonData("Settings", _canvasGroupSwitcher.EnableCanvasGroup("Settings", true)),
        new TitleButtonData("Credits", _canvasGroupSwitcher.EnableCanvasGroup("Credit", true)),
        new TitleButtonData("Licenses", _canvasGroupSwitcher.EnableCanvasGroup("License", true)),
        new TitleButtonData("Open Twitter", TitleFunctions.OpenTwitter),
        new TitleButtonData("Open Steam", TitleFunctions.OpenSteam),
        new TitleButtonData("Exit Game", TitleFunctions.ExitGame)
    };
    private CanvasGroupSwitcher _canvasGroupSwitcher;
    
    private void SetUpTitleButtons()
    {
        foreach (var buttonData in _titleButtons)
        {
            var buttonObject = Instantiate(titleButtonPrefab, titleButtonContainer);
            var button = buttonObject.GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = buttonData.ButtonText;
            button.onClick.AddListener(() => buttonData.OnClickAction.Invoke());
        }
    }
    
    private void Awake()
    {
        _canvasGroupSwitcher = new CanvasGroupSwitcher(canvasGroups);
        SetUpTitleButtons();
        
        // フェードイン
        fadeImage.DOFade(0, 0.5f).SetUpdate(true).OnComplete(() =>
        {
            fadeImage.gameObject.SetActive(false);
        }).Play();
    }

}
