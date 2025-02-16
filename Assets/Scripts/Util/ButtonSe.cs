using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSe : MonoBehaviour
{
    [SerializeField] private AudioClip hoverSe;
    [SerializeField] private float hoverVolume = 1.0f;
    [SerializeField] private AudioClip clickSe;
    [SerializeField] private float clickVolume = 1.0f;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSe == null) return;
        var pitch = Random.Range(0.9f, 1.1f); 
        SeManager.Instance.PlaySe(clickSe, clickVolume, pitch);
    }
}
