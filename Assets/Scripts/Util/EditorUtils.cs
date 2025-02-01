using UnityEngine;
using UnityEditor;

[ExecuteAlways] // エディターでもスクリプトが動作するようにする
public class EditorUtils : MonoBehaviour
{
    [SerializeField] private Material backgroundMaterial;
    
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
        if (backgroundMaterial != null) {
            backgroundMaterial.mainTextureOffset = Vector2.zero;
            Debug.Log("Material parameters reset to default.");
        }
    }
}