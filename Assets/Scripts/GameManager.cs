using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameMode {
        Single,
        Double
    }
    public GameMode CurrentMode;
    private static GameManager gameManager;
    private string nextScene;
    public string NextScene {
        get {
            return nextScene;
        }
    }

    public static GameManager Instance {
        get {
            return gameManager;
        }
    }
    private GameManager() {}
    public void LoadSceneAsync(string name) {
        nextScene = name;
        SceneManager.LoadScene("Loading");
    }

    public void EnterGame(GameMode mode) {
        CurrentMode = mode;
        switch (mode) {
            case GameMode.Single:
                LoadSceneAsync("Game");
                break;
            case GameMode.Double:
                // todo LoadSceneAsync("GameRoom"); // 多人联机房间场景
                break;
        }
    }
    private void Awake() {
        gameManager = this;
    }
    private void Start() {
        Screen.fullScreen = false;
    }
}
