using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MyNetworkRoomManager : NetworkManager
{
    [Header("Room Manager")]
    public GameObject roomPlayerPrefab;
    [Scene]
    public string gameScene;
    public int minPlayer = 2;
    public Action<NetworkConnection> onCreateRoomPlayer;
    public Action<NetworkConnection> onClientDisconnect;
    public Action<bool> onGameStartStateChange;
    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);
        if (GameData.mode == TankMode.SINGLE) {
            GameData.networkPlayers.Add(conn);
        } else if (GameData.mode == TankMode.DOUBLE) {
            // 两个本地用户
            GameData.networkPlayers.Add(conn);
            GameData.networkPlayers.Add(conn);
        }
    }
    public override void OnServerAddPlayer(NetworkConnection conn) {
        if (IsSceneActive(gameScene)) {
            // todo nothing
            // base.OnServerAddPlayer(conn);
        } else {
            onCreateRoomPlayer(conn);
            GameData.networkPlayers.Add(conn);
            if (onGameStartStateChange != null) {
                onGameStartStateChange(numPlayers >= minPlayer);
            }
        }
    }
    public override void OnServerDisconnect(NetworkConnection conn) {
        if (!IsSceneActive(gameScene)) {
            for (int i = GameData.networkPlayers.Count - 1; i >= 0; --i) {
                if (GameData.networkPlayers[i].connectionId == conn.connectionId) {
                    GameData.networkPlayers.RemoveAt(i);
                }
            }
            if (onGameStartStateChange != null) {
                onGameStartStateChange(numPlayers >= minPlayer);
            }
        }
        base.OnServerDisconnect(conn);
    }
    // public override void OnServerSceneChanged(string sceneName) {
    //     if (sceneName == gameScene) {

    //     }
    // }
    public override void OnStartClient()
    {
        base.OnStartClient();
        ClientScene.RegisterPrefab(roomPlayerPrefab.gameObject);
    }
    public override void OnClientConnect(NetworkConnection conn) {
        // 游戏中不允许加入
        if (IsSceneActive(gameScene)) {
            conn.Disconnect();
            return;
        }
        base.OnClientConnect(conn);
        if (GameData.IsLan) {
            ClientScene.AddPlayer(conn); // 自动创建roomPlayer
        }
    }
    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        if (onClientDisconnect != null) {
            onClientDisconnect(conn);
        }
    }
}
