using UnityEngine;

public class SortOrder : MonoBehaviour
{
    [SerializeField] private int sortingOrder = 100;
    [SerializeField] private string layer;
    private Renderer _vfxRenderer;

    private void OnValidate()
    {
        _vfxRenderer = GetComponent<Renderer>();
        if (_vfxRenderer)
        {
            _vfxRenderer.sortingOrder = sortingOrder;
            _vfxRenderer.sortingLayerName = layer;
        }
    }
}