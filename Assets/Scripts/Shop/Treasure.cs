using UnityEngine;
using System.Collections.Generic;

public class Treasure : MonoBehaviour
{
    [SerializeField] private List<GameObject> items;
    private const int MAX_ITEMS = 4;
    
    public void OpenTreasure(int count = 1)
    {
        if(count is > MAX_ITEMS or < 0) Debug.LogError("Count is bigger than items count");
        
        
        
    }
    
    private void Awake()
    {
        foreach (var item in items)
        {
            item.SetActive(false);
        }
    }
}
