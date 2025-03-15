using System;
using R3;
using UnityEngine;

public class DebugLogDisplay : MonoBehaviour
{
    private const int MAX_LOG_LINES = 50; // 表示するログの最大行数
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
        _logText += type switch
        {
            // エラーメッセージとスタックトレースをlogTextに追加
            LogType.Error or LogType.Exception => $"[Error] {logString}\n{stackTrace}\n",
            LogType.Warning => $"[Warning] {logString}\n{stackTrace}\n",
            _ => $"{logString}\n"
        };

        // 表示するログの行数がMAX_LOG_LINESを超えたら古いログを削除
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