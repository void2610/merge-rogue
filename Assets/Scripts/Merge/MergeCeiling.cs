using UnityEngine;

public class MergeCeiling : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BallBase>() != null)
        {
            GameManager.instance.player.TakeDamage(other.GetComponent<BallBase>().level);
            Destroy(other.gameObject);
        }
    }
}
