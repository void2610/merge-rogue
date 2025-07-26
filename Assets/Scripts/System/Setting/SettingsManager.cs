using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム設定の管理を行うサービスクラス
/// VContainerでシングルトンとして注入される
/// </summary>
public class SettingsManager : IDisposable
{
    private readonly List<ISettingBase> _settings = new();
    private readonly Subject<string> _onSettingChanged = new();
    private readonly CompositeDisposable _disposables = new();
    
    private static string SettingsFilePath => System.IO.Path.Combine(Application.persistentDataPath, "game_settings.json");
    
    /// <summary>
    /// 全ての設定項目の読み取り専用リスト
    /// </summary>
    public IReadOnlyList<ISettingBase> Settings => _settings.AsReadOnly();
    
    public SettingsManager()
    {
        // 既存の設定がない場合は初期設定を作成
        if (_settings.Count == 0) InitializeDefaultSettings();
        
        // 各設定の値変更イベントを監視
        SubscribeToSettingChanges();
        
        // セーブデータから設定を読み込む
        LoadSettings();
        ApplyCurrentValues();
    }
    
    /// <summary>
    /// デフォルト設定を初期化
    /// OnValueChanged.Subscribe でシステムに直接反映するパターン
    /// </summary>
    private void InitializeDefaultSettings()
    {
        // BGM音量設定
        var bgmSetting = new SliderSetting("BGM音量", "バックグラウンドミュージックの音量", 0.8f, 0f, 1f);
        bgmSetting.OnValueChanged.Subscribe(v => BgmManager.Instance.BgmVolume = v).AddTo(_disposables);
        _settings.Add(bgmSetting);
        
        // SE音量設定
        var seSetting = new SliderSetting("SE音量", "効果音の音量", 0.8f, 0f, 1f);
        seSetting.OnValueChanged.Subscribe(v => SeManager.Instance.SeVolume = v).AddTo(_disposables);
        _settings.Add(seSetting);
        
        // SE音量テスト
        var seTestSetting = new ButtonSetting("SE音量テスト", "現在のSE音量で効果音を再生します", "再生");
        seTestSetting.ButtonAction = () => SeManager.Instance.PlaySe("Test");
        _settings.Add(seTestSetting);
        
        // ゲーム速度設定
        var gameSpeedSetting = new SliderSetting("ゲーム速度", "ゲームの速度を調整します", 1f, 0.1f, 3f);
        gameSpeedSetting.OnValueChanged.Subscribe(v =>
        {
            if (SceneManager.GetActiveScene().name == "TitleScene") return; // タイトルシーンでは速度変更しない
            Time.timeScale = v;
            // GameManagerのTimeScaleも更新
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TimeScale = v;
            }
        }).AddTo(_disposables);
        _settings.Add(gameSpeedSetting);
        
        // フルスクリーン切り替え
        var fullscreenSetting = new EnumSetting("フルスクリーン", "フルスクリーン表示の切り替え", 
            new[] { "false", "true" }, Screen.fullScreen ? "true" : "false", new[] { "オフ", "オン" });
        fullscreenSetting.OnValueChanged.Subscribe(v => Screen.fullScreen = v == "true").AddTo(_disposables);
        _settings.Add(fullscreenSetting);
        
        // 設定のリセットボタン
        var deleteDataSetting = new ButtonSetting(
            "設定のリセット", 
            "設定をデフォルト値に戻します",
            "デフォルトに戻す", 
            true, 
            "本当に設定をリセットしますか？"
        );
        deleteDataSetting.ButtonAction = ResetAllSettings;
        _settings.Add(deleteDataSetting);
    }
    
    /// <summary>
    /// 各設定の値変更イベントを監視（通知とセーブ処理）
    /// </summary>
    private void SubscribeToSettingChanges()
    {
        foreach (var setting in _settings)
        {
            setting.OnSettingChanged
                .Subscribe(_ => {
                    _onSettingChanged.OnNext(setting.SettingName);
                    SaveSettings(); // 設定変更時に自動保存
                })
                .AddTo(_disposables);
        }
    }
    
    /// <summary>
    /// 設定名で設定項目を取得
    /// </summary>
    public T GetSetting<T>(string settingName) where T : class, ISettingBase
    {
        return _settings.FirstOrDefault(s => s.SettingName == settingName) as T;
    }
    
    /// <summary>
    /// すべての設定をデフォルト値にリセット
    /// </summary>
    private void ResetAllSettings()
    {
        foreach (var setting in _settings)
        {
            setting.ResetToDefault();
        }
    }
    
    /// <summary>
    /// 設定データをファイルに保存
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            var settingsData = new SettingsData();
            
            foreach (var setting in _settings)
            {
                settingsData.settingValues[setting.SettingName] = setting.SerializeValue();
            }
            
            var json = JsonUtility.ToJson(settingsData, true);
            
#if UNITY_WEBGL && !UNITY_EDITOR
            PlayerPrefs.SetString("GameSettings", json);
            PlayerPrefs.Save();
#else
            System.IO.File.WriteAllText(SettingsFilePath, json);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"設定データの保存に失敗しました: {e.Message}");
        }
    }
    
    /// <summary>
    /// ファイルから設定データを読み込み
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            string json;
            
#if UNITY_WEBGL && !UNITY_EDITOR
            json = PlayerPrefs.GetString("GameSettings", "");
            if (string.IsNullOrEmpty(json)) return;
#else
            var filePath = SettingsFilePath;
            if (!System.IO.File.Exists(filePath)) return;
            json = System.IO.File.ReadAllText(filePath);
#endif
            
            var settingsData = JsonUtility.FromJson<SettingsData>(json);
            
            if (settingsData?.settingValues != null)
            {
                foreach (var setting in _settings)
                {
                    if (settingsData.settingValues.TryGetValue(setting.SettingName, out var value))
                    {
                        setting.DeserializeValue(value);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"設定データの読み込みに失敗しました: {e.Message}");
        }
    }
    
    /// <summary>
    /// 現在の設定値を適用
    /// </summary>
    private void ApplyCurrentValues()
    {
        foreach (var setting in _settings)
        {
            setting.ApplyCurrentValue();
        }
    }
    
    /// <summary>
    /// リソース解放
    /// </summary>
    public void Dispose()
    {
        _disposables?.Dispose();
    }
    
    /// <summary>
    /// 設定データのシリアライゼーション用クラス
    /// </summary>
    [System.Serializable]
    private class SettingsData
    {
        public SerializableDictionary<string, string> settingValues = new();
    }
}