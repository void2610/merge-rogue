using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class BackgroundController : MonoBehaviour
{
    [Header("背景")]
    [SerializeField] private SpriteRenderer bgSpriteRenderer;
    [SerializeField] private List<GameObject> torches = new();
    [SerializeField] private Vector3 defaultTorchPosition;
    [SerializeField] private float torchInterval = 5;
    
    private Material _bgMaterial;
    private static readonly int _mainTex = Shader.PropertyToID("_MainTex");
    private Tween _torchTween;

    private void Start()
    {
        InitializeMaterial();
    }

    private void InitializeMaterial()
    {
        _bgMaterial = new Material(bgSpriteRenderer.material);
        bgSpriteRenderer.material = _bgMaterial;
        _bgMaterial.SetTextureOffset(_mainTex, new Vector2(0, 0));
    }

    public void PlayStageTransition()
    {
        DOTween.To(() => _bgMaterial.GetTextureOffset(_mainTex), 
                   x => _bgMaterial.SetTextureOffset(_mainTex, x), 
                   new Vector2(1, 0), 2.0f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                _bgMaterial.SetTextureOffset(_mainTex, new Vector2(0, 0));
                MoveTorchesToNewPosition();
            })
            .SetLink(gameObject)
            .Forget();

        AnimateTorches();
    }

    private void MoveTorchesToNewPosition()
    {
        var tmp = torches[0];
        torches.RemoveAt(0);
        torches.Add(tmp);
        _torchTween.Kill();
        tmp.transform.position = defaultTorchPosition + new Vector3(torchInterval * (torches.Count - 1), 0, 0);
    }

    private void AnimateTorches()
    {
        for (var i = 0; i < torches.Count; i++)
        {
            var t = torches[i];
            var tween = t.transform.DOMove(t.transform.position - new Vector3(torchInterval, 0, 0), 2.0f)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject);
            if (i == 0) _torchTween = tween;
        }
        
        torches[^1].SetActive(Random.Range(0.0f, 1.0f) < 0.5f);
    }

    private void OnDestroy()
    {
        _torchTween?.Kill();
    }
}