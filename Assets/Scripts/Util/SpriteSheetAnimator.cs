using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

/// <summary>
/// SpriteSheet を指定し、分割済みスプライトを Inspector ボタンで一括取得してアニメーション再生するクラス
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimator : MonoBehaviour
{
    [Header("スプライトシート（Multiple 設定済みの Texture2D）")]
    [SerializeField] private Texture2D spriteSheet;

    [Header("フレーム用スプライト（Editor の Refresh Frames で自動設定される）")]
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();

    [Header("1秒あたりのフレーム数")]
    [SerializeField] private float framesPerSecond = 10f;

    private float _timer;
    private int _currentFrame;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (sprites == null || sprites.Count == 0)
            Debug.LogError("[SpriteSheetAnimator] sprites が設定されていません。\nInspector の Refresh Frames を実行してください。");
    }

    private void Update()
    {
        if (sprites == null || sprites.Count == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / framesPerSecond)
        {
            _timer -= 1f / framesPerSecond;
            _currentFrame = (_currentFrame + 1) % sprites.Count;
            _spriteRenderer.sprite = sprites[_currentFrame];
        }
    }

    /// <summary>
    /// Inspector 上のボタンから呼び出すため public に変更
    /// </summary>
    [ContextMenu("Refresh Frames")]
    public void RefreshFrames()
    {
#if UNITY_EDITOR
        if (!spriteSheet)
        {
            Debug.LogError("[SpriteSheetAnimator] spriteSheet が設定されていません。");
            return;
        }
        var path = AssetDatabase.GetAssetPath(spriteSheet);
        var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                                  .OfType<Sprite>()
                                  .OrderBy(s => s.name)
                                  .ToList();
        sprites = assets;
        EditorUtility.SetDirty(this);
        Debug.Log($"[SpriteSheetAnimator] {sprites.Count} 枚のフレームを読み込みました。 (SpriteSheet: {spriteSheet.name})");
#endif
    }
}


