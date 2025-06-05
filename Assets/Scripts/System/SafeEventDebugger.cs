#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using SafeEventSystem;

/// <summary>
/// 新しい安全なイベントシステムのデバッグツール
/// エディタでモディファイアの状態を監視し、問題を特定する
/// </summary>
[System.Serializable]
public class SafeEventDebugger : EditorWindow
{
    private Vector2 _scrollPosition;
    private bool _showCoinEvents = true;
    private bool _showAttackEvents = true;
    private bool _showDamageEvents = true;
    private bool _showSimpleEvents = true;
    private bool _autoRefresh = true;
    private double _lastRefreshTime;
    private const double REFRESH_INTERVAL = 1.0; // 1秒間隔

    [MenuItem("Tools/Safe Event Debugger")]
    public static void ShowWindow()
    {
        GetWindow<SafeEventDebugger>("Safe Event Debugger");
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("This tool is only available during Play Mode.", MessageType.Info);
            return;
        }

        DrawHeader();
        DrawEventSections();
        DrawControlButtons();
        
        if (_autoRefresh && EditorApplication.timeSinceStartup - _lastRefreshTime > REFRESH_INTERVAL)
        {
            _lastRefreshTime = EditorApplication.timeSinceStartup;
            Repaint();
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Safe Event System Monitor", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            Repaint();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEventSections()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        // コイン関連イベント
        _showCoinEvents = EditorGUILayout.Foldout(_showCoinEvents, "💰 Coin Events", true);
        if (_showCoinEvents)
        {
            EditorGUI.indentLevel++;
            DrawModifiableEvent("Coin Gain", SafeEventManager.OnCoinGain);
            DrawModifiableEvent("Coin Consume", SafeEventManager.OnCoinConsume);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // 攻撃関連イベント
        _showAttackEvents = EditorGUILayout.Foldout(_showAttackEvents, "⚔️ Attack Events", true);
        if (_showAttackEvents)
        {
            EditorGUI.indentLevel++;
            DrawModifiableEvent("Player Attack", SafeEventManager.OnPlayerAttack);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // ダメージ・回復関連イベント
        _showDamageEvents = EditorGUILayout.Foldout(_showDamageEvents, "❤️ Health Events", true);
        if (_showDamageEvents)
        {
            EditorGUI.indentLevel++;
            DrawModifiableEvent("Player Damage", SafeEventManager.OnPlayerDamage);
            DrawModifiableEvent("Player Heal", SafeEventManager.OnPlayerHeal);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // シンプルイベント
        _showSimpleEvents = EditorGUILayout.Foldout(_showSimpleEvents, "📢 Simple Events", true);
        if (_showSimpleEvents)
        {
            EditorGUI.indentLevel++;
            DrawSimpleEventStatus("Battle Start", SafeEventManager.OnBattleStartSimple);
            DrawSimpleEventStatus("Enemy Defeated", SafeEventManager.OnEnemyDefeatedSimple);
            DrawSimpleEventStatus("Shop Enter", SafeEventManager.OnShopEnterSimple);
            DrawSimpleEventStatus("Rest Enter", SafeEventManager.OnRestEnterSimple);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawModifiableEvent<T>(string eventName, ModifiableEvent<T> modifiableEvent) where T : struct
    {
        var modifiers = modifiableEvent.GetModifiers();
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField(eventName, EditorStyles.boldLabel);
        
        if (modifiers.Count == 0)
        {
            EditorGUILayout.LabelField("No modifiers registered", EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.LabelField($"Modifiers: {modifiers.Count}", EditorStyles.miniLabel);
            
            EditorGUI.indentLevel++;
            foreach (var modifier in modifiers.OrderBy(m => (int)m.Phase).ThenBy(m => m.Priority))
            {
                var ownerName = modifier.Owner?.GetType().Name ?? "Unknown";
                var modifierName = modifier.GetType().Name;
                var phase = modifier.Phase;
                var priority = modifier.Priority;
                
                EditorGUILayout.BeginHorizontal();
                
                // フェーズに応じた色分け
                var phaseColor = GetPhaseColor(phase);
                var originalColor = GUI.color;
                GUI.color = phaseColor;
                GUILayout.Label("●", GUILayout.Width(12));
                GUI.color = originalColor;
                
                EditorGUILayout.LabelField($"{ownerName} ({modifierName})", GUILayout.MinWidth(150));
                EditorGUILayout.LabelField($"{phase} (P:{priority})", EditorStyles.miniLabel, GUILayout.Width(120));
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawSimpleEventStatus<T>(string eventName, R3.Subject<T> subject)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(eventName, GUILayout.MinWidth(100));
        
        // Subjectの購読者数を表示（R3のSubjectは直接アクセスできないため、概算）
        EditorGUILayout.LabelField("Active", EditorStyles.miniLabel);
        
        EditorGUILayout.EndHorizontal();
    }

    private Color GetPhaseColor(ModificationPhase phase)
    {
        return phase switch
        {
            ModificationPhase.PreProcess => Color.cyan,
            ModificationPhase.Addition => Color.green,
            ModificationPhase.Multiplication => Color.yellow,
            ModificationPhase.Conversion => Color.magenta,
            ModificationPhase.Override => Color.red,
            ModificationPhase.PostProcess => Color.blue,
            _ => Color.white
        };
    }

    private void DrawControlButtons()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Test Coin Gain (+100)"))
        {
            if (GameManager.Instance)
            {
                GameManager.Instance.AddCoin(100);
            }
        }
        
        if (GUILayout.Button("Test Battle Start"))
        {
            SafeEventManager.TriggerBattleStart();
        }
        
        if (GUILayout.Button("Print All Modifiers"))
        {
            SafeEventManager.DebugPrintAllModifiers();
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Legend:", EditorStyles.boldLabel);
        
        DrawLegendItem(GetPhaseColor(ModificationPhase.PreProcess), "PreProcess", "前処理（条件チェック等）");
        DrawLegendItem(GetPhaseColor(ModificationPhase.Addition), "Addition", "加算修正（+5攻撃力等）");
        DrawLegendItem(GetPhaseColor(ModificationPhase.Multiplication), "Multiplication", "乗算修正（x2コイン等）");
        DrawLegendItem(GetPhaseColor(ModificationPhase.Conversion), "Conversion", "変換修正（単体→全体等）");
        DrawLegendItem(GetPhaseColor(ModificationPhase.Override), "Override", "上書き修正（コイン消費→0等）");
        DrawLegendItem(GetPhaseColor(ModificationPhase.PostProcess), "PostProcess", "後処理（状態異常付与等）");
        
        EditorGUILayout.EndVertical();
    }

    private void DrawLegendItem(Color color, string phase, string description)
    {
        EditorGUILayout.BeginHorizontal();
        
        var originalColor = GUI.color;
        GUI.color = color;
        GUILayout.Label("●", GUILayout.Width(12));
        GUI.color = originalColor;
        
        EditorGUILayout.LabelField($"{phase}: {description}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void OnInspectorUpdate()
    {
        if (_autoRefresh)
        {
            Repaint();
        }
    }
}

/// <summary>
/// 実行時にSafeEventSystemの状態を監視するコンポーネント
/// </summary>
public class SafeEventRuntimeMonitor : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool logEventProcessing = false;
    public bool logModifierCounts = false;
    
    private void Start()
    {
        if (logEventProcessing)
        {
            // イベント処理のログを有効化
            Debug.Log("[SafeEventMonitor] Event processing logging enabled");
        }
    }
    
    private void Update()
    {
        if (logModifierCounts && Time.frameCount % 300 == 0) // 5秒毎
        {
            LogModifierCounts();
        }
    }
    
    private void LogModifierCounts()
    {
        var coinGainModifiers = SafeEventManager.OnCoinGain.GetModifiers().Count;
        var coinConsumeModifiers = SafeEventManager.OnCoinConsume.GetModifiers().Count;
        var attackModifiers = SafeEventManager.OnPlayerAttack.GetModifiers().Count;
        var damageModifiers = SafeEventManager.OnPlayerDamage.GetModifiers().Count;
        
        Debug.Log($"[SafeEventMonitor] Modifier counts - CoinGain:{coinGainModifiers}, CoinConsume:{coinConsumeModifiers}, Attack:{attackModifiers}, Damage:{damageModifiers}");
    }

    [ContextMenu("Force Print All Modifiers")]
    public void ForcePrintAllModifiers()
    {
        SafeEventManager.DebugPrintAllModifiers();
    }
}
#endif