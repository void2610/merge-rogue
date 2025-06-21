using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class Rest : MonoBehaviour
{
    [SerializeField] private Button restButton;
    [SerializeField] private Button organizeButton;
    [SerializeField] private Button skipButton;
    
    private static IInventoryService _inventoryService;
    
    [Inject]
    public void InjectDependencies(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }
    
    private static void OnClickRest()
    {
        var restAmount = GameManager.Instance.Player.MaxHealth.Value  * 0.2f;
        var finalAmount = EventManager.OnRest.Process((int)restAmount);
        if(finalAmount > 0) GameManager.Instance.Player.Heal(finalAmount);
        
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        UIManager.Instance.EnableCanvasGroup("Rest", false);
    }
    
    private static void OnClickOrganise()
    {
        _inventoryService.StartEditSwap();
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