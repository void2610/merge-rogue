using System;
using UnityEngine;
using System.Collections.Generic;
using R3;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;
    
    [SerializeField] private GameObject relicPrefab;
    [SerializeField] private Transform relicContainer;
    [SerializeField] private Vector3 relicGridPosition;
    [SerializeField] private Vector2Int relicGridSize;
    [SerializeField] private Vector2 relicOffset;
    [SerializeField] private float relicUISize;
    
    [SerializeField] private List<RelicData> testRelics;
    
    private const int MAX_RELICS = 27;
    private readonly List<RelicData> _relics = new();
    private readonly List<RelicBase> _behaviors = new();
    private readonly List<RelicUI> _relicUIs = new();
    
    public List<RelicData> GetCurrentRelics() => _relics;
    
    public void AddRelic(RelicData relic)
    {
        if(_relics.Count >= MAX_RELICS)
        {
            NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.CantHoldAnyMoreRelics);
            return;
        }
        
        _relics.Add(relic);
        var rui = CreateRelicUI(relic);
        ApplyEffect(relic, rui);
        
        UpdateRelicUINavigation();
    }
    
    public void RemoveRelic(RelicData relic)
    {
        var index = _relics.FindIndex(r => r.id == relic.id);
        if(index == -1)
        {
            Debug.LogError("指定されたレリックが存在しません: " + relic.id);
            return;
        }

        var behavior = _behaviors[index];
        behavior.RemoveEffect();
        _behaviors.Remove(behavior);
        _relics.Remove(relic);
        Destroy(_relicUIs[index].gameObject);
        _relicUIs.RemoveAt(index);

        UpdateRelicUINavigation();
    }
    
    public bool HasRelic(Type t)
    {
        if(!typeof(RelicBase).IsAssignableFrom(t))
            throw new ArgumentException("指定されたクラスはRelicBaseを継承していません: " + t);
        return _behaviors.Exists(b => b.GetType() == t);
    }
    
    private RelicUI CreateRelicUI(RelicData r)
    {
        var go = Instantiate(relicPrefab, relicContainer);
        go.transform.localPosition = relicGridPosition +
            new Vector3(relicOffset.x * ((_relics.Count - 1) / relicGridSize.y), -relicOffset.y * ((_relics.Count - 1) % relicGridSize.y));
        go.transform.localScale = new Vector3(relicUISize, relicUISize, 1);
        var relicUI = go.GetComponent<RelicUI>();
        relicUI.SetRelicData(r);
        _relicUIs.Add(relicUI);
        return relicUI;
    }
    
    private void ApplyEffect(RelicData r, RelicUI rui)
    {
        var type = System.Type.GetType(r.className);
        var behaviour = gameObject.AddComponent(type) as RelicBase;
        if(!behaviour)
        {
            Debug.LogError("指定されたクラスは存在しません: " + r.className);
            return;
        }
        
        behaviour.Init(rui);
        _behaviors.Add(behaviour);
    }
    
    // RelicUI のナビゲーション設定を更新するメソッド
    private void UpdateRelicUINavigation()
    {
        for (var i = 0; i < _relicUIs.Count; i++)
        {
            // RelicUI に Button などの Selectable がアタッチされている前提
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
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        if (!Application.isEditor) return;
        foreach (var r in testRelics)
        {
            AddRelic(r);
        }
    }
}
