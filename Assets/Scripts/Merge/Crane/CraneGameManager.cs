using UnityEngine;
using Cysharp.Threading.Tasks;

public class CraneGameManager : SingletonMonoBehaviour<CraneGameManager>
{
    [SerializeField] private Transform craneRoot;
    [SerializeField] private Crane crane;
    [SerializeField] private Collider2D ballGetTrigger;
    [SerializeField] private int maxBalls = 10;
    [SerializeField] private Vector2 ballSpawnPositionX = new Vector2(-5, 5);
    [SerializeField] private float ballSpawnPositionY;
    
    private const float MOVE_SPEED = 5f;
    
    private bool _isCraneMoving;
    private Vector3 _currentRootPosition = new Vector3(0, 1.6f, 0);
    
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
                MergeManager.Instance.AddBallFromCrane(ball);
            }
        }
        
        GameManager.Instance.ChangeState(GameManager.GameState.Merge);
        
        _isCraneMoving = false;
    }

    private void Update()
    { 
        if(UIManager.Instance.IsAnyCanvasGroupEnabled()) return;
        if(GameManager.Instance.state != GameManager.GameState.Crane) return;
        if (_isCraneMoving) return;
        
        if (InputProvider.Instance.Gameplay.LeftClick.IsPressed()) StartCraneMoving().Forget();
        
        if (InputProvider.Instance.Gameplay.LeftMove.IsPressed() && _currentRootPosition.x > -2f)
            _currentRootPosition += Vector3.left * (MOVE_SPEED * Time.deltaTime);

        if (InputProvider.Instance.Gameplay.RightMove.IsPressed() && _currentRootPosition.x < 2f)
            _currentRootPosition += Vector3.right * (MOVE_SPEED * Time.deltaTime);
        
        craneRoot.localPosition = _currentRootPosition;
        crane.transform.localPosition = _currentRootPosition + new Vector3(0, -0.3f, 0);
    }
}
