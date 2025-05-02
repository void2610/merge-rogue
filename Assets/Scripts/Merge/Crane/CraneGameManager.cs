using UnityEngine;

public class CraneGameManager : MonoBehaviour
{
    [SerializeField] private int maxBalls = 10;
    [SerializeField] private Vector2 ballSpawnPositionX = new Vector2(-5, 5);
    [SerializeField] private float ballSpawnPositionY;
    
    public void CreateBalls()
    {
       for (var i = 0; i < maxBalls; i++)
              CreateRandomBallinCraneArea();
    }
    
    private GameObject CreateRandomBallinCraneArea()
    {
        var ball = InventoryManager.Instance.GetRandomBall();
        var x = GameManager.Instance.RandomRange(ballSpawnPositionX.x, ballSpawnPositionX.y);
        ball.transform.position = new Vector3(x, ballSpawnPositionY, 0);
        ball.transform.SetParent(this.transform);
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        
        return ball;
    }
}
