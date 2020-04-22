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
    public Button pauseBtn;
    public GameObject GameOverPrefab;   // 游戏结束
    public GameObject GameWinPrefab;    // 游戏胜利

    private GameInfoManager info;
    private GameObject gameEnd;
    private enum ClickType { PAUSE, RESUME, RETRY, BACK, EXIT }
    private void Start() {
        info = GameController.Instance.InfoManager;
        GameController.Instance.AddListener(MsgID.ENEMY_SPAWN, OnMsgEnemySpawn);
        GameController.Instance.AddListener(MsgID.ENEMY_DIE, OnMsgEnemyDie);
        GameController.Instance.AddListener(MsgID.PLAYER_SPAWN, OnMsgPlayerTankChange);
        GameController.Instance.AddListener(MsgID.GAME_PAUSE, OnMsgGamePause);
        GameController.Instance.AddListener(MsgID.GAME_OVER, OnMsgGameOver);
        GameController.Instance.AddListener(MsgID.GAME_WIN, OnMsgGameWin);
        GameController.Instance.AddListener(MsgID.BONUS_TANK_TRIGGER, OnMsgPlayerTankChange);
        GameController.Instance.AddListener(MsgID.GAME_RESUME, OnMsgGameResume);
        GameController.Instance.AddListener(MsgID.GAME_INFO_UPDATE, OnMsgInfoUpdate);
        GameController.Instance.AddListener(MsgID.GAME_START, OnMsgGameStart);
        GameController.Instance.AddListener(MsgID.GAME_RETRY, OnMsgGameRetry);
        resumeBtn.onClick.AddListener(() => OnClick(ClickType.RESUME));
        retryBtn.onClick.AddListener(() => OnClick(ClickType.RETRY));
        backBtn.onClick.AddListener(() => OnClick(ClickType.BACK));
        exitBtn.onClick.AddListener(() => OnClick(ClickType.EXIT));
        pauseBtn.onClick.AddListener(() => OnClick(ClickType.PAUSE));
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
    private void UpdateOperationUI() {
        SetPauseMask(info.IsGamePlaying && info.IsGamePause);
        SetResumeBtn(info.IsGamePlaying && info.IsGamePause);
        SetOperations(info.IsGamePause || !info.IsGamePlaying);
        SetPauseBtn(info.IsGamePlaying && info.IsGamePause);
    }
    private void SetPauseMask(bool active) {
        pauseMask.SetActive(active);
    }
    private void SetOperations(bool active) {
        operations.SetActive(active);
    }
    private void SetPauseBtn(bool active) {
        if (Global.Instance.IsMobile) {
            pauseBtn.gameObject.SetActive(active);
        }
    }
    private void SetResumeBtn(bool active) {
        resumeBtn.gameObject.SetActive(active);
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
            case ClickType.PAUSE:
                GameController.Instance.PostMsg(new Msg(MsgID.GAME_PAUSE, null));
                break;
            default:
                break;
        }
    }
    private void DestroyGameEnd() {
        if (gameEnd != null) {
            Destroy(gameEnd);
            gameEnd = null;
        }
    }
    private void OnMsgInfoUpdate(Msg msg) {
        UpdateEnemyCount();
        UpdateKilledCount();
        UpdatePlayerTankCount();
    }
    private void OnMsgGameStart(Msg msg) {
        DestroyGameEnd();
        UpdateOperationUI();
    }
    private void OnMsgGameRetry(Msg msg) {
        DestroyGameEnd();
        UpdateOperationUI();
    }
    private void OnMsgPlayerTankChange(Msg msg) {
        UpdatePlayerTankCount();
    }
    private void OnMsgEnemyDie(Msg msg) {
        UpdateKilledCount();
    }
    private void OnMsgEnemySpawn(Msg msg) {
        UpdateEnemyCount();
    }
    public void OnMsgGamePause(Msg msg) {
        UpdateOperationUI();
    }
    private void OnMsgGameResume(Msg msg) {
        UpdateOperationUI();
    }
    public void OnMsgGameWin(Msg msg) {
        gameEnd = Instantiate(GameWinPrefab, new Vector3(0f, 3f, 0f), Quaternion.identity);
        UpdateOperationUI();
    }
    public void OnMsgGameOver(Msg msg) {
        gameEnd = Instantiate(GameOverPrefab, new Vector3(0f, 3f, 0f), Quaternion.identity);
        UpdateOperationUI();
    }
}
