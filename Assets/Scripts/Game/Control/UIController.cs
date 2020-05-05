using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIController : NetworkBehaviour {
    public Text enemyCountText;
    public Text killedCountText;
    public Text tankCountText;
    public Text gameLevelText;
    public GameObject levelWinPrefab;
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
    public GameObject levelTextPrefab;  // 第x关

    private GameObject gameEnd;
    private enum ClickType { PAUSE, RESUME, RETRY, BACK, EXIT }
    private void Start() {
        // 移动设备输入界面初始化
        mobileInput.SetActive(GameData.isMobile);
        // 不是主机不可以点击重新开始游戏
        retryBtn.gameObject.SetActive(GameData.isHost);
        // 隐藏关卡信息
        gameLevelText.gameObject.SetActive(false);
        // 按钮监听
        resumeBtn.onClick.AddListener(() => OnClick(ClickType.RESUME));
        retryBtn.onClick.AddListener(() => OnClick(ClickType.RETRY));
        backBtn.onClick.AddListener(() => OnClick(ClickType.BACK));
        exitBtn.onClick.AddListener(() => OnClick(ClickType.EXIT));
        pauseBtn.onClick.AddListener(() => OnClick(ClickType.PAUSE));
        if (isServer) {
            // 事件监听
            Messager.Instance.Listen(MessageID.DATA_GAME_START, OnMsgGameStart);
            Messager.Instance.Listen(MessageID.DATA_ENEMY_SPAWN, OnMsgEnemySpawn);
            Messager.Instance.Listen(MessageID.DATA_ENEMY_DIE, OnMsgEnemyDie);
            Messager.Instance.Listen<int>(MessageID.DATA_PLAYER_SPAWN, OnMsgPlayerTankChange);
            Messager.Instance.Listen(MessageID.DATA_BONUS_TANK, () => OnMsgPlayerTankChange(0));
            Messager.Instance.Listen(MessageID.GAME_OVER, OnMsgGameOver);
            Messager.Instance.Listen(MessageID.GAME_WIN, OnMsgGameWin);
            Messager.Instance.Listen(MessageID.GAME_RETRY, OnMsgGameRetry);
            Messager.Instance.Listen(MessageID.START_LEVEL, OnMsgStartLevel);
            Messager.Instance.Listen(MessageID.DATA_LEVEL_WIN, OnMsgLevelWin);
        }
        Messager.Instance.Listen(MessageID.GAME_RESUME, OnMsgGameResume);
        Messager.Instance.Listen(MessageID.GAME_PAUSE, OnMsgGamePause);
    }
    private void OnDestroy() {
        Messager.Instance.CancelListen(MessageID.GAME_RESUME, OnMsgGameResume);
        Messager.Instance.CancelListen(MessageID.GAME_PAUSE, OnMsgGamePause);
        if (GameData.isHost) {
            // 事件监听
            Messager.Instance.CancelListen(MessageID.DATA_GAME_START, OnMsgGameStart);
            Messager.Instance.CancelListen(MessageID.DATA_ENEMY_SPAWN, OnMsgEnemySpawn);
            Messager.Instance.CancelListen(MessageID.DATA_ENEMY_DIE, OnMsgEnemyDie);
            Messager.Instance.CancelListen<int>(MessageID.DATA_PLAYER_SPAWN, OnMsgPlayerTankChange);
            Messager.Instance.CancelListen(MessageID.DATA_BONUS_TANK, () => OnMsgPlayerTankChange(0));
            Messager.Instance.CancelListen(MessageID.GAME_OVER, OnMsgGameOver);
            Messager.Instance.CancelListen(MessageID.GAME_WIN, OnMsgGameWin);
            Messager.Instance.CancelListen(MessageID.GAME_RETRY, OnMsgGameRetry);
            Messager.Instance.CancelListen(MessageID.START_LEVEL, OnMsgStartLevel);
            Messager.Instance.CancelListen(MessageID.DATA_LEVEL_WIN, OnMsgLevelWin);
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
        SetPauseBtn(GameData.isGamePlaying && !GameData.isGamePausing);
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
                Messager.Instance.Send(MessageID.GAME_RESUME);
                break;
            case ClickType.RETRY:
                if (isServer) {
                    Messager.Instance.Send(MessageID.GAME_START);
                }
                break;
            case ClickType.PAUSE:
                Messager.Instance.Send(MessageID.GAME_PAUSE);
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
    }
    [ClientRpc]
    private void RpcGameStart() {
        DestroyGameEnd();
        if (isClientOnly) {
            GameData.isGamePlaying = true;
            GameData.isGamePausing = false;
        }
        UpdateOperationUI();
    }
    [ServerCallback]
    private void OnMsgStartLevel() {
        RpcStartLevel(GameData.gameLevel + 1, GameData.maxGameLevel + 1);
        // 服务器的值设置到客户端UI
        RpcSetUI(GameData.LeftEnemyCount, GameData.killedEnemyCount, GameData.PlayerTankCount);
    }
    [ClientRpc]
    private void RpcStartLevel(int level, int maxGameLevel) {
        GameObject obj = Instantiate(levelTextPrefab, GameObject.Find("/Canvas").transform);
        obj.GetComponent<Text>().text = $"第{level}关";
        Destroy(obj, 2f);
        gameLevelText.gameObject.SetActive(true);
        gameLevelText.text = $"关卡: {level}/{maxGameLevel}";
    }
    [ServerCallback]
    private void OnMsgLevelWin() {
        RpcLevelWin();
    }
    [ClientRpc]
    private void RpcLevelWin() {
        Destroy(Instantiate(levelWinPrefab, new Vector3(0f, 2.5f, 3f), Quaternion.identity), 3f);
        AudioController.Cur.PlayBack(BackAudio.LEVEL_WIN);
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
    [ServerCallback]
    public void OnMsgGameWin() {
        RpcGameWin();
    }
    [ClientRpc]
    private void RpcGameWin() {
        if (isClientOnly) { GameData.isGamePlaying = false; }
        AudioController.Cur.PlayBack(BackAudio.WIN);
        gameEnd = Instantiate(GameWinPrefab, new Vector3(0f, 3f, 2f), Quaternion.identity);
        UpdateOperationUI();
    }
    [ServerCallback]
    public void OnMsgGameOver() {
        RpcGameOver();
    }
    [ClientRpc]
    private void RpcGameOver() {
        if (isClientOnly) { GameData.isGamePlaying = false; }
        gameEnd = Instantiate(GameOverPrefab, new Vector3(0f, 3f, 2f), Quaternion.identity);
        AudioController.Cur.PlayBack(BackAudio.OVER);
        UpdateOperationUI();
    }
}
