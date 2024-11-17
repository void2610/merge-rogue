using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.VFX;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private Vector3 targetPosition = Vector3.zero; // 移動先の座標
    [SerializeField] private float duration = 2f; // 移動時間
    [SerializeField] private int intermediatePointCount = 3; // 中間点の数
    [SerializeField] private float maxControlPointOffset = 3f; // 制御点オフセットの最大値
    [SerializeField] private bool destroyOnComplete = true; // 移動完了時にオブジェクトを破棄するか
    
    private VisualEffect vfx;

    /// <summary>
    /// 指定された座標までカーブを描いて移動
    /// </summary>
    /// <param name="pos">移動先の座標</param>
    /// <param name="dur">移動時間</param>
    /// <param name="doc">移動完了時にオブジェクトを破棄するか</param>
    public void MoveTo(Vector3? pos = null, float dur = -1.0f, bool doc = true)
    {
        if (pos != null) targetPosition = (Vector3)pos;
        if (dur > 0.0f) duration = dur;
        destroyOnComplete = doc;

        // 現在位置とターゲット位置を取得
        var startPosition = transform.position;

        // 中間点を自動生成
        var pathPoints = GenerateIntermediatePoints(startPosition, targetPosition);

        // DOTweenで移動
        transform.DOPath(
                pathPoints,
                duration,
                PathType.CatmullRom, // Catmull-Romスプラインを使用
                PathMode.Full3D      // 3D空間での移動
            )
            .SetEase(Ease.InOutSine) // 移動のスムーズさ
            .OnComplete(() =>
            {
                if(vfx != null) vfx.Stop();
                if (destroyOnComplete) StartCoroutine(waitAndDestroy(1f));
            });
    }

    /// <summary>
    /// 開始点と終了点の間に中間点を生成
    /// </summary>
    /// <param name="start">開始点</param>
    /// <param name="end">終了点</param>
    /// <returns>中間点を含む座標配列</returns>
    private Vector3[] GenerateIntermediatePoints(Vector3 start, Vector3 end)
    {
        var points = new Vector3[intermediatePointCount + 2]; // 開始点と終了点を含む
        Vector3 randomOffset = Random.insideUnitSphere * maxControlPointOffset;

        points[0] = start;
        points[^1] = end;

        // 中間点を計算
        for (int i = 1; i <= intermediatePointCount; i++)
        {
            float t = (float)i / (intermediatePointCount + 1); // 進行割合
            var midpoint = Vector3.Lerp(start, end, t); // 線形補間で中間位置を計算

            // ランダムな方向と大きさのオフセットを生成
            midpoint += randomOffset;

            points[i] = midpoint;
        }

        return points;
    }
    
    private IEnumerator waitAndDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }

    private void Start()
    {
        vfx = GetComponent<VisualEffect>();
        MoveTo();
    }
}
