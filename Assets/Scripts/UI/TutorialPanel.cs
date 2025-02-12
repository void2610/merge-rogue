using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] private List<GameObject> pages;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    
    private int _currentPage = 0;
    private GameObject _currentActivePage;
    private RectTransform _rectTransform;

    private void NextPage()
    {
        if (_currentPage >= pages.Count - 1) return;
        ChangePage(_currentPage + 1);
    }
    
    private void PreviousPage()
    {
        if (_currentPage <= 0) return;
        ChangePage(_currentPage - 1);
    }
    
    private void ChangePage(int page)
    {
        if (_currentActivePage)
        {
            DescriptionWindow.Instance.RemoveTextFromObservation(_currentActivePage);
            Destroy(_currentActivePage);
        }
        _currentActivePage = Instantiate(pages[page], this.transform);
        _currentPage = page;
        
        // ハイライトからウィンドウ表示できるように
        DescriptionWindow.Instance.AddTextToObservation(_currentActivePage);
        
        previousButton.interactable = _currentPage > 0;
        nextButton.interactable = _currentPage < pages.Count - 1;
    }

    private void Start(){
        ChangePage(0);
        
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            NextPage();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PreviousPage();
        }
    }
}