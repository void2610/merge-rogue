using System;
using R3;
using UnityEngine;

public class DebugLogDisplay : MonoBehaviour
{
    private const int MAX_LOG_LINES = 10; // 表示するログの最大行数
    private string _logText = "";
    private readonly GUIStyle _guiStyle = new();

    private void Awake()
    {
        // ログのテキストをスタイルに設定
        _guiStyle.fontSize = 15;
        _guiStyle.normal.textColor = Color.white;
        
        // 一定間隔で最初の行を削除
        Observable.Interval(System.TimeSpan.FromSeconds(5))
            .Subscribe(_ => DeleteFirstLine())
            .AddTo(this);
    }

    private void OnGUI()
    {
        // ゲーム画面中にログを表示（Windowsのビルド時のみ有効かつエディタで実行していない場合のみ有効かつ0キーで表示/非表示を切り替え）
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _logText = "";
        }
        GUI.Label(new Rect(10, 50, Screen.width, Screen.height), _logText, _guiStyle);
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
        _logText += logString + "\n";

        // 表示するログの行数がMaxLogLinesを超えたら、古いログを削除
        var logLines = _logText.Split('\n');
        if (logLines.Length > MAX_LOG_LINES)
        {
            _logText = string.Join("\n", logLines, logLines.Length - MAX_LOG_LINES, MAX_LOG_LINES);
        }
    }

    private void DeleteFirstLine()
    {
        var logLines = _logText.Split('\n');
        if (logLines.Length > 1)
        {
            _logText = string.Join("\n", logLines, 1, logLines.Length - 1);
        }
    }
}