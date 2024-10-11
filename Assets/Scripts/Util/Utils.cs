using UnityEngine;
using System.Collections;

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
