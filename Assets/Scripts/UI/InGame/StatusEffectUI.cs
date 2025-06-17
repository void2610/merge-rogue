using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private GameObject statusEffectIconPrefab;
    [SerializeField] private float iconSize = 0.0125f;
    [SerializeField] private Vector2 offset = new(-0.68f, 0.23f);
    [SerializeField] private float margin = 0.4f;
    
    private readonly Dictionary<StatusEffectType, GameObject> _statusEffectIcons = new();
    
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

    public void UpdateUI(List<StatusEffectBase> effects)
    {
        foreach (var icon in _statusEffectIcons.Values)
            if(icon) icon.SetActive(false);
        
        for (var i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            var icon = _statusEffectIcons[effect.Type];
            if (!icon) continue;
            icon.SetActive(true);
            icon.transform.position = this.transform.position + new Vector3( offset.x + i * margin, offset.y, 0);
            icon.transform.Find("Stack").GetComponent<TextMeshProUGUI>().text = effect.StackCount.ToString();
        }
    }

    private void Start()
    {
        var statusEffectDataList = ContentProvider.Instance.StatusEffectList;
        
        foreach (var statusEffectData in statusEffectDataList.list)
        {
            var type = statusEffectData.type;
            var icon = statusEffectData.icon;
            var go = Instantiate(statusEffectIconPrefab, transform);
            go.transform.localScale = new Vector3(iconSize, iconSize, 1);
            go.transform.Find("Icon").GetComponent<Image>().sprite = icon;
            go.transform.Find("Stack").GetComponent<TextMeshProUGUI>().text = "";
            _statusEffectIcons[type] = go;
            
            var displayName = GetStatusEffectDisplayName(statusEffectData);
            go.AddSubDescriptionWindowEvent(displayName);
            go.SetActive(false);
        }
    }
    
    private string GetStatusEffectDisplayName(StatusEffectData data)
    {
        // TODO: ローカライゼーションシステムと連携時はここを修正
        // if (!string.IsNullOrEmpty(data.localizationKeyName))
        //     return LocalizationManager.GetLocalizedValue(data.localizationKeyName);
        
        // 暫定的に日本語名を返す
        return data.type switch
        {
            StatusEffectType.Burn => "火傷",
            StatusEffectType.Regeneration => "再生",
            StatusEffectType.Shield => "シールド",
            StatusEffectType.Freeze => "凍結",
            StatusEffectType.Invincible => "無敵",
            StatusEffectType.Shock => "感電",
            StatusEffectType.Power => "パワー",
            StatusEffectType.Rage => "怒り",
            StatusEffectType.Curse => "呪い",
            StatusEffectType.Confusion => "混乱",
            _ => data.type.ToString()
        };
    }
}
