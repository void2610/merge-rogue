using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSe : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] private AudioClip hoverSe;
    [SerializeField] private float hoverVolume = 1.0f;
    [SerializeField] private AudioClip clickSe;
    [SerializeField] private float clickVolume = 1.0f;
    
    private Button _button;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if(_button && !_button.interactable) return;
        if (clickSe == null) return;
        var pitch = Random.Range(0.9f, 1.1f); 
        SeManager.Instance.PlaySe(clickSe, clickVolume, pitch);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(_button && !_button.interactable) return;
        if (hoverSe == null) return;
        var pitch = Random.Range(0.9f, 1.1f); 
        SeManager.Instance.PlaySe(hoverSe, hoverVolume, pitch);
    }
    
    private void Awake()
    {
        _button = this.GetComponent<Button>();
    }
}
