using UnityEngine.SceneManagement;
using UnityEngine;
public enum GameMode {
    SINGLE, // 单人游戏
    DOUBLE, // 本地双人游戏
    LAN     // 局域网游戏
}
/// <summary>
/// 全局信息管理
/// </summary>
public class Global
{
    public static Global Instance { get; } = new Global();
    public GameMode SelectedGameMode { get; set; }
    public const string WelcomeScene = "Welcome";
    public const string GameScene = "Game";
    private Global() { }

    /// <summary>
    /// 退出游戏/editor运行模式
    /// </summary>
    public void Quit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    /// <summary>
    /// 当前是否为移动平台
    /// </summary>
    /// <returns>true说明在移动平台上(Android/iOS)</returns>
    public bool IsMobile() {
#if UNITY_IOS || UNITY_ANDROID
        return true;
#else
        return false;
#endif
    }
    public void EnterGame() {
        SceneManager.LoadScene(GameScene); // 这里很快, 直接同步加载了
    }
    public void BackToWelcome() {
        SceneManager.LoadScene(WelcomeScene);
    }
}
