using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [SerializeField]
    private List<GameObject> inventory = new List<GameObject>();

    public void AddBall(GameObject ball)
    {
        inventory.Add(ball);
    }

    public void RemoveBall(int index)
    {
        if (index >= 0 && index < inventory.Count)
        {
            inventory.RemoveAt(index);
        }
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
