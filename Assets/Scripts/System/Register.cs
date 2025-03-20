using System.Collections.Generic;
using JetBrains.Annotations;

public static class Register
{
    private static Dictionary<string, int> _intDictionary = new();
    private static Dictionary<string, float> _floatDictionary = new();
    private static Dictionary<string, string> _stringDictionary = new();
    
    public static void RegisterInt(string key, int value) => _intDictionary[key] = value;
    public static void RegisterFloat(string key, float value) => _floatDictionary[key] = value;
    public static void RegisterString(string key, string value) => _stringDictionary[key] = value;
    
    public static int? GetInt(string key) => _intDictionary.ContainsKey(key) ? _intDictionary[key] : (int?)null;
    public static float? GetFloat(string key) => _floatDictionary.ContainsKey(key) ? _floatDictionary[key] : (float?)null;
    [CanBeNull] public static string GetString(string key) => _stringDictionary.ContainsKey(key) ? _stringDictionary[key] : null;
    
    public static bool TryGetInt(string key, out int value) => _intDictionary.TryGetValue(key, out value);
    public static bool TryGetFloat(string key, out float value) => _floatDictionary.TryGetValue(key, out value);
    public static bool TryGetString(string key, out string value) => _stringDictionary.TryGetValue(key, out value);
    
    public static bool ContainsInt(string key) => _intDictionary.ContainsKey(key);
    public static bool ContainsFloat(string key) => _floatDictionary.ContainsKey(key);
    public static bool ContainsString(string key) => _stringDictionary.ContainsKey(key);
    
    public static void RemoveInt(string key) => _intDictionary.Remove(key);
    public static void RemoveFloat(string key) => _floatDictionary.Remove(key);
    public static void RemoveString(string key) => _stringDictionary.Remove(key);

    public static void Clear()
    {
        _intDictionary.Clear();
        _floatDictionary.Clear();
        _stringDictionary.Clear();
    }
}
