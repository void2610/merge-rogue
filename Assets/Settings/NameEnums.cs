using System;

namespace uPalette.Generated
{
    public enum ColorTheme
    {
        Default,
    }

    public static class ColorThemeExtensions
    {
        public static string ToThemeId(this ColorTheme theme)
        {
            switch (theme)
            {
                case ColorTheme.Default:
                    return "4cdfc659-724c-45a6-8a1a-6cef44e9252c";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum ColorEntry
    {
        NormalText,
        BackGround,
        Display,
        Accent,
        Button,
        WindowBackGround,
        SubText,
    }

    public static class ColorEntryExtensions
    {
        public static string ToEntryId(this ColorEntry entry)
        {
            switch (entry)
            {
                case ColorEntry.NormalText:
                    return "af515ef3-f896-4abf-80b8-0ab3bc46b877";
                case ColorEntry.BackGround:
                    return "08bdf8f5-274e-41e3-9fb0-10254636975e";
                case ColorEntry.Display:
                    return "b64c48df-8757-4910-a698-0f474f0b13e1";
                case ColorEntry.Accent:
                    return "c2f11f32-6b76-49f4-a9cf-916e11b26cc6";
                case ColorEntry.Button:
                    return "a4a97bfe-89a3-46d8-86dd-bd692b94adb3";
                case ColorEntry.WindowBackGround:
                    return "8ab899da-40ec-48f6-acd4-8f66a0079bc6";
                case ColorEntry.SubText:
                    return "2e8f84c6-5cff-4f73-8793-b449a0124623";
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum GradientTheme
    {
        Default,
    }

    public static class GradientThemeExtensions
    {
        public static string ToThemeId(this GradientTheme theme)
        {
            switch (theme)
            {
                case GradientTheme.Default:
                    return "69268790-5155-414d-889f-c4a30dbfa7f5";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum GradientEntry
    {
    }

    public static class GradientEntryExtensions
    {
        public static string ToEntryId(this GradientEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTheme
    {
        Default,
    }

    public static class CharacterStyleThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTheme.Default:
                    return "73deca3f-7928-4ef8-bf95-2f008dd1fc17";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleEntry
    {
    }

    public static class CharacterStyleEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTMPTheme
    {
        Default,
    }

    public static class CharacterStyleTMPThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTMPTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTMPTheme.Default:
                    return "84d248ad-8382-4024-b81b-b2029ecc42b6";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleTMPEntry
    {
    }

    public static class CharacterStyleTMPEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleTMPEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }
}
