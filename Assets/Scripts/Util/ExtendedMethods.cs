using UnityEngine;

public static class ExtendedMethods
{
    /// <summary>
    /// stringのListの値を全てLogする
    /// </summary>
    public static void Print(this System.Collections.Generic.List<string> list)
    {
        foreach (var s in list)
        {
            Debug.Log(s);
        }
    }
}
