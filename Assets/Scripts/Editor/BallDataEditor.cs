using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// BallDataのカスタムエディタ
/// ローカライゼーションテーブルから日本語テキストを表示
/// </summary>
[CustomEditor(typeof(BallData))]
public class BallDataEditor : Editor
{
    private static readonly Color _previewBoxColor = new Color(0.15f, 0.15f, 0.15f, 0.3f);
    private static readonly Color _warningColor = new Color(1f, 0.8f, 0.8f, 0.5f);
    private Dictionary<string, string> _japaneseLocalizations;
    
    private void OnEnable()
    {
        // ローカライゼーションデータを読み込み
        _japaneseLocalizations = LocalizationEditorHelper.LoadJapaneseLocalization("Ball");
    }
    
    public override void OnInspectorGUI()
    {
        var ballData = (BallData)target;
        
        serializedObject.Update();
        
        // 基本情報セクション
        DrawBasicInfoSection();
        
        EditorGUILayout.Space(10);
        
        // パラメータセクション
        DrawParametersSection();
        
        EditorGUILayout.Space(10);
        
        // ローカライゼーションプレビューセクション
        if (!string.IsNullOrEmpty(ballData.className))
        {
            DrawLocalizationPreviewSection(ballData);
        }
        
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
    private void DrawBasicInfoSection()
    {
        EditorGUILayout.LabelField("基本情報", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("className"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sprite"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rarity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("availableDemo"));
            
            // レガシーフィールド（非推奨）
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("レガシーフィールド（非推奨）", EditorStyles.miniLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("descriptions"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("flavorText"));
            }
        }
    }
    
    /// <summary>
    /// パラメータセクションを描画
    /// </summary>
    private void DrawParametersSection()
    {
        EditorGUILayout.LabelField("パラメータ（レベル別）", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attacks"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sizes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weights"));
        }
    }
    
    /// <summary>
    /// ローカライゼーションプレビューセクションを描画
    /// </summary>
    private void DrawLocalizationPreviewSection(BallData ballData)
    {
        EditorGUILayout.LabelField("ローカライゼーションプレビュー", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.VerticalScope(GetPreviewBoxStyle()))
        {
            // 表示名
            var nameKey = $"{ballData.className}_N";
            var nameText = GetLocalizedText(nameKey);
            DrawLocalizationField("表示名", nameKey, nameText);
            
            EditorGUILayout.Space(5);
            
            // 説明文
            var descKey = $"{ballData.className}_D";
            var descText = GetLocalizedText(descKey);
            DrawLocalizationField("説明", descKey, descText, true);
            
            EditorGUILayout.Space(5);
            
            // フレーバーテキスト
            var flavorKey = $"{ballData.className}_F";
            var flavorText = GetLocalizedText(flavorKey);
            DrawLocalizationField("フレーバー", flavorKey, flavorText, true);
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
            EditorGUILayout.LabelField($"[{key}]", EditorStyles.miniLabel);
        }
        
        var content = localizedText == null ? $"<未設定: {key}>" : localizedText;
        
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
            var style = GetLocalizedTextStyle(localizedText, false);
            // GUIを無効化して読み取り専用にする
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(content, style);
            }
        }
    }
    
    /// <summary>
    /// ローカライズされたテキストを取得
    /// </summary>
    private string GetLocalizedText(string key)
    {
        return LocalizationEditorHelper.GetLocalizedText(_japaneseLocalizations, key);
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
}