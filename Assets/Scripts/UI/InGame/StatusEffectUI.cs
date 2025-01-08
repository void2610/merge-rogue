using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    [Serializable]
    private class StatusEffectIcon
    {
        public StatusEffectType type;
        public Sprite sprite;
    }
    
    [SerializeField] private List<StatusEffectIcon> statusEffectSprites;
    [SerializeField] private GameObject statusEffectIconPrefab;
    
    private readonly Dictionary<StatusEffectType, GameObject> _statusEffectIcons = new();
    private readonly Vector2 _offset = new(-0.68f, 0.23f);
    private const float MARGIN = 0.4f;
    private const float ICON_SIZE = 0.0125f;

    public void UpdateUI(List<StatusEffectBase> effects)
    {
        foreach (var icon in _statusEffectIcons.Values)
        {
            // icon.SetActive(false);
        }
        
        for (var i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            var icon = _statusEffectIcons[effect.Type];
            icon.SetActive(true);
            icon.transform.position = this.transform.position + new Vector3( _offset.x + i * MARGIN, _offset.y, 0);
            icon.transform.Find("Stack").GetComponent<TextMeshProUGUI>().text = effect.StackCount.ToString();
        }
    }

    private void Awake()
    {
        foreach (StatusEffectType type in Enum.GetValues(typeof(StatusEffectType)))
        {
            var icon = statusEffectSprites.Find(s => s.type == type).sprite;
            if (icon == null) continue;
            var go = Instantiate(statusEffectIconPrefab, transform);
            go.transform.localScale = new Vector3(ICON_SIZE, ICON_SIZE, 1);
            go.transform.Find("Icon").GetComponent<Image>().sprite = icon;
            go.transform.Find("Stack").GetComponent<TextMeshProUGUI>().text = "";
            _statusEffectIcons[type] = go;
        }
    }
}
