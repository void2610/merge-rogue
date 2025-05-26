using UnityEngine;
using Coffee.UIEffects;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageShinyEffect : MonoBehaviour
{
    private UIEffect _uiEffect;
    private UIEffectTweener _tweener;

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
        
        _tweener = this.gameObject.AddComponent<UIEffectTweener>();
    }
}
