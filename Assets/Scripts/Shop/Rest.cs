using UnityEngine;
using UnityEngine.UI;
using SafeEventSystem;

public class Rest : MonoBehaviour
{
    [SerializeField] private Button restButton;
    [SerializeField] private Button organizeButton;
    [SerializeField] private Button skipButton;
    
    private static void OnClickRest()
    {
        var restAmount = GameManager.Instance.Player.MaxHealth.Value  * 0.2f;
        var finalAmount = SafeEventManager.OnRest.Process((int)restAmount);
        if(finalAmount > 0) GameManager.Instance.Player.Heal(finalAmount);
        
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        UIManager.Instance.EnableCanvasGroup("Rest", false);
    }
    
    private static void OnClickOrganise()
    {
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