using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIController : NetworkBehaviour {
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

    private GameObject gameEnd;
    private enum ClickType { PAUSE, RESUME, RETRY, BACK, EXIT }
    private void Start() {
        // 移动设备输入界面初始化
        mobileInput.SetActive(GameData.isMobile);
        // 按钮监听
        resumeBtn.onClick.AddListener(() => OnClick(ClickType.RESUME));
        retryBtn.onClick.AddListener(() => OnClick(ClickType.RETRY));
        backBtn.onClick.AddListener(() => OnClick(ClickType.BACK));
        exitBtn.onClick.AddListener(() => OnClick(ClickType.EXIT));
        pauseBtn.onClick.AddListener(() => OnClick(ClickType.PAUSE));
        if (isServer) {
            // 事件监听
            Messager.Instance.Listen(MessageID.DATA_GAME_START, OnMsgGameStart);
            Messager.Instance.Listen(MessageID.ENEMY_SPAWN, OnMsgEnemySpawn);
            Messager.Instance.Listen(MessageID.ENEMY_DIE, OnMsgEnemyDie);
            Messager.Instance.Listen<int>(MessageID.PLAYER_SPAWN, OnMsgPlayerTankChange);
            Messager.Instance.Listen(MessageID.BONUS_TANK_TRIGGER, () => OnMsgPlayerTankChange(0));
            // Messager.Instance.Listen(MessageID.GAME_PAUSE, OnMsgGamePause);
            Messager.Instance.Listen(MessageID.GAME_OVER, OnMsgGameOver);
            Messager.Instance.Listen(MessageID.GAME_WIN, OnMsgGameWin);
            Messager.Instance.Listen(MessageID.GAME_RESUME, OnMsgGameResume);
            // Messager.Instance.Listen(MessageID.GAME_INFO_UPDATE, OnMsgInfoUpdate);
            Messager.Instance.Listen(MessageID.GAME_RETRY, OnMsgGameRetry);
        }
    }
    [ClientRpc]
    private void RpcSetUI(int enemyCount, int killedCount, int playerLife) {
        SetEnemyCount(enemyCount);
        SetKilledCount(killedCount);
        SetPlayerTankCount(playerLife);
    }
    private void SetEnemyCount(int count) {
        enemyCountText.text = $"剩余敌人: {count}";
    }
    private void SetKilledCount(int count) {
        killedCountText.text = $"击杀敌人: {count}";
    }
    private void SetPlayerTankCount(int count) {
        tankCountText.text = $"玩家生命: {count}";
    }
    private void UpdateOperationUI() {
        SetPauseMask(GameData.isGamePlaying && GameData.isGamePausing);
        SetResumeBtn(GameData.isGamePlaying && GameData.isGamePausing);
        SetOperations(GameData.isGamePausing || !GameData.isGamePlaying);
        SetPauseBtn(GameData.isGamePlaying && GameData.isGamePausing);
    }
    private void SetPauseMask(bool active) {
        pauseMask.SetActive(active);
    }
    private void SetOperations(bool active) {
        operations.SetActive(active);
    }
    private void SetPauseBtn(bool active) {
        if (GameData.isMobile) {
            pauseBtn.gameObject.SetActive(active);
        }
    }
    private void SetResumeBtn(bool active) {
        resumeBtn.gameObject.SetActive(active);
    }
    private void OnClick(ClickType type) {
        switch (type) {
            case ClickType.BACK:
                Global.EnterWelcome();
                break;
            case ClickType.EXIT:
                Global.Quit();
                break;
            case ClickType.RESUME:
                // GameController.Instance.PostMsg(new Msg(MsgID.GAME_RESUME, null));
                break;
            case ClickType.RETRY:
                // GameController.Instance.PostMsg(new Msg(MsgID.GAME_RETRY, null));
                break;
            case ClickType.PAUSE:
                // GameController.Instance.PostMsg(new Msg(MsgID.GAME_PAUSE, null));
                break;
            default:
                break;
        }
    }
    private void DestroyGameEnd() {
        if (gameEnd != null) {
            Destroy(gameEnd);   // gameEnd并不是一个网络组件
            gameEnd = null;
        }
    }
    [ServerCallback]
    private void OnMsgGameStart() {
        RpcGameStart();
        // 服务器的值设置到客户端UI
        RpcSetUI(GameData.LeftEnemyCount, GameData.killedEnemyCount, GameData.PlayerTankCount);
    }
    [ClientRpc]
    private void RpcGameStart() {
        if (isClientOnly) { // 不要改变host上的值, 因为那是服务器
            GameData.isGamePlaying = true;
            GameData.isGamePausing = false;
            UpdateOperationUI();
        }
    }
    [ServerCallback]
    private void OnMsgPlayerTankChange(int _) {
        RpcSetUI(GameData.LeftEnemyCount, GameData.killedEnemyCount, GameData.PlayerTankCount);
    }
    [ServerCallback]
    private void OnMsgEnemyDie() {
        RpcSetUI(GameData.LeftEnemyCount, GameData.killedEnemyCount, GameData.PlayerTankCount);
    }
    [ServerCallback]
    private void OnMsgEnemySpawn() {
        RpcSetUI(GameData.LeftEnemyCount, GameData.killedEnemyCount, GameData.PlayerTankCount);
    }
    private void OnMsgGameRetry() {
        DestroyGameEnd();
        UpdateOperationUI();
    }
    public void OnMsgGamePause() {
        UpdateOperationUI();
    }
    private void OnMsgGameResume() {
        UpdateOperationUI();
    }
    public void OnMsgGameWin() {
        gameEnd = Instantiate(GameWinPrefab, new Vector3(0f, 3f, 0f), Quaternion.identity);
        UpdateOperationUI();
    }
    public void OnMsgGameOver() {
        gameEnd = Instantiate(GameOverPrefab, new Vector3(0f, 3f, 0f), Quaternion.identity);
        UpdateOperationUI();
    }
}
