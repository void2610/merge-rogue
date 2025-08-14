using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// StageEventDataのカスタムエディタ
/// ローカライゼーションテーブルから日本語テキストを表示
/// </summary>
[CustomEditor(typeof(StageEventData))]
public class StageEventDataEditor : Editor
{
    private static readonly Color _previewBoxColor = new Color(0.15f, 0.15f, 0.15f, 0.3f);
    private static readonly Color _warningColor = new Color(1f, 0.8f, 0.8f, 0.5f);
    private Dictionary<string, string> _japaneseLocalizations;
    
    private void OnEnable()
    {
        // ローカライゼーションデータを読み込み
        LoadLocalizationData();
    }
    
    /// <summary>
    /// ローカライゼーションデータを読み込み
    /// </summary>
    private void LoadLocalizationData()
    {
        _japaneseLocalizations = new Dictionary<string, string>();
        
        try
        {
            // 日本語テーブルファイルを直接読み込み
            var jaTablePath = "Assets/Localization/StringTable/StageEvent/StageEvent_ja.asset";
            var sharedDataPath = "Assets/Localization/StringTable/StageEvent/StageEvent Shared Data.asset";
            
            var jaTableText = System.IO.File.ReadAllText(jaTablePath);
            var sharedDataText = System.IO.File.ReadAllText(sharedDataPath);
            
            // 共有データからキーとIDの対応を取得
            var keyIdMap = ParseSharedData(sharedDataText);
            
            // 日本語テーブルからIDと文字列の対応を取得
            var idTextMap = ParseJapaneseTable(jaTableText);
            
            // キーと日本語文字列の対応を構築
            foreach (var keyId in keyIdMap)
            {
                if (idTextMap.TryGetValue(keyId.Value, out var localizedText))
                {
                    _japaneseLocalizations[keyId.Key] = localizedText;
                }
                else
                {
                    // IDが存在しない場合は空文字列として登録
                    _japaneseLocalizations[keyId.Key] = "";
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ローカライゼーションデータの読み込みに失敗: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 共有データファイルを解析してキー→ID対応を取得
    /// </summary>
    private Dictionary<string, string> ParseSharedData(string yamlText)
    {
        var keyIdMap = new Dictionary<string, string>();
        
        // YAMLからエントリを抽出
        var entryMatches = Regex.Matches(yamlText, @"- m_Id: (\d+)\s+m_Key: (\w+)");
        
        foreach (Match match in entryMatches)
        {
            var id = match.Groups[1].Value;
            var key = match.Groups[2].Value;
            keyIdMap[key] = id;
        }
        
        return keyIdMap;
    }
    
    /// <summary>
    /// 日本語テーブルファイルを解析してID→テキスト対応を取得
    /// </summary>
    private Dictionary<string, string> ParseJapaneseTable(string yamlText)
    {
        var idTextMap = new Dictionary<string, string>();
        
        // YAMLからローカライズされたテキストを抽出
        // 引用符あり・なし両方のパターンに対応、空文字列も含む
        var entryMatches = Regex.Matches(yamlText, @"- m_Id: (\d+)\s+m_Localized: (?:""(.*?)""|(.*))\s*$", RegexOptions.Multiline);
        
        foreach (Match match in entryMatches)
        {
            var id = match.Groups[1].Value;
            // 引用符ありパターン（グループ2）または引用符なしパターン（グループ3）から取得
            var localizedText = match.Groups[2].Success 
                ? match.Groups[2].Value 
                : match.Groups[3].Value;
            // Unicodeエスケープシーケンスをデコード
            localizedText = Regex.Unescape(localizedText);
            idTextMap[id] = localizedText;
        }
        
        return idTextMap;
    }
    
    public override void OnInspectorGUI()
    {
        var stageEventData = (StageEventData)target;
        
        serializedObject.Update();
        
        // 基本情報セクション
        DrawBasicInfoSection(stageEventData);
        
        EditorGUILayout.Space(10);
        
        // 選択肢セクション
        DrawOptionsSection(stageEventData);
        
        // 変更を適用
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    /// <summary>
    /// 基本情報セクションを描画
    /// </summary>
    private void DrawBasicInfoSection(StageEventData stageEventData)
    {
        EditorGUILayout.LabelField("基本情報", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            // Event ID
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventId"));
            
            // ローカライゼーションプレビュー
            if (!string.IsNullOrEmpty(stageEventData.eventId))
            {
                DrawLocalizationPreview(stageEventData);
            }
            
            // 重み
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"));
        }
    }
    
    /// <summary>
    /// ローカライゼーションプレビューを描画
    /// </summary>
    private void DrawLocalizationPreview(StageEventData stageEventData)
    {
        EditorGUILayout.Space(5);
        
        using (new EditorGUILayout.VerticalScope(GetPreviewBoxStyle()))
        {
            // メイン説明文
            var mainDescKey = stageEventData.GetMainDescriptionKey();
            var mainDesc = GetLocalizedText(mainDescKey);
            DrawLocalizationField("メイン説明文", mainDescKey, mainDesc, true);
        }
    }
    
    /// <summary>
    /// 選択肢セクションを描画
    /// </summary>
    private void DrawOptionsSection(StageEventData stageEventData)
    {
        EditorGUILayout.LabelField("選択肢", EditorStyles.boldLabel);
        
        var optionsProperty = serializedObject.FindProperty("options");
        
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            // 配列サイズを独自に描画
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("サイズ", GUILayout.Width(60));
                var sizeProperty = optionsProperty.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(sizeProperty, GUIContent.none);
            }
            
            // 各選択肢
            if (optionsProperty.arraySize == 0)
            {
                EditorGUILayout.Space(5);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("選択肢がありません", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.Space(5);
            }
            else
            {
                for (int i = 0; i < optionsProperty.arraySize; i++)
                {
                    DrawSingleOption(stageEventData, optionsProperty.GetArrayElementAtIndex(i), i);
                }
            }
        }
    }
    
    /// <summary>
    /// 単一の選択肢を描画
    /// </summary>
    private void DrawSingleOption(StageEventData stageEventData, SerializedProperty optionProperty, int index)
    {
        EditorGUILayout.Space(5);
        
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField($"選択肢 {index + 1}", EditorStyles.boldLabel);
            
            // Actions (SerializeReference対応)
            var actionsProperty = optionProperty.FindPropertyRelative("actions");
            EditorGUILayout.PropertyField(actionsProperty, new GUIContent("アクション"), true);
            
            // Is Endless
            EditorGUILayout.PropertyField(optionProperty.FindPropertyRelative("isEndless"));
            
            // ローカライゼーションプレビュー
            if (!string.IsNullOrEmpty(stageEventData.eventId))
            {
                DrawOptionLocalizationPreview(stageEventData, index);
            }
        }
    }
    
    /// <summary>
    /// オプションのローカライゼーションプレビューを描画
    /// </summary>
    private void DrawOptionLocalizationPreview(StageEventData stageEventData, int arrayIndex)
    {
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("ローカライゼーションプレビュー", EditorStyles.miniLabel);
        
        using (new EditorGUILayout.VerticalScope(GetPreviewBoxStyle()))
        {
            var dummyOption = new StageEventData.EventOptionData();
            
            // オプション説明
            var descKey = dummyOption.GetDescriptionKey(stageEventData.eventId, arrayIndex);
            var desc = GetLocalizedText(descKey);
            DrawLocalizationField("説明", descKey, desc);
            
            // 効果説明
            var effectKey = dummyOption.GetEffectDescriptionKey(stageEventData.eventId, arrayIndex);
            var effect = GetLocalizedText(effectKey);
            DrawLocalizationField("効果", effectKey, effect);
            
            // 結果説明
            var resultKey = dummyOption.GetResultDescriptionKey(stageEventData.eventId, arrayIndex);
            var result = GetLocalizedText(resultKey);
            DrawLocalizationField("結果", resultKey, result);
        }
    }
    
    /// <summary>
    /// ローカライゼーションフィールドを描画（読み取り専用）
    /// </summary>
    private void DrawLocalizationField(string label, string key, string localizedText, bool isTextArea = false)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"{label}:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"[{key}]", EditorStyles.miniLabel, GUILayout.Width(150));
        }
        
        var content = localizedText == null ? $"<未設定: {key}>" : localizedText;
        
        // メインの説明文（isTextArea）の場合は複数行表示、それ以外は省略表示
        if (isTextArea)
        {
            var style = GetLocalizedTextStyle(localizedText, true);
            // GUIを無効化して読み取り専用にする
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(content, style, GUILayout.MinHeight(40));
            }
        }
        else
        {
            // 改行を含む場合は最初の行のみ表示し、長すぎる場合は省略
            var displayContent = content;
            if (content.Contains('\n'))
            {
                displayContent = content.Substring(0, content.IndexOf('\n')) + "...";
            }
            else if (content.Length > 50)
            {
                displayContent = content.Substring(0, 47) + "...";
            }
            
            var style = GetLocalizedTextStyle(localizedText, false);
            // GUIを無効化して読み取り専用にする
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(displayContent, style);
            }
        }
    }
    
    /// <summary>
    /// ローカライズされたテキストを取得
    /// </summary>
    private string GetLocalizedText(string key)
    {
        if (_japaneseLocalizations == null || string.IsNullOrEmpty(key))
            return null;
            
        _japaneseLocalizations.TryGetValue(key, out var localizedText);
        return localizedText;
    }
    
    /// <summary>
    /// プレビューボックスのスタイルを取得
    /// </summary>
    private GUIStyle GetPreviewBoxStyle()
    {
        var style = new GUIStyle(EditorStyles.helpBox);
        style.normal.background = CreateColorTexture(_previewBoxColor);
        return style;
    }
    
    /// <summary>
    /// ローカライズテキストのスタイルを取得
    /// </summary>
    private GUIStyle GetLocalizedTextStyle(string localizedText, bool isTextArea = false)
    {
        // 読み取り専用フィールド用のベーススタイル
        var style = new GUIStyle(isTextArea ? EditorStyles.label : EditorStyles.textField);
        
        if (localizedText == null)
        {
            style.normal.background = CreateColorTexture(_warningColor);
            style.fontStyle = FontStyle.Italic;
            style.normal.textColor = Color.red;
        }
        else
        {
            style.fontStyle = FontStyle.Normal;
            if (!isTextArea)
            {
                // TextFieldの場合のみ背景色を設定
                var readOnlyColor = EditorGUIUtility.isProSkin ? 
                    new Color(0.08f, 0.08f, 0.08f, 1f) : 
                    new Color(0.65f, 0.65f, 0.65f, 1f);
                style.normal.background = CreateColorTexture(readOnlyColor);
            }
            var textColor = EditorGUIUtility.isProSkin ? 
                new Color(0.7f, 0.7f, 0.7f, 1f) : 
                new Color(0.2f, 0.2f, 0.2f, 1f);
            style.normal.textColor = textColor;
        }
        
        style.wordWrap = true;
        style.alignment = TextAnchor.UpperLeft;
        style.padding = new RectOffset(4, 4, 2, 2);
        style.fontSize = EditorStyles.label.fontSize;
        style.fixedHeight = 0;
        style.stretchHeight = true;
        return style;
    }
    
    /// <summary>
    /// 指定色のテクスチャを作成
    /// </summary>
    private Texture2D CreateColorTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
    
    /// <summary>
    /// カスタムリスト描画（はみ出し防止）
    /// </summary>
    private void DrawCustomList(SerializedProperty listProperty, string displayName)
    {
        if (listProperty == null) return;
        
        // フォールドアウトを無効化してラベルだけ表示
        EditorGUILayout.LabelField($"▼ {displayName}", EditorStyles.foldout);
        
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            // サイズフィールド
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("サイズ", GUILayout.Width(60));
                var sizeProperty = listProperty.FindPropertyRelative("Array.size");
                var newSize = EditorGUILayout.IntField(sizeProperty.intValue);
                if (newSize != sizeProperty.intValue && newSize >= 0)
                {
                    sizeProperty.intValue = newSize;
                }
            }
            
            // リスト要素
            if (listProperty.arraySize == 0)
            {
                EditorGUILayout.Space(5);
                var labelRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
                var labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                EditorGUI.LabelField(labelRect, "リストは空です", labelStyle);
                EditorGUILayout.Space(5);
            }
            else
            {
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    EditorGUILayout.Space(3);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var elementProperty = listProperty.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(elementProperty, new GUIContent($"要素 {i}"), true);
                    }
                }
            }
        }
    }
}