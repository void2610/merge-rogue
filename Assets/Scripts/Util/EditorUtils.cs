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
        // 再生モード変更時のイベントに登録
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        // 再生モード変更時のイベントから解除
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // 再生終了時にマテリアルのパラメータをリセット
            ResetMaterialParameters();
        }
    }

    private void ResetMaterialParameters()
    {
        if (backgroundMaterial != null && arrowMaterial != null) {
            backgroundMaterial.SetTextureOffset("_MainTex", Vector2.zero);
            arrowMaterial.SetFloat("_Alpha", 1.0f);
        
            Debug.Log("Material parameters reset to default.");
        }
    }
}