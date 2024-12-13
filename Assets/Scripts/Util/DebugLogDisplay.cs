using UnityEngine;

public class DebugLogDisplay : MonoBehaviour
{
    private const int MAX_LOG_LINES = 10; // 表示するログの最大行数
    private string logText = "";
    private readonly GUIStyle guiStyle = new();

    private void Start()
    {
        // ログのテキストをスタイルに設定
        guiStyle.fontSize = 20;
        guiStyle.normal.textColor = Color.white;

        // エディタで実行していない場合のみ、ゲーム画面内のログを表示
        Debug.Log("Start DebugLogDisplay");
    }

    private void OnGUI()
    {
        // ゲーム画面中にログを表示（Windowsのビルド時のみ有効かつエディタで実行していない場合のみ有効かつ0キーで表示/非表示を切り替え）
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            logText = "";
        }
        GUI.Label(new Rect(10, 50, Screen.width, Screen.height), logText, guiStyle);
    }

    private void OnEnable()
    {
        // デバッグログを表示するためのイベントハンドラを登録
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        // イベントハンドラを解除
        Application.logMessageReceived -= HandleLog;
    }
    
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Debug.Log()のテキストをlogTextに追加
        logText += logString + "\n";

        // 表示するログの行数がMaxLogLinesを超えたら、古いログを削除
        var logLines = logText.Split('\n');
        if (logLines.Length > MAX_LOG_LINES)
        {
            logText = string.Join("\n", logLines, logLines.Length - MAX_LOG_LINES, MAX_LOG_LINES);
        }
    }
}