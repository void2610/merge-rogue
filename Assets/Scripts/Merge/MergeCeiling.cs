using UnityEngine;

public class MergeCeiling : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var ball = other.GetComponent<BallBase>();
        if (ball == null || ball.isFrozen) return;
        
        
        
        GameManager.Instance.player.TakeDamage(other.GetComponent<BallBase>().level);
        Destroy(other.gameObject);
    }
}
