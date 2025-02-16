using UnityEngine;
using UnityEngine.EventSystems;

public class PlaySeOnAwake : MonoBehaviour
{
    [SerializeField] private AudioClip se;
    [SerializeField] private float volume = 1.0f;
    
    private void Start()
    {
        var pitch = Random.Range(0.9f, 1.1f); 
        SeManager.Instance.PlaySe(se, volume, pitch);
    }
}
