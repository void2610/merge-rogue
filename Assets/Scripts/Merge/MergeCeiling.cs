using UnityEngine;

public class MergeCeiling : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var ball = other.GetComponent<BallBase>();
        if (ball == null || ball.IsFrozen) return;
        
        GameManager.Instance.Player.Damage(AttackType.Normal, other.GetComponent<BallBase>().Rank);
        Destroy(other.gameObject);
    }
}
