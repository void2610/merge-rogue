using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// マウスカーソル設定コンポーネント
/// UIエレメントにアタッチしてカーソル変更を行う
/// </summary>
public class SetMouseCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [FormerlySerializedAs("cursorType")] 
    [SerializeField] private CursorIconType cursorIconType;

    private IMouseCursorService _mouseCursorService;
    private bool _isInjected = false;
    
    [Inject]
    public void InjectDependencies(IMouseCursorService mouseCursorService)
    {
        _mouseCursorService = mouseCursorService;
        _isInjected = true;
    }

    private void Start()
    {
        // 依存性注入が失敗した場合、手動で取得を試みる
        if (_isInjected) return;
        
        var lifetimeScope = FindChildLifetimeScope();
        if (!lifetimeScope) return;
        
        var container = lifetimeScope.Container;
        if (container == null) return;
        
        _mouseCursorService = container.Resolve<IMouseCursorService>();
        _isInjected = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_mouseCursorService == null) return;
        
        // ボタンが無効な場合はカーソル変更しない
        if (this.TryGetComponent(out Button button))
            if (!button.interactable) return;
        
        _mouseCursorService.SetCursor(cursorIconType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_mouseCursorService == null) return;
        _mouseCursorService.ResetCursor();
    }
    
    /// <summary>
    /// 現在のシーンの子LifetimeScope（TitleやMain）を探す
    /// RootLifetimeScopeは除外する
    /// </summary>
    /// <returns>子LifetimeScope、見つからない場合はnull</returns>
    private VContainer.Unity.LifetimeScope FindChildLifetimeScope()
    {
        var allScopes = FindObjectsByType<VContainer.Unity.LifetimeScope>(FindObjectsSortMode.None);
        
        foreach (var scope in allScopes)
        {
            // RootLifetimeScopeは除外（DontDestroyOnLoadオブジェクトかどうかで判定）
            if (scope.gameObject.scene.name == "DontDestroyOnLoad") continue;
            
            // MouseCursorServiceが登録されているかチェック
            if (scope.Container != null && scope.Container.TryResolve<IMouseCursorService>(out _))
            {
                return scope;
            }
        }
        
        return null;
    }
}