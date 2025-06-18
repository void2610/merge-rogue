using System;
using UnityEngine.Serialization;

/// <summary>
/// 音声設定データ
/// </summary>
[Serializable]
public struct AudioSettings
{
    public float bgmVolume;
    public float seVolume;
    
    public static AudioSettings Default => new AudioSettings
    {
        bgmVolume = 0.5f,
        seVolume = 0.5f
    };
}

/// <summary>
/// シード設定データ
/// </summary>
[Serializable]
public struct SeedSettings
{
    public int seed;
    public string seedText;
    
    public static SeedSettings Default => new SeedSettings
    {
        seed = 0,
        seedText = ""
    };
}

/// <summary>
/// ゲーム全般設定データ
/// </summary>
[Serializable]
public struct GameSettings
{
    public AudioSettings audio;
    public SeedSettings seed;
    
    public static GameSettings Default => new GameSettings
    {
        audio = AudioSettings.Default,
        seed = SeedSettings.Default
    };
}