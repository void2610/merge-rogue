using UnityEngine;
using Cysharp.Threading.Tasks;

public class CraneGameManager : SingletonMonoBehaviour<CraneGameManager>
{
    [SerializeField] private Crane crane;
    [SerializeField] private Collider2D ballGetTrigger;
    [SerializeField] private int maxBalls = 10;
    [SerializeField] private Vector2 ballSpawnPositionX = new Vector2(-5, 5);
    [SerializeField] private float ballSpawnPositionY;
    
    private bool _isCraneMoving;
    
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

    private async UniTaskVoid StartCraneMoving()
    {
         _isCraneMoving = true;
         await crane.StartArmMove();
         
         // BoxCollider2Dに侵入したボールを取得
        var colliders = Physics2D.OverlapBoxAll(ballGetTrigger.bounds.center, ballGetTrigger.bounds.size, 0);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<BallBase>(out var ball))
            {
                // ボールをマージエリアに移動
                Debug.Log(ball.name);
            }
        }
        
        GameManager.Instance.ChangeState(GameManager.GameState.Merge);
        
        _isCraneMoving = false;
    }

    private void Update()
    {
       if(GameManager.Instance.state != GameManager.GameState.Crane) return;
         if (_isCraneMoving) return;

         if (InputProvider.Instance.Gameplay.LeftClick.IsPressed()) StartCraneMoving().Forget();
    }
}
