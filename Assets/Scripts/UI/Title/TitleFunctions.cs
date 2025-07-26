using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleFunctions
{
    public static void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }
    public static void OpenTwitter()
    {
        Application.OpenURL("https://x.com/void2610");
    }

    public static void OpenSteam()
    {
        Application.OpenURL("https://store.steampowered.com/app/3646540/Merge_Rogue/?beta=1");
    }

    public static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}