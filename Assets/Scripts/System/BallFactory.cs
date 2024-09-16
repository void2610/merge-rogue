using UnityEngine;

public class BallFactory : MonoBehaviour
{
    public static BallFactory instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    [SerializeField]
    private GameObject ballBasePrefab;

    public GameObject CreateNormalBall()
    {
        GameObject ball = Instantiate(ballBasePrefab);
        ball.AddComponent<NormalBall>().Freeze();

        return ball;
    }
}
