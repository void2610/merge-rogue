using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpriteSheet を指定し、分割済みスプライトを Inspector ボタンで一括取得してアニメーション再生するクラス
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimator : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites = new ();
    [SerializeField] private float framesPerSecond = 10f;

    private float _timer;
    private int _currentFrame;
    private SpriteRenderer _spriteRenderer;

    public void Setup(List<Sprite> s, int fps)
    {
        this.sprites = s;
        this.framesPerSecond = fps;
    }

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
}


