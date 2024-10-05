using UnityEngine;

public class MergeCeiling : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BallBase>() == null) return;
        
        GameManager.Instance.player.TakeDamage(other.GetComponent<BallBase>().level);
        Destroy(other.gameObject);
    }
}
