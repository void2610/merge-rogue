using UnityEngine;
using UnityEditor;

[ExecuteAlways] // エディターでもスクリプトが動作するようにする
public class EditorUtils : MonoBehaviour
{
    [SerializeField] private Material backgroundMaterial;
    [SerializeField] private Material fillingRateGaugeMaterial;
    
    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
    }

# if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ResetMaterialParameters();
        }
    }
# endif

    private void ResetMaterialParameters()
    {
        backgroundMaterial.mainTextureOffset = Vector2.zero;
        fillingRateGaugeMaterial.SetColor("_EmissionColor", Color.red);
        Debug.Log("Material parameters reset to default.");
    }
}