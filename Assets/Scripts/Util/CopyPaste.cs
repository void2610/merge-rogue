using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class CopyPaste : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    //コピー
    [DllImport("__Internal")]
    private static extern void CopyWebGL(string str);
    //ペースト
    [DllImport("__Internal")]
    private static extern void PasteWeb();
    private string unityClip;
    private string webClip;

    void Start()
    {
        unityClip = "";
        webClip = "";
        PasteWeb(); 
    }

    void Update()
    {
        if(GUIUtility.systemCopyBuffer != unityClip){
            unityClip = GUIUtility.systemCopyBuffer;
            CopyWebGL(unityClip);
            Debug.Log("Copy unity to clip");
        }
        PasteWeb();
    }
    public void paste(string text){
        if(text != webClip){
            webClip = text;
            GUIUtility.systemCopyBuffer = webClip;
        }
    }
#else
#endif
}