using UnityEngine;

public class VersionText : MonoBehaviour
{
    [SerializeField] private string version = "0.0.0";
    private void Awake()
    {
        var t = $"Ver.{version}";
        
        #if DEMO_PLAY
        t += " (demo)";
        Debug.Log("Demo Build");
        # else
        Debug.Log("Full Build");
        #endif
        
        GetComponent<TMPro.TextMeshProUGUI>().text = t;
    }
}
