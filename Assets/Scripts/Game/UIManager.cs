using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public Text enemyCountText;
    public Text killedCountText;
    public Text tankCountText;
    public GameObject pauseMask;
    public GameObject operations;
    public GameObject mobileInput;
    public Button resumeBtn;
    public Button retryBtn;
    public Button backBtn;
    public Button exitBtn;
    public GameObject GameOverPrefab;   // 游戏结束
    public GameObject GameWinPrefab;    // 游戏胜利

    private GameInfoManager info;
    private GameObject gameEnd;
    private enum ClickType { RESUME, RETRY, BACK, EXIT }
    private void Start() {
        info = GameController.Instance.InfoManager;
        GameController.Instance.AddListener(MsgID.ENEMY_SPAWN, msg => UpdateEnemyCount());
        GameController.Instance.AddListener(MsgID.ENEMY_DIE, msg => UpdateKilledCount());
        GameController.Instance.AddListener(MsgID.PLAYER_SPAWN, msg => UpdatePlayerTankCount());
        GameController.Instance.AddListener(MsgID.GAME_START, msg => OnMsgGameStart());
        GameController.Instance.AddListener(MsgID.GAME_PAUSE, msg => OnMsgGamePause());
        GameController.Instance.AddListener(MsgID.GAME_OVER, msg => OnMsgGameOver());
        GameController.Instance.AddListener(MsgID.GAME_WIN, msg => OnMsgGameWin());
        GameController.Instance.AddListener(MsgID.BONUS_TANK_TRIGGER, msg => UpdatePlayerTankCount());
        resumeBtn.onClick.AddListener(() => OnClick(ClickType.RESUME));
        retryBtn.onClick.AddListener(() => OnClick(ClickType.RETRY));
        backBtn.onClick.AddListener(() => OnClick(ClickType.BACK));
        exitBtn.onClick.AddListener(() => OnClick(ClickType.EXIT));
        mobileInput.SetActive(Global.Instance.IsMobile); // 移动段的输入
    }
    private void UpdateEnemyCount() {
        enemyCountText.text = $"剩余敌人: {info.LeftEnemyCount}";
    }
    private void UpdateKilledCount() {
        killedCountText.text = $"击杀数量: {info.KilledEnemyCount}";
    }
    private void UpdatePlayerTankCount() {
        tankCountText.text = $"玩家坦克: {info.PlayerTankCount - info.SpawnedPlayerCount}";
    }
    private void SetPauseMask(bool active) {
        pauseMask.SetActive(active);
    }
    private void SetOperations(bool active) {
        operations.SetActive(active);
    }
    private void InitialUI() {
        UpdateEnemyCount();
        UpdateKilledCount();
        UpdatePlayerTankCount();
        SetPauseMask(false);
        SetOperations(false);
    }
    private void OnClick(ClickType type) {
        switch (type) {
            case ClickType.BACK:
                Global.Instance.BackToWelcome();
                break;
            case ClickType.EXIT:
                Global.Instance.Quit();
                break;
            case ClickType.RESUME:
                GameController.Instance.PostMsg(new Msg(MsgID.GAME_RESUME, null));
                break;
            case ClickType.RETRY:
                GameController.Instance.PostMsg(new Msg(MsgID.GAME_RETRY, null));
                break;
            default:
                break;
        }
    }
    public void OnMsgGamePause() {
        SetPauseMask(true);
        SetOperations(true);
        resumeBtn.gameObject.SetActive(true);
    }
    public void OnMsgGameStart() {
        InitialUI();
    }
    public void OnMsgGameWin() {
        gameEnd = Instantiate(GameWinPrefab, Vector3.zero, Quaternion.identity);
        OnMsgGameEnd();
    }
    public void OnMsgGameOver() {
        gameEnd = Instantiate(GameOverPrefab, Vector3.zero, Quaternion.identity);
        OnMsgGameEnd();
    }
    public void OnMsgGameEnd() {
        SetPauseMask(false);
        SetOperations(true);
        resumeBtn.gameObject.SetActive(false);
    }
}
