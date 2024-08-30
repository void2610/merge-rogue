using UnityEngine;

public class MergeCeiling : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name);
        if (other.GetComponent<Ball>() != null)
        {
            GameManager.instance.player.TakeDamage(other.GetComponent<Ball>().level);
            Destroy(other.gameObject);
        }
    }
}
