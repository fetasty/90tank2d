using UnityEngine;
using Mirror;

/// <summary>
/// ServerOnly!!
/// 游戏逻辑
/// </summary>
[DisallowMultipleComponent]
public class GameController : NetworkBehaviour {
    private void Start() {
        if (isServer) {
            Messager.Instance.Listen(MessageID.DATA_GAME_START, OnMsgGameStart);
            Messager.Instance.Listen(MessageID.DATA_ENEMY_DIE, OnMsgEnemyDie);
            Messager.Instance.Listen<int>(MessageID.DATA_PLAYER_DIE, OnMsgPlayerDie);
            Messager.Instance.Listen(MessageID.HOME_DESTROY, OnMsgHomeDestroy);
            Messager.Instance.Listen(MessageID.DATA_LEVEL_WIN, OnMsgLevelWin);
            Messager.Instance.Listen(MessageID.DATA_START_LEVEL, OnMsgStartLevel);
            Messager.Instance.Listen(MessageID.ENEMY_CRIZY, OnMsgEnemyCrizy);
            AddDummyPlayers();
            Invoke(nameof(StartGame), 1f);
        }
    }
    private void OnDestroy() {
        if (GameData.isHost) {
            Messager.Instance.CancelListen(MessageID.DATA_GAME_START, OnMsgGameStart);
            Messager.Instance.CancelListen(MessageID.DATA_ENEMY_DIE, OnMsgEnemyDie);
            Messager.Instance.CancelListen<int>(MessageID.DATA_PLAYER_DIE, OnMsgPlayerDie);
            Messager.Instance.CancelListen(MessageID.HOME_DESTROY, OnMsgHomeDestroy);
            Messager.Instance.CancelListen(MessageID.DATA_LEVEL_WIN, OnMsgLevelWin);
            Messager.Instance.CancelListen(MessageID.DATA_START_LEVEL, OnMsgStartLevel);
            Messager.Instance.CancelListen(MessageID.ENEMY_CRIZY, OnMsgEnemyCrizy);
        }
    }
    /// <summary>
    /// 为每个用户添加一个"假"player, 让场景上的networkidentity都激活起来
    /// </summary>
    [ServerCallback]
    private void AddDummyPlayers() {
        foreach(NetworkConnection conn in GameData.networkPlayers) {
            AddDummyPlayerForConnection(conn);
        }
    }
    private void AddDummyPlayerForConnection(NetworkConnection conn) {
        GameObject dummy = new GameObject($"dummy-{conn.connectionId}");
        dummy.AddComponent<NetworkIdentity>();
        NetworkServer.ReplacePlayerForConnection(conn, dummy, true);
    }
    [ServerCallback]
    private void StartGame() {
        // 通知服务端的其它模块, 游戏开始
        Messager.Instance.Send(MessageID.GAME_START);
    }
    private void Update() {
        PauseUpdate();
    }
    private void OnApplicationFocus(bool focusStatus) {
        if (GameData.IsLan) { return; } // 多人游戏不可暂停
        if (!focusStatus && GameData.isGamePlaying && !GameData.isGamePausing) {
            Messager.Instance.Send(MessageID.GAME_PAUSE);
        }
    }
    private void PauseUpdate() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (GameData.isGamePlaying && !GameData.isGamePausing) {
                Messager.Instance.Send(MessageID.GAME_PAUSE);
            } else {
                GetComponent<EscQuit>().ClickQuit();
            }
        }
    }
    private void OnMsgGameStart() {
        // 开始新关卡
        StartLevel();
    }
    private void StartLevel() {
        // 通知GameData初始化关卡数据
        Messager.Instance.Send(MessageID.START_LEVEL);
        Invoke(nameof(LevelDataComplete), 2f);
    }
    private void LevelDataComplete() {
        // 代替发送Data消息
        Messager.Instance.Send(MessageID.DATA_START_LEVEL);
        GameData.isInGameLevel = true;
    }
    private void OnMsgStartLevel() {
        RpcPlayStartAudio();
    }
    [ClientRpc]
    private void RpcPlayStartAudio() {
        AudioController.Cur.PlayBack(BackAudio.START);
    }
    private void OnMsgEnemyCrizy() {
        RpcPlayerEnemyCrizyAudio();
    }
    [ClientRpc]
    private void RpcPlayerEnemyCrizyAudio() {
        AudioController.Cur.PlayEffect(EffectAudio.CRIZY);
    }
    private void OnMsgLevelWin() {
        Invoke(nameof(StartLevel), 4f); // 一段时间后开始新的关卡
    }
    private void OnMsgEnemyDie() {
        if (GameData.aliveEnemyCount <= 0 && !GameData.CanSpawnEnemy) {
            Messager.Instance.Send(MessageID.LEVEL_WIN); // 下一关?
        }
    }
    private void OnMsgPlayerDie(int _) {
        if (GameData.alivePlayerCount <= 0 && !GameData.CanSpawnPlayer) {
            Messager.Instance.Send(MessageID.GAME_OVER);
        }
    }
    private void OnMsgHomeDestroy() {
        Messager.Instance.Send(MessageID.GAME_OVER);
    }
}
