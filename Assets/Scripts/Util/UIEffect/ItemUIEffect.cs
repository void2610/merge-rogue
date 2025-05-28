using UnityEngine;
using Coffee.UIEffects;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemUIEffect : MonoBehaviour
{
    private UIEffect _uiEffect;

    public void SetColor(Rarity rarity)
    {
        if (_uiEffect)
        {
            _uiEffect.transitionColor = rarity.GetColor();
        }
    }

    private void Awake()
    {
        _uiEffect = this.gameObject.AddComponent<UIEffect>();
        _uiEffect.LoadPreset("ItemShiny");
    }
}
