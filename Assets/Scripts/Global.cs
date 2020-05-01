using UnityEngine.SceneManagement;
using UnityEngine;
using Mirror;
/// <summary>
/// 场景切换与退出
/// </summary>
public class Global
{
    private Global() { }
    /// <summary>
    /// 退出游戏/editor运行模式
    /// </summary>
    public static void Quit() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public static void EnterGame() {
        NetworkManager.singleton.ServerChangeScene(GameData.gameScene);
        GameData.currentScene = GameData.gameScene; // manager.networkSceneName
    }
    public static void EnterRoomOffline() {
        SceneManager.LoadScene(GameData.roomOfflineScene);
        GameData.currentScene = GameData.roomOfflineScene;
    }
    public static void EnterRoomOnline() {
        SceneManager.LoadScene(GameData.roomOnlineScene);
        GameData.currentScene = GameData.roomOnlineScene;
    }
    public static void EnterWelcome() {
        SceneManager.LoadScene(GameData.welcomeScene);
        GameData.currentScene = GameData.welcomeScene;
    }
}
