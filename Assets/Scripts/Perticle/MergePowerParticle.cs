using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.VFX;

public class MergePowerParticle : MonoBehaviour
{
    [SerializeField] private Vector3 targetPosition = Vector3.zero; // 移動先の座標
    [SerializeField] private float duration = 2f; // 移動時間
    [SerializeField] private int intermediatePointCount = 3; // 中間点の数
    [SerializeField] private float maxControlPointOffset = 3f; // 制御点オフセットの最大値
    
    private VisualEffect vfx;
    private Vector3 previousPosition;
    
    public void MoveTo(Color color)
    {
        vfx = GetComponent<VisualEffect>();
        vfx.SetVector3("Color", new Vector3(color.r, color.g, color.b));
        
        duration = Random.Range(Mathf.Max(0.01f, duration - 1f), duration + 1f);
        
        // 現在位置とターゲット位置を取得
        var startPosition = transform.position;

        // 中間点を自動生成
        var pathPoints = GenerateIntermediatePoints(startPosition, targetPosition);

        // DOTweenで移動
        transform.DOPath(
                pathPoints,
                duration,
                PathType.CatmullRom, // Catmull-Romスプラインを使用
                PathMode.Sidescroller2D
            )
            .SetEase(Ease.InOutSine) // 移動のスムーズさ
            .OnComplete(() =>
            {
                StartCoroutine(WaitAndDestroy(3f));
            });
        
        // 少し早めにVFXを停止
        DOVirtual.DelayedCall(duration - 0.3f, () =>
        {
            if (vfx != null) vfx.Stop();
        });
    }

    private Vector3[] GenerateIntermediatePoints(Vector3 start, Vector3 end)
    {
        var points = new Vector3[intermediatePointCount + 2]; // 開始点と終了点を含む
        var randomOffset = Random.insideUnitSphere * maxControlPointOffset;

        points[0] = start;
        points[^1] = end;

        // 中間点を計算
        for (var i = 1; i <= intermediatePointCount; i++)
        {
            float t = (float)i / (intermediatePointCount + 1); // 進行割合
            var midpoint = Vector3.Lerp(start, end, t); // 線形補間で中間位置を計算

            // ランダムな方向と大きさのオフセットを生成
            midpoint += randomOffset;

            points[i] = midpoint;
            points[i].z = 0;
        }

        return points;
    }
    
    private IEnumerator WaitAndDestroy(float time)
    {
        yield return new WaitForSeconds(time / GameManager.Instance.timeScale);
        Destroy(this.gameObject);
    }
}
