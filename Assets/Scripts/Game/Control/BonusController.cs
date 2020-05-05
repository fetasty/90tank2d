using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BonusController : NetworkBehaviour
{
    private float stopWatchTimer;
    private void Start() {
        if (isServer) {
            Messager.Instance.Listen(MessageID.BONUS_STOP_WATCH_TRIGGER, OnMsgBonusStopWatch);
            Messager.Instance.Listen(MessageID.GAME_RETRY, OnMsgGameStart);
            Messager.Instance.Listen(MessageID.GAME_START, OnMsgGameStart);
            Messager.Instance.Listen(MessageID.DATA_LEVEL_WIN, OnMsgLevelWin);
        }
    }
    private void OnDestroy() {
        if (GameData.isHost) {
            Messager.Instance.CancelListen(MessageID.BONUS_STOP_WATCH_TRIGGER, OnMsgBonusStopWatch);
            Messager.Instance.CancelListen(MessageID.GAME_RETRY, OnMsgGameStart);
            Messager.Instance.CancelListen(MessageID.GAME_START, OnMsgGameStart);
            Messager.Instance.CancelListen(MessageID.DATA_LEVEL_WIN, OnMsgLevelWin);
        }
    }
    private void Update() {
        if (stopWatchTimer > 0f) {
            stopWatchTimer -= Time.deltaTime;
            if (stopWatchTimer <= 0f) {
                GameData.isStopWatchRunning = false;
            }
        }
    }
    private void OnMsgBonusStopWatch() {
        stopWatchTimer = GameData.bonusStopWatchTime;
        GameData.isStopWatchRunning = true;
    }
    private void OnMsgGameStart() {
        GameData.isStopWatchRunning = false;
        stopWatchTimer = 0f;
    }
    private void OnMsgLevelWin() {
        GameData.isStopWatchRunning = false;
        stopWatchTimer = 0f;
    }
}
