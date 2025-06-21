using UnityEngine;

/// <summary>
/// ゲーム設定管理サービス
/// PlayerPrefsを使用してゲーム全般の設定を管理します
/// </summary>
public class GameSettingsService : IGameSettingsService
{
    // PlayerPrefsキー定数
    private const string BGM_VOLUME_KEY = "BgmVolume";
    private const string SE_VOLUME_KEY = "SeVolume";
    private const string SEED_KEY = "Seed";
    private const string SEED_TEXT_KEY = "SeedText";
    private const string DOUBLE_SPEED_KEY = "IsDoubleSpeed";
    
    public GameSettingsService()
    {
        // 初期化時にデフォルト設定が存在しない場合は初期化
        if (!HasSettings())
        {
            InitializeDefaultSettings();
        }
    }
    
    /// <summary>
    /// 全設定をデフォルト値で初期化します
    /// </summary>
    public void InitializeDefaultSettings()
    {
        var defaultSettings = GameSettings.Default;
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, defaultSettings.audio.bgmVolume);
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, defaultSettings.audio.seVolume);
        SaveSeedSettings(defaultSettings.seed);
    }
    
    /// <summary>
    /// 音声設定をリセットします
    /// </summary>
    public void ResetAudioSettings()
    {
        var defaultAudio = AudioSettings.Default;
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, defaultAudio.bgmVolume);
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, defaultAudio.seVolume);
    }
    
    /// <summary>
    /// 音声設定を取得します
    /// </summary>
    /// <returns>現在の音声設定</returns>
    public AudioSettings GetAudioSettings()
    {
        return new AudioSettings
        {
            bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, AudioSettings.Default.bgmVolume),
            seVolume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, AudioSettings.Default.seVolume)
        };
    }
    
    /// <summary>
    /// BGM音量を保存します
    /// </summary>
    /// <param name="volume">BGM音量（0.0-1.0）</param>
    public void SaveBgmVolume(float volume)
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// SE音量を保存します
    /// </summary>
    /// <param name="volume">SE音量（0.0-1.0）</param>
    public void SaveSeVolume(float volume)
    {
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// シード設定を取得します
    /// </summary>
    /// <returns>現在のシード設定</returns>
    public SeedSettings GetSeedSettings()
    {
        return new SeedSettings
        {
            seed = PlayerPrefs.GetInt(SEED_KEY, SeedSettings.Default.seed),
            seedText = PlayerPrefs.GetString(SEED_TEXT_KEY, SeedSettings.Default.seedText)
        };
    }
    
    /// <summary>
    /// シードテキストからシード値を生成し保存します
    /// </summary>
    /// <param name="seedText">シードテキスト</param>
    /// <returns>生成されたシード値</returns>
    public int GenerateAndSaveSeed(string seedText)
    {
        var seed = string.IsNullOrEmpty(seedText) ? 0 : seedText.GetHashCode();
        SaveSeed(seed);
        SaveSeedText(seedText);
        return seed;
    }
    
    /// <summary>
    /// シードテキストを保存します
    /// </summary>
    /// <param name="seedText">保存するシードテキスト</param>
    public void SaveSeedText(string seedText)
    {
        PlayerPrefs.SetString(SEED_TEXT_KEY, seedText ?? "");
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// シード値を直接保存します
    /// </summary>
    /// <param name="seed">保存するシード値</param>
    public void SaveSeed(int seed)
    {
        PlayerPrefs.SetInt(SEED_KEY, seed);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 設定データが存在するかチェックします
    /// </summary>
    /// <returns>設定データが存在する場合true</returns>
    public bool HasSettings()
    {
        return PlayerPrefs.HasKey(BGM_VOLUME_KEY);
    }
    
    /// <summary>
    /// 倍速設定を取得します
    /// </summary>
    /// <returns>倍速が有効な場合true</returns>
    public bool IsDoubleSpeedEnabled()
    {
        return PlayerPrefs.GetInt(DOUBLE_SPEED_KEY, 0) == 1;
    }
    
    /// <summary>
    /// 倍速設定を保存します
    /// </summary>
    /// <param name="enabled">倍速を有効にする場合true</param>
    public void SaveDoubleSpeedEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(DOUBLE_SPEED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 倍速設定を切り替えます
    /// </summary>
    /// <returns>切り替え後の倍速設定</returns>
    public bool ToggleDoubleSpeed()
    {
        var newValue = !IsDoubleSpeedEnabled();
        SaveDoubleSpeedEnabled(newValue);
        return newValue;
    }
    
    /// <summary>
    /// シード設定を保存します（内部使用）
    /// </summary>
    /// <param name="settings">保存するシード設定</param>
    private void SaveSeedSettings(SeedSettings settings)
    {
        PlayerPrefs.SetInt(SEED_KEY, settings.seed);
        PlayerPrefs.SetString(SEED_TEXT_KEY, settings.seedText);
        PlayerPrefs.Save();
    }
}