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
    }
}
