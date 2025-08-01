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
    
    /// <summary>
    /// 設定が変更された時のイベント（UIの更新通知用）
    /// </summary>
    public Observable<string> OnSettingChanged => _onSettingChanged;
    
    public SettingsManager()
    {
        // LocalizeStringLoaderの初期化を待ってから設定を初期化
        InitializeAsync().Forget();
    }
    
    private async UniTaskVoid InitializeAsync()
    {
        // LocalizeStringLoaderの初期化を待機
        if (LocalizeStringLoader.Instance)
        {
            await LocalizeStringLoader.Instance.WaitForInitialization();
        }
        
        InitializeDefaultSettings();
        // 各設定の値変更イベントを監視
        SubscribeToSettingChanges();
        // セーバータから設定を読み込む
        LoadSettings();
        
        // 初期化完了後にGameManagerのゲーム速度を設定
        if (SceneManager.GetActiveScene().name == "MainScene" && GameManager.Instance)
        {
            GameManager.Instance.ApplyGameSpeedFromSettings();
        }
    }
    
    /// <summary>
    /// デフォルト設定を初期化
    /// OnValueChanged.Subscribe でシステムに直接反映するパターン
    /// </summary>
    private void InitializeDefaultSettings()
    {
        // BGM音量設定
        var bgmSetting = new SliderSetting("BGM_VOLUME", 0.8f, 0f, 1f);
        bgmSetting.OnValueChanged.Subscribe(v => BgmManager.Instance.BgmVolume = v).AddTo(_disposables);
        _settings.Add(bgmSetting);
        
        // SE音量設定
        var seSetting = new SliderSetting("SE_VOLUME", 0.8f, 0f, 1f);
        seSetting.OnValueChanged.Subscribe(v => SeManager.Instance.SeVolume = v).AddTo(_disposables);
        _settings.Add(seSetting);
        
        // SE音量テスト
        var seTestSetting = new ButtonSetting("SE_VOLUME_TEST")
        {
            ButtonAction = () => SeManager.Instance.PlaySe("Test")
        };
        _settings.Add(seTestSetting);
        
        // ゲーム速度設定
        var gameSpeedSetting = new EnumSetting("GAME_SPEED", new[] { "1.0", "2.0", "3.0" }, 0);
        gameSpeedSetting.OnValueChanged.Subscribe(v =>
        {
            // MainSceneでGameManagerが存在する場合のみ速度を変更
            if (SceneManager.GetActiveScene().name == "MainScene" && GameManager.Instance) 
            {
                GameManager.Instance.ApplyGameSpeedFromSettings();
            }
        }).AddTo(_disposables);
        _settings.Add(gameSpeedSetting);
        
        // フルスクリーン切り替え
        var fullscreenSetting = new EnumSetting("FULL_SCREEN", new[] { "false", "true" }, Screen.fullScreen ? 1 : 0);
        fullscreenSetting.OnValueChanged.Subscribe(v => Screen.fullScreen = v == "true").AddTo(_disposables);
        _settings.Add(fullscreenSetting);
        
        // シードタイプ設定
        var seedTypeSetting = new EnumSetting("SEED_TYPE", new[] { "manual", "random" }, 1);
        _settings.Add(seedTypeSetting);
        
        // シード値設定
        var seedSetting = new TextInputSetting("SEED_VALUE", "", 20);
        _settings.Add(seedSetting);
        
        // 言語設定
        var defaultLanguage = GetCurrentLanguageCode();
        var defaultLanguageIndex = defaultLanguage == "ja" ? 0 : 1;
        var languageSetting = new EnumSetting("LANGUAGE", new[] { "ja", "en" }, defaultLanguageIndex);
        languageSetting.OnValueChanged.Subscribe(SetLanguage).AddTo(_disposables);
        _settings.Add(languageSetting);
        
        // 設定のリセットボタン
        var deleteDataSetting = new ButtonSetting("RESET", true)
        {
            ButtonAction = ResetAllSettings
        };
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
    /// 設定名またはローカライズキーで設定項目を取得
    /// </summary>
    public T GetSetting<T>(string settingKey) where T : class, ISettingBase
    {
        // まずローカライズキーで検索
        var setting = _settings.FirstOrDefault(s => s.LocalizeKey == settingKey);
        // 見つからない場合は表示名で検索（後方互換性のため）
        setting ??= _settings.FirstOrDefault(s => s.SettingName == settingKey);
        return setting as T;
    }
    
    /// <summary>
    /// シード値を取得
    /// </summary>
    public string GetSeedValue()
    {
        var seedTypeSetting = GetSetting<EnumSetting>("SEED_TYPE");
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
            var seedSetting = GetSetting<TextInputSetting>("SEED_VALUE");
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