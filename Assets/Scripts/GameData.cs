
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
public enum TankMode {
    SINGLE, // 单人游戏
    DOUBLE, // 本地双人游戏
    LAN     // 局域网游戏
}
public static class GameData {
    // 全局数据
    public const string welcomeScene = "Welcome";
    public const string gameScene = "Game";
    public const string roomOfflineScene = "RoomOffline";
    public const string roomOnlineScene = "RoomOnline";
    public static readonly bool isMobile;
    public static string currentScene;
    public static TankMode mode;

    // 配置数据
    // 敌人
    public static int configTotalEnemyCount = 40;
    public static int configMaxAliveEnemyCount = 6;
    public static float configMinEnemySpawnTime = 0.5f;
    public static float configMaxEnemySpawnTime = 3f;
    public static float configEnemySpawnPointWaitTime = 1.5f;
    public static int[] configEnemyTypeWeights = { 40, 30, 10, 10, 10 }; // 100
    public static int configEnemyBonusWeight = 20; // 100
    // 玩家
    public static int configInitialPlayerTankCount = 3; // 初始生命条数
    // 道具
    public static float configBonusStopWatchTime = 10f;   // 秒表时长
    public static float configBonusShieldTime = 15f;      // 护盾时长
    public static float configBonusShovelTime = 40f;      // 铁锹时长


    // 使用数据
    // 难度系数
    public static int difficulty = 0;
    // 敌人
    public static int totalEnemyCount;
    public static int maxAliveEnemyCount;
    public static float minEnemySpawnTime;
    public static float maxEnemySpawnTime;
    public static float enemySpawnPointWaitTime;
    public static int[] enemyTypeWeights = { 40, 30, 10, 10, 10 }; // 100
    public static int enemyBonusWeight; // 100
    // 玩家
    public static int initialPlayerTankCount; // 初始生命条数
    // 道具
    public static float bonusStopWatchTime;   // 秒表时长
    public static float bonusShieldTime;      // 护盾时长
    public static float bonusShovelTime;      // 铁锹时长

    // 动态数据
    public static bool isGameStarted;               // 进入游戏场景后是否开始游戏 (只会设置一次)
    public static bool isGamePlaying;
    public static bool isGamePausing;
    public static int spawnedEnemyCount;            // 已经生成的敌人数量
    public static int killedEnemyCount;             // 已经击杀的敌人数量
    public static int aliveEnemyCount;              // 活着的敌人数量, 包括出生中
    public static int alivePlayerCount;             // 活着的玩家数量, 包括出生中
    public static int totalPlayerLifeCount;              // 玩家生命条数
    public static int spawnedPlayerCount;           // 已经生成的玩家数量

    // // 本地客户端数据, 与服务器不相干
    // public static bool isLocalGamePlaying;          // 本地维护的游戏进行状态
    // public static bool isLocalGamePausing;          // 本地维护的游戏暂停状态

    // 缓存的用户数据
    public const string PLAYER_NAME_KEY = "PlayerName";
    public const string SKIP_TUTORIAL_KEY = "SkipTutorial";
    public static string playerName;                // 用户自己输入的名字
    public static bool isSkipTutorial;              // 是否选择过跳过教程

    // 多人游戏数据
    public static bool isHost;                      // 是否作为主机
    // playerID 与 connection映射, 服务端保存
    public static List<NetworkConnection> networkPlayers = new List<NetworkConnection>();

    // 提供的数据
    public static float EnemySpawnTime { get{ return UnityEngine.Random.Range(minEnemySpawnTime, maxEnemySpawnTime); }}
    public static bool CanSpawnEnemy { get { return spawnedEnemyCount < totalEnemyCount && aliveEnemyCount < maxAliveEnemyCount; }}
    public static int EnemyType {
        get {
            int num = UnityEngine.Random.Range(0, 100);
            for (int i = 0; i < enemyTypeWeights.Length; ++i) {
                if (num < enemyTypeWeights[i]) { return i; }
                num -= enemyTypeWeights[i];
            }
            return 0;
        }
    }
    public static int LeftEnemyCount { get { return totalEnemyCount - spawnedEnemyCount + aliveEnemyCount; }}
    public static bool EnemyBonus { get { return UnityEngine.Random.Range(0, 100) < enemyBonusWeight; }}
    public static bool CanSpawnPlayer { get { return spawnedPlayerCount < totalPlayerLifeCount; }}
    public static int PlayerTankCount { get { return totalPlayerLifeCount - spawnedPlayerCount; }}
    static GameData() {
        // 平台信息
        #if UNITY_IOS || UNITY_ANDROID
            isMobile = true;
        #else
            isMobile = false;
        #endif
        // 当前场景
        currentScene = welcomeScene;
        // 事件监听
        Messager.Instance.Listen(MessageID.GAME_START, OnGameStart);
        Messager.Instance.Listen(MessageID.ENEMY_SPAWN, OnEnemySpawn);
        Messager.Instance.Listen(MessageID.ENEMY_DIE, OnMsgEnemyDie);
        Messager.Instance.Listen<int>(MessageID.PLAYER_SPAWN, OnMsgPlayerSpawn);
        Messager.Instance.Listen<int>(MessageID.PLAYER_DIE, OnMsgPlayerDie);
        Messager.Instance.Listen(MessageID.BONUS_TANK_TRIGGER, OnMsgBonusTank);
        // Messager.Instance.Listen(MessageID.BONUS_STOP_WATCH_TRIGGER, OnMsgBonusPause); // 给bonus模块处理, 以GameData为中心
        // 读取缓存数据
        ReadPrefDatas();
    }
    /// <summary>
    /// 将难度数据根据玩家人数配置
    /// </summary>
    public static void SetDifficulty() {
        // 读取玩家数量, 设定难度系数
        difficulty = networkPlayers.Count; // todo 敌人根据该系数 暴走一下? 剩余<=difficulty, 生成暴走
        // 敌人
        totalEnemyCount = configTotalEnemyCount + (difficulty - 1) * 30;
        maxAliveEnemyCount = configMaxAliveEnemyCount + (difficulty - 1) * 2;
        minEnemySpawnTime = configMinEnemySpawnTime;
        maxEnemySpawnTime = configMaxEnemySpawnTime - (difficulty - 1) * 0.5f;
        enemySpawnPointWaitTime = configEnemySpawnPointWaitTime - (difficulty - 1) * 0.25f;
        for (int i = 0; i < enemyTypeWeights.Length; ++i) {
            enemyTypeWeights[i] = configEnemyTypeWeights[i];
        }
        enemyBonusWeight = configEnemyBonusWeight; // 100
        // 玩家
        initialPlayerTankCount = configInitialPlayerTankCount + (difficulty - 1) * 3; // 初始生命条数
        totalPlayerLifeCount = initialPlayerTankCount;   // 如果做关卡, 可能有用
        // 道具
        bonusStopWatchTime = configBonusStopWatchTime;   // 秒表时长
        bonusShieldTime = configBonusShieldTime;      // 护盾时长
        bonusShovelTime = configBonusShovelTime;      // 铁锹时长
    }
    public static void ReadPrefDatas() {
        playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, SystemInfo.deviceName);
        isSkipTutorial = PlayerPrefs.GetInt(SKIP_TUTORIAL_KEY, 0) > 0;
    }
    public static void ClearPrefDatas() {
        PlayerPrefs.DeleteAll();
    }
    private static void OnGameStart() {
        // 难度设置
        SetDifficulty();
        isGamePlaying = true;
        // 通知数据变动
        Messager.Instance.Send(MessageID.DATA_GAME_START);
    }
    private static void OnEnemySpawn() {
        ++spawnedEnemyCount;
        ++aliveEnemyCount;
        Messager.Instance.Send(MessageID.DATA_ENEMY_SPAWN);
    }
    private static void OnMsgEnemyDie() {
        --aliveEnemyCount;
        ++killedEnemyCount;
        Messager.Instance.Send(MessageID.DATA_ENEMY_DIE);
    }
    private static void OnMsgPlayerSpawn(int id) {
        ++spawnedPlayerCount;
        ++alivePlayerCount;
        Messager.Instance.Send<int>(MessageID.DATA_PLAYER_SPAWN, id);
    }
    private static void OnMsgPlayerDie(int id) {
        --alivePlayerCount;
        Messager.Instance.Send<int>(MessageID.DATA_PLAYER_DIE, id);
    }
    private static void OnMsgBonusTank() {
        ++totalPlayerLifeCount;
        Messager.Instance.Send(MessageID.DATA_BONUS_TANK);
    }
}
