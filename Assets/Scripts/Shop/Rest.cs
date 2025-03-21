using UnityEngine;
using UnityEngine.UI;

public class Rest : MonoBehaviour
{
    [SerializeField] private Button restButton;
    [SerializeField] private Button organizeButton;
    [SerializeField] private Button skipButton;
    
    private static void OnClickRest()
    {
        var restAmount = GameManager.Instance.Player.MaxHealth.Value  * 0.2f;
        EventManager.OnRest.Trigger((int)restAmount);
        var v = EventManager.OnRest.GetAndResetValue();
        if(v > 0) GameManager.Instance.Player.Heal(v);
        
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        UIManager.Instance.EnableCanvasGroup("Rest", false);
    }

    private static void OnClickOrganise()
    {
        EventManager.OnOrganise.Trigger(0);
        UIManager.Instance.EnableCanvasGroup("Rest", false);
        InventoryManager.Instance.InventoryUI.StartEdit(InventoryUI.InventoryUIState.Swap);
    }
    
    private static void OnClickSkip()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        UIManager.Instance.EnableCanvasGroup("Rest", false);
    }

    private void Awake()
    {
        restButton.onClick.AddListener(OnClickRest);
        organizeButton.onClick.AddListener(OnClickOrganise);
        skipButton.onClick.AddListener(OnClickSkip);
    }
}