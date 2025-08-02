using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearScreenView : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private Image image;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private GameObject demoClearUI;

    public void Show(int act, bool isDemoClear = false)
    {
        image.sprite = sprites[act];
        demoClearUI.SetActive(isDemoClear);
        nextButton.gameObject.SetActive(!isDemoClear);

        nextButton.GetComponent<FocusSelectable>().enabled = !isDemoClear;
        titleButton.GetComponent<FocusSelectable>().enabled = isDemoClear;
        
        UIManager.Instance.EnableCanvasGroup("Clear", true);
    }
    
    private void OnClickNext()
    {
        // このウィンドウを閉じて、更新されたマップを開く
        UIManager.Instance.EnableCanvasGroup("Clear", false);
        UIManager.Instance.EnableCanvasGroup("Map", true);
    }
    
    private void Start()
    {
        nextButton.onClick.AddListener(OnClickNext);
        titleButton.onClick.AddListener(() => UIManager.Instance.OnClickTitle());

        var nn = nextButton.navigation;
        nn.mode = Navigation.Mode.Explicit;
        nn.selectOnRight = titleButton;
        nn.selectOnLeft = titleButton;
        nextButton.navigation = nn;
        var tt = titleButton.navigation;
        tt.mode = Navigation.Mode.Explicit;
        tt.selectOnRight = nextButton;
        tt.selectOnLeft = nextButton;
        titleButton.navigation = tt;
    }
}
