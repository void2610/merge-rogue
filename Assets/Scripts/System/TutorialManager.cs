using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("チュートリアルデータ")]
    [SerializeField] private List<TutorialPageData> tutorialPages = new List<TutorialPageData>();
    
    [Header("UI要素")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image tutorialImage;
    
    // プライベートフィールド
    private int _currentPageIndex = 0;
    private EventSystem _eventSystem;
    
    private void Awake()
    {
        _eventSystem = EventSystem.current;
        
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);
        
        // 最初のページを表示
        ShowPage(0);
    }
    
    private void NextPage()
    {
        if (_currentPageIndex >= tutorialPages.Count - 1) return;
        
        if (_eventSystem && nextButton)
            _eventSystem.SetSelectedGameObject(nextButton.gameObject);
        
        ShowPage(_currentPageIndex + 1);
    }
    
    private void PreviousPage()
    {
        if (_currentPageIndex <= 0) return;
        
        if (_eventSystem && previousButton)
            _eventSystem.SetSelectedGameObject(previousButton.gameObject);
        
        ShowPage(_currentPageIndex - 1);
    }
    
    private void ShowPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= tutorialPages.Count)
        {
            Debug.LogError($"TutorialManager: ページインデックス {pageIndex} が範囲外です");
            return;
        }
        
        _currentPageIndex = pageIndex;
        
        // UI更新
        UpdateUI(tutorialPages[_currentPageIndex]);
        UpdateNavigationButtons();
    }
    
    private void UpdateUI(TutorialPageData pageData)
    {
        UpdateTitle(pageData);
        UpdateDescription(pageData);
        UpdateImage(pageData);
    }
    
    private void UpdateTitle(TutorialPageData pageData)
    {
        titleText.text = pageData.GetTitle();
    }
    
    private void UpdateDescription(TutorialPageData pageData)
    {
        descriptionText.text = pageData.GetDescription();
    }
    
    private void UpdateImage(TutorialPageData pageData)
    {
        if (pageData.TutorialImage)
        {
            tutorialImage.sprite = pageData.TutorialImage;
            tutorialImage.gameObject.SetActive(true);
        }
        else
        {
            tutorialImage.gameObject.SetActive(false);
        }
    }
    
    private void UpdateNavigationButtons()
    {
        nextButton.interactable = _currentPageIndex < tutorialPages.Count - 1;
        previousButton.interactable = _currentPageIndex > 0;
    }
}