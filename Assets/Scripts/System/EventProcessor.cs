using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventProcessor : MonoBehaviour
{
    public static EventProcessor Instance;
    
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private List<GameObject> options;
    
    private StageEventBase _currentEvent;
    
    public void SetRandomEvent()
    {
        _currentEvent = ContentProvider.Instance.GetRandomEvent();
        _currentEvent.Init();
        descriptionText.text = _currentEvent.MainDescription;
        options.ForEach(option => option.SetActive(false));
        
        for (var i = 0; i < _currentEvent.Options.Count; i++)
        {
            options[i].SetActive(true);
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = _currentEvent.Options[i].description;
            SetOptionBehaviour(options[i].GetComponent<Button>(), _currentEvent.Options[i]);
            options[i].GetComponent<Button>().interactable = _currentEvent.Options[i].IsAvailable();
        }
        EventManager.OnEventEnter.Trigger(_currentEvent);
    }

    private static void SetOptionBehaviour(Button button, StageEventBase.OptionData option)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            option.Action();
            if(option.isEndless) return;
            
            UIManager.Instance.EnableCanvasGroup("Event", false);
            GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        });
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
