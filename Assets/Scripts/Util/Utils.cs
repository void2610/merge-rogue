using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour
{
    public static Utils instance;
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
    public void WaitAndInvoke(float time, System.Action action)
    {
        StartCoroutine(_WaitAndInvoke(time, action));
    }
    private IEnumerator _WaitAndInvoke(float time, System.Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
}
