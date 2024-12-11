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
        if (trigger == null)
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
        if (trigger == null) return;
        trigger.triggers.Clear();
    }
    
    public void WaitAndInvoke(float time, System.Action action)
    {
        StartCoroutine(_WaitAndInvoke(time, action));
    }
    private IEnumerator _WaitAndInvoke(float time, System.Action action)
    {
        yield return new WaitForSecondsRealtime(time);
        action();
    }
}
