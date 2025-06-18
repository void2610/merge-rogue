using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private GameObject statusEffectIconPrefab;
    [SerializeField] private float iconSize = 0.0125f;
    [SerializeField] private Vector2 offset = new(-0.68f, 0.23f);
    [SerializeField] private float margin = 0.4f;
    
    private readonly Dictionary<StatusEffectType, GameObject> _statusEffectIcons = new();
    
    private IContentService _contentService;
    
    [Inject]
    public void InjectDependencies(IContentService contentService)
    {
        _contentService = contentService;
    }
    
    public List<Selectable> GetStatusEffectIcons()
    {
        var icons = new List<Selectable>();
        foreach (var icon in _statusEffectIcons.Values)
        {
            if (!icon || !icon.activeSelf) continue;
            icons.Add(icon.GetComponent<Selectable>());
        }
        // 表示位置に基づいて昇順にソートする
        icons.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        return icons;
    }

    public void UpdateUI(Dictionary<StatusEffectType, int> effectStacks)
    {
        foreach (var icon in _statusEffectIcons.Values)
            if(icon) icon.SetActive(false);
        
        var i = 0;
        foreach (var kvp in effectStacks)
        {
            var type = kvp.Key;
            var stackCount = kvp.Value;
            if (!_statusEffectIcons.ContainsKey(type)) continue;
            
            var icon = _statusEffectIcons[type];
            if (!icon) continue;
            icon.SetActive(true);
            icon.transform.position = this.transform.position + new Vector3( offset.x + i * margin, offset.y, 0);
            icon.transform.Find("Stack").GetComponent<TextMeshProUGUI>().text = stackCount.ToString();
            i++;
        }
    }

    private void Start()
    {
        // VContainerの注入を待つ
        if (_contentService == null) return;
        
        var dataList = _contentService.StatusEffectList;
        InitializeStatusEffectIcons(dataList);
    }
    
    /// <summary>
    /// ステータスエフェクトアイコンを初期化する
    /// </summary>
    /// <param name="dataList">ステータスエフェクトデータリスト</param>
    private void InitializeStatusEffectIcons(StatusEffectDataList dataList)
    {
        foreach (var statusEffectData in dataList.list)
        {
            var type = statusEffectData.type;
            var icon = statusEffectData.icon;
            var go = Instantiate(statusEffectIconPrefab, transform);
            go.transform.localScale = new Vector3(iconSize, iconSize, 1);
            go.transform.Find("Icon").GetComponent<Image>().sprite = icon;
            go.transform.Find("Stack").GetComponent<TextMeshProUGUI>().text = "";
            _statusEffectIcons[type] = go;
            
            var displayName = StatusEffectManager.Instance.GetLocalizedName(type);
            go.AddSubDescriptionWindowEvent(displayName);
            go.SetActive(false);
        }
    }
}
