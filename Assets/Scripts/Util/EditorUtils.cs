using UnityEngine;
using UnityEditor;

[ExecuteAlways] // エディターでもスクリプトが動作するようにする
public class EditorUtils : MonoBehaviour
{
    [SerializeField] private Material backgroundMaterial;
    [SerializeField] private Material arrowMaterial;
    
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

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        # if UNITY_EDITOR
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ResetMaterialParameters();
        }
        # endif
    }

    private void ResetMaterialParameters()
    {
        if (backgroundMaterial != null && arrowMaterial != null) {
            backgroundMaterial.mainTextureOffset = Vector2.zero;
            arrowMaterial.SetFloat("_Alpha", 1.0f);
        
            Debug.Log("Material parameters reset to default.");
        }
    }
}