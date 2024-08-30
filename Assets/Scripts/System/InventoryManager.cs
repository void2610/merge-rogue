using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [SerializeField]
    private List<GameObject> inventory = new List<GameObject>();

    public void AddBall(GameObject ball)
    {
        int l = ball.GetComponent<BallBase>().level;
        inventory[l - 1] = ball;
    }

    public void RemoveBall(int index)
    {
        if (index >= 0 && index < inventory.Count)
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
        float total = 0;
        foreach (var b in inventory)
        {
            if (b != null)
                total += b.GetComponent<BallBase>().probability;
        }
        float r = GameManager.instance.RandomRange(0.0f, total);
        foreach (var b in inventory)
        {
            if (b != null)
            {
                r -= b.GetComponent<BallBase>().probability;
                if (r <= 0)
                {
                    return b;
                }
            }
        }
        return inventory[0];
    }

    public GameObject GetBallByLevel(int level)
    {
        if (level > 0 && level <= inventory.Count)
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
    }
}
