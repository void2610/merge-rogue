using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [SerializeField]
    private int inventorySize = 6;
    [SerializeField]
    private List<GameObject> inventory = new List<GameObject>();

    private List<float> probabilities = new List<float> { 1f, 0.8f, 0.6f, 0.4f, 0.2f, 0.0f };

    public void AddBall(GameObject ball)
    {
        // TODO: 大きさを自動で変える ボールの大きさ倍率*レベル
        int l = ball.GetComponent<BallBase>().level;
        inventory[l - 1] = ball;
    }

    public void RemoveBall(int index)
    {
        if (index >= 0 && index < inventorySize)
        {
            inventory.RemoveAt(index);
        }
    }

    public List<GameObject> GetInventory()
    {
        return inventory;
    }

    public GameObject GetRandomBall()
    {
        float total = probabilities.Sum();
        float r = GameManager.instance.RandomRange(0.0f, total);
        for (int i = 0; i < inventorySize; i++)
        {
            if (r < probabilities[i])
            {
                return inventory[i];
            }
            r -= probabilities[i];
        }
        return inventory[0];
    }

    public GameObject GetBallByLevel(int level)
    {
        if (level > 0 && level <= inventorySize)
        {
            return inventory[level - 1];
        }
        return null;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        for (int i = 0; i < inventorySize; i++)
        {
            var ball = BallFactory.instance.CreateNormalBall();
            ball.GetComponent<BallBase>().level = i + 1;
            inventory.Add(ball);
        }
    }

    private void Start()
    {

    }
}
