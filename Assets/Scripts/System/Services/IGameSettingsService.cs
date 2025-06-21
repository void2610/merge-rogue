/// <summary>
/// ゲーム設定管理サービスのインターフェース
/// </summary>
public interface IGameSettingsService
{
    /// <summary>
    /// 全設定をデフォルト値で初期化します
    /// </summary>
    void InitializeDefaultSettings();
    
    /// <summary>
    /// 音声設定をリセットします
    /// </summary>
    void ResetAudioSettings();
    
    /// <summary>
    /// 音声設定を取得します
    /// </summary>
    /// <returns>現在の音声設定</returns>
    AudioSettings GetAudioSettings();
    
    /// <summary>
    /// BGM音量を保存します
    /// </summary>
    /// <param name="volume">BGM音量（0.0-1.0）</param>
    void SaveBgmVolume(float volume);
    
    /// <summary>
    /// SE音量を保存します
    /// </summary>
    /// <param name="volume">SE音量（0.0-1.0）</param>
    void SaveSeVolume(float volume);
    
    /// <summary>
    /// シード設定を取得します
    /// </summary>
    /// <returns>現在のシード設定</returns>
    SeedSettings GetSeedSettings();
    
    /// <summary>
    /// シードテキストからシード値を生成し保存します
    /// </summary>
    /// <param name="seedText">シードテキスト</param>
    /// <returns>生成されたシード値</returns>
    int GenerateAndSaveSeed(string seedText);
    
    /// <summary>
    /// シードテキストを保存します
    /// </summary>
    /// <param name="seedText">保存するシードテキスト</param>
    void SaveSeedText(string seedText);
    
    /// <summary>
    /// シード値を直接保存します
    /// </summary>
    /// <param name="seed">保存するシード値</param>
    void SaveSeed(int seed);
    
    /// <summary>
    /// 設定データが存在するかチェックします
    /// </summary>
    /// <returns>設定データが存在する場合true</returns>
    bool HasSettings();
    
    /// <summary>
    /// 倍速設定を取得します
    /// </summary>
    /// <returns>倍速が有効な場合true</returns>
    bool IsDoubleSpeedEnabled();
    
    /// <summary>
    /// 倍速設定を保存します
    /// </summary>
    /// <param name="enabled">倍速を有効にする場合true</param>
    void SaveDoubleSpeedEnabled(bool enabled);
    
    /// <summary>
    /// 倍速設定を切り替えます
    /// </summary>
    /// <returns>切り替え後の倍速設定</returns>
    bool ToggleDoubleSpeed();
}