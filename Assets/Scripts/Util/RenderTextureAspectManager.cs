using UnityEngine;
using UnityEngine.UI;

public class RenderTextureAspectManager : MonoBehaviour
{
    [SerializeField] private Vector2 aspectRatio = new Vector2(16, 9);
    [SerializeField] private float defaultScale　= 1.065f;

    
    private RawImage renderTexture;
    private RectTransform renderTextureRectTransform;

    private void Start()
    {
        renderTexture = GetComponent<RawImage>();
        renderTextureRectTransform = renderTexture.GetComponent<RectTransform>();
        renderTextureRectTransform.offsetMin = Vector2.zero;
        renderTextureRectTransform.offsetMax = Vector2.zero;
        
        AdjustRenderTextureSize();
    }

    private void Update()
    {
        AdjustRenderTextureSize();
    }

    private void AdjustRenderTextureSize()
    {
        var screenAspect = (float)Screen.width / (float)Screen.height;
        var targetAspect = aspectRatio.x / aspectRatio.y;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (screenAspect > targetAspect)
        {
            // スクリーンが目標アスペクトより横長の場合
            var width = targetAspect / screenAspect;
            renderTextureRectTransform.anchorMin = new Vector2(((1 - width) / 2), 0);
            renderTextureRectTransform.anchorMax = new Vector2((1 + width) / 2, 1);
            var scaleFactor = screenWidth / (screenHeight * targetAspect);
            
            renderTextureRectTransform.localScale = new Vector3(scaleFactor * defaultScale, defaultScale, 1.055f);
        }
        else
        {
            // スクリーンが目標アスペクトより縦長の場合
            var height = screenAspect / targetAspect;
            renderTextureRectTransform.anchorMin = new Vector2(0, (1 - height) / 2);
            renderTextureRectTransform.anchorMax = new Vector2(1, (1 + height) / 2);
            
            var scaleFactor = screenHeight / (screenWidth / targetAspect);
            renderTextureRectTransform.localScale = new Vector3(defaultScale, scaleFactor * defaultScale, 1.055f);
        }
    }
}