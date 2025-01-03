using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Utils : MonoBehaviour
{
    public static Utils Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    
    public static void AddEventToObject(GameObject obj, System.Action action, EventTriggerType type)
    {
        var trigger = obj.GetComponent<EventTrigger>();
        if (!trigger)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }
        trigger.triggers.RemoveAll(x => x.eventID == type);
        
        var entry = new EventTrigger.Entry {eventID = type};
        entry.callback.AddListener((data) => action());
        trigger.triggers.Add(entry);
    }
    
    public static void RemoveAllEventFromObject(GameObject obj)
    {
        var trigger = obj.GetComponent<EventTrigger>();
        if (trigger)
            trigger.triggers.Clear();
        var button = obj.GetComponent<UnityEngine.UI.Button>();
        if (button)
            button.onClick.RemoveAllListeners();
    }
}
