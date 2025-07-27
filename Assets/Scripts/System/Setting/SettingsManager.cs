using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using Cysharp.Threading.Tasks;

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
        var gameSpeedSetting = new EnumSetting("ゲーム速度", "ゲームの速度を調整します", 
            new[] { "1.0", "2.0", "3.0" }, "1.0", new[] { "x1", "x2", "x3" });
        gameSpeedSetting.OnValueChanged.Subscribe(v =>
        {
            // タイトルシーンでは速度変更しない
            if (SceneManager.GetActiveScene().name == "TitleScene") return; 
            if (float.TryParse(v, out var speed)) GameManager.Instance.TimeScale = speed;
        }).AddTo(_disposables);
        _settings.Add(gameSpeedSetting);
        
        // フルスクリーン切り替え
        var fullscreenSetting = new EnumSetting("フルスクリーン", "フルスクリーン表示の切り替え", 
            new[] { "false", "true" }, Screen.fullScreen ? "true" : "false", new[] { "オフ", "オン" });
        fullscreenSetting.OnValueChanged.Subscribe(v => Screen.fullScreen = v == "true").AddTo(_disposables);
        _settings.Add(fullscreenSetting);
        
        // シードタイプ設定
        var seedTypeSetting = new EnumSetting("シードタイプ", "シード値の生成方法を選択します", 
            new[] { "manual", "random" }, "random", new[] { "手動指定", "ランダム生成" });
        _settings.Add(seedTypeSetting);
        
        // シード値設定
        var seedSetting = new TextInputSetting("シード値", "ランダム生成のシード値を設定します（手動指定時のみ有効）", "", 20, "シード値を入力");
        _settings.Add(seedSetting);
        
        // シード値ランダム生成ボタン
        var randomSeedSetting = new ButtonSetting("ランダムシード生成", "ランダムなシード値を生成して手動指定に設定します", "生成");
        randomSeedSetting.ButtonAction = () => {
            var guid = Guid.NewGuid();
            var randomSeed = guid.ToString("N")[..8]; // 8文字のランダム文字列
            seedTypeSetting.CurrentValue = "manual"; // シードタイプを手動指定に変更
            seedSetting.CurrentValue = randomSeed;   // ランダムシードを設定
        };
        _settings.Add(randomSeedSetting);
        
        // 言語設定
        var defaultLanguage = GetCurrentLanguageCode();
        var languageSetting = new EnumSetting("言語", "ゲーム内で使用する言語を設定します", 
            new[] { "ja", "en" }, defaultLanguage, new[] { "日本語", "English" });
        languageSetting.OnValueChanged.Subscribe(SetLanguage).AddTo(_disposables);
        _settings.Add(languageSetting);
        
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
            setting.OnSettingChanged.Subscribe(_ => LoadSettingsImpl(setting)).AddTo(_disposables);
        return;
        
        void LoadSettingsImpl(ISettingBase setting)
        {
            _onSettingChanged.OnNext(setting.SettingName);
            SaveSettings(); // 設定変更時に自動保存
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
    /// シード値を取得
    /// </summary>
    public string GetSeedValue()
    {
        var seedTypeSetting = GetSetting<EnumSetting>("シードタイプ");
        var seedType = seedTypeSetting?.CurrentValue ?? "random";
        
        if (seedType == "random")
        {
            // ランダムシードを生成
            var guid = Guid.NewGuid();
            return guid.ToString("N")[..8]; // 8文字のランダム文字列
        }
        else
        {
            // 手動指定のシード値を取得
            var seedSetting = GetSetting<TextInputSetting>("シード値");
            var manualSeed = seedSetting?.CurrentValue ?? "";
            
            // 手動指定でも空の場合はランダムシードを生成
            if (string.IsNullOrEmpty(manualSeed))
            {
                var guid = Guid.NewGuid();
                return guid.ToString("N")[..8];
            }
            
            return manualSeed;
        }
    }
    
    /// <summary>
    /// 言語コードからLocaleを設定
    /// </summary>
    private void SetLanguage(string languageCode)
    {
        SetLanguageImpl().Forget();
        return;

        async UniTaskVoid SetLanguageImpl()
        {
            try
            {
                // ローカライゼーション設定の初期化を待つ
                await LocalizationSettings.InitializationOperation.Task;

                // 対応する言語を検索
                var availableLocales = LocalizationSettings.AvailableLocales.Locales;
                var targetLocale = availableLocales.FirstOrDefault(locale => locale.Identifier.Code == languageCode);
                if (targetLocale) LocalizationSettings.SelectedLocale = targetLocale;
            }
            catch (Exception e) { Debug.LogError($"{e.Message}"); }
        }
    }
    
    /// <summary>
    /// 現在の言語コードを取得
    /// </summary>
    private string GetCurrentLanguageCode()
    {
        try
        {
            if (LocalizationSettings.InitializationOperation.IsDone && LocalizationSettings.SelectedLocale)
            {
                var currentCode = LocalizationSettings.SelectedLocale.Identifier.Code;
                if (currentCode is "ja" or "en") return currentCode;
            }
            return "en";
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}");
            return "en";
        }
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
    [Serializable]
    private class SettingsData
    {
        public SerializableDictionary<string, string> settingValues = new();
    }
}