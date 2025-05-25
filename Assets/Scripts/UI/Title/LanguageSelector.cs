using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;

public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        // ドロップダウンに言語名を追加
        languageDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            options.Add(locale.Identifier.CultureInfo.NativeName); // ex: 日本語, English
        }

        languageDropdown.AddOptions(options);

        // 現在の言語を初期値に設定
        var currentLocale = LocalizationSettings.SelectedLocale;
        int index = LocalizationSettings.AvailableLocales.Locales.IndexOf(currentLocale);
        languageDropdown.value = index;

        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;
    }
}