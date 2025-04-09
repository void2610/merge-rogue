using UnityEngine;
using System.Collections.Generic;

public class FillingRateTrigger : MonoBehaviour
{
    // トリガーとして使うCollider2D（このGameObjectにアタッチされている前提）
    private Collider2D _triggerCollider;

    private void Awake()
    {
        _triggerCollider = this.GetComponent<Collider2D>();
    }
    
    /// <summary>
    /// 現在トリガーに侵入している BallBase を持つオブジェクトがあるかチェックする
    /// </summary>
    public bool IsCollideWithBall()
    {
        // 重なっているCollider2Dを格納するリスト
        var overlappingColliders = new List<Collider2D>();

        // フィルタ。ここでフィルター条件を設定することも可能（例: 特定レイヤーのみなど）。
        var filter = new ContactFilter2D();
        filter.useTriggers = true; // トリガー同士も対象にする場合はtrue

        // triggerColliderのOverlapColliderを用いて、重なっている全てのCollider2Dを取得
        var count = _triggerCollider.Overlap(filter, overlappingColliders);

        // 取得したコライダーの中から、BallBaseを持つものがあるかチェック
        foreach (var col in overlappingColliders)
        {
            // オブジェクトが破棄されている場合はcolがnullになっている可能性があるので注意
            if(col && col.TryGetComponent<BallBase>(out _)) return true;
        }
        return false;
    }
}
