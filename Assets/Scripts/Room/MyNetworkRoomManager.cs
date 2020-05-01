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
        base.OnClientConnect(conn);
        if (!IsSceneActive(gameScene)) {
            ClientScene.AddPlayer(conn);
        }
    }
    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        if (onClientDisconnect != null) {
            onClientDisconnect(conn);
        }
    }
}
