using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MergeManager : MonoBehaviour
{
    [System.Serializable]
    public struct BallData
    {
        public GameObject prefab;
        public float probability;
    }

    public static MergeManager instance;

    [SerializeField]
    private GameObject fallAnchor;
    [SerializeField]
    private List<BallData> balls;

    public float moveSpeed = 1.0f;
    public float coolTime = 1.0f;
    private float leftLimit = -3.2f;
    private float rightLimit = 3.2f;

    private GameObject currentBall;
    private GameObject nextBall;
    private float probabilitySum;
    private GameObject ballContainer;
    private float lastFallTime = 0;

    public void SpawnBall(int level, Vector3 p = default, Quaternion q = default)
    {
        if (level < 1 || level > balls.Count) return;

        GameObject b = balls[level - 1].prefab;
        Instantiate(b, p, q, ballContainer.transform);
    }

    private void FallAndDecideNextBall()
    {
        Instantiate(currentBall, fallAnchor.transform.position + Vector3.down, Quaternion.identity, ballContainer.transform);
        fallAnchor.GetComponent<SpriteRenderer>().color = nextBall.GetComponent<Ball>().color;
        fallAnchor.transform.localScale = Vector3.one * nextBall.GetComponent<Ball>().size;

        float r = GameManager.instance.RandomRange(0.0f, probabilitySum);
        foreach (var ball in balls)
        {
            r -= ball.probability;
            if (r <= 0)
            {
                currentBall = nextBall;
                nextBall = ball.prefab;
                break;
            }
        }
    }

    private void DecideBall()
    {
        float r = GameManager.instance.RandomRange(0.0f, probabilitySum);
        foreach (var ball in balls)
        {
            r -= ball.probability;
            if (r <= 0)
            {
                currentBall = ball.prefab;
                break;
            }
        }

        r = GameManager.instance.RandomRange(0.0f, probabilitySum);
        foreach (var ball in balls)
        {
            r -= ball.probability;
            if (r <= 0)
            {
                nextBall = ball.prefab;
                break;
            }
        }

        fallAnchor.GetComponent<SpriteRenderer>().color = currentBall.GetComponent<Ball>().color;
        fallAnchor.transform.localScale = Vector3.one * currentBall.GetComponent<Ball>().size;
    }

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        probabilitySum = 0;
        foreach (var ball in balls)
        {
            probabilitySum += ball.probability;
        }

        ballContainer = new GameObject("BallContainer");
        fallAnchor.transform.position = new Vector3(0, 1.7f, 0);
    }

    void Start()
    {
        DecideBall();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A) && fallAnchor.transform.position.x > leftLimit)
        {
            fallAnchor.transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) && fallAnchor.transform.position.x < rightLimit)
        {
            fallAnchor.transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastFallTime > coolTime)
        {
            lastFallTime = Time.time;
            FallAndDecideNextBall();
        }
    }
}
