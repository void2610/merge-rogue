using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// レリックUI表示を管理するクラス
/// RelicServiceのイベントを購読してUI更新を行う
/// MonoBehaviourとしてUnityエディタで設定可能
/// </summary>
public class RelicUIManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject relicPrefab;
    [SerializeField] private Transform relicContainer;
    [SerializeField] private Vector3 relicGridPosition;
    [SerializeField] private Vector2Int relicGridSize;
    [SerializeField] private Vector2 relicOffset;
    [SerializeField] private float relicUISize;
    
    private readonly List<RelicUI> _relicUIs = new();
    private IRelicService _relicService;
    
    /// <summary>
    /// 依存性注入
    /// </summary>
    [Inject]
    public void InjectDependencies(IRelicService relicService)
    {
        _relicService = relicService;
    }
    
    private void Start()
    {
        // RelicServiceのイベントを購読
        if (_relicService != null)
        {
            _relicService.OnRelicAdded += OnRelicAdded;
            _relicService.OnRelicRemoved += OnRelicRemoved;
        }
    }
    
    private void OnDestroy()
    {
        // イベント購読を解除
        if (_relicService != null)
        {
            _relicService.OnRelicAdded -= OnRelicAdded;
            _relicService.OnRelicRemoved -= OnRelicRemoved;
        }
    }
    
    /// <summary>
    /// レリックが追加された際の処理
    /// </summary>
    /// <param name="relicData">追加されたレリックデータ</param>
    /// <param name="relicBehavior">作成されたレリック効果</param>
    private void OnRelicAdded(RelicData relicData, RelicBase relicBehavior)
    {
        var relicUI = CreateRelicUI(relicData);
        
        // RelicBaseにUIを設定
        if (relicBehavior != null)
        {
            // SetUIメソッドでUIを設定
            relicBehavior.SetUI(relicUI);
        }
        
        UpdateRelicUINavigation();
    }
    
    /// <summary>
    /// レリックが削除された際の処理
    /// </summary>
    /// <param name="relicData">削除されたレリックデータ</param>
    private void OnRelicRemoved(RelicData relicData)
    {
        var index = _relicService.Relics.ToList().FindIndex(r => r.className == relicData.className);
        if (index >= 0 && index < _relicUIs.Count)
        {
            Destroy(_relicUIs[index].gameObject);
            _relicUIs.RemoveAt(index);
            UpdateRelicUINavigation();
        }
    }
    
    /// <summary>
    /// RelicUIを作成する
    /// </summary>
    /// <param name="relicData">レリックデータ</param>
    /// <returns>作成されたRelicUI</returns>
    private RelicUI CreateRelicUI(RelicData relicData)
    {
        var go = Instantiate(relicPrefab, relicContainer);
        go.transform.localPosition = relicGridPosition +
            new Vector3(relicOffset.x * ((_relicUIs.Count) / relicGridSize.y), -relicOffset.y * ((_relicUIs.Count) % relicGridSize.y));
        go.transform.localScale = new Vector3(relicUISize, relicUISize, 1);
        
        var relicUI = go.GetComponent<RelicUI>();
        relicUI.SetRelicData(relicData);
        _relicUIs.Add(relicUI);
        
        return relicUI;
    }
    
    /// <summary>
    /// RelicUIのナビゲーション設定を更新する
    /// </summary>
    private void UpdateRelicUINavigation()
    {
        for (var i = 0; i < _relicUIs.Count; i++)
        {
            var selectable = _relicUIs[i].GetComponent<Selectable>();
            if (!selectable)
            {
                Debug.LogWarning("RelicUIにSelectableコンポーネントがありません: " + _relicUIs[i].name);
                continue;
            }
            
            var nav = selectable.navigation;
            nav.mode = Navigation.Mode.Explicit;
            
            // 上方向：前のRelicUI（存在する場合）
            if (i > 0)
                nav.selectOnUp = _relicUIs[i - 1].GetComponent<Selectable>();
            else
                nav.selectOnUp = null;
            
            // 下方向：次のRelicUI（存在する場合）
            if (i < _relicUIs.Count - 1)
                nav.selectOnDown = _relicUIs[i + 1].GetComponent<Selectable>();
            else
                nav.selectOnDown = null;
            
            // 左右は必要に応じて設定（ここでは未設定）
            nav.selectOnLeft = null;
            nav.selectOnRight = null;
            
            selectable.navigation = nav;
        }
    }
}