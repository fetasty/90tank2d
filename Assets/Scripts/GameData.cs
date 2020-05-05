
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
    public static int configTotalEnemyCount = 10;
    public static int configMaxAliveEnemyCount = 6;
    public static float configMinEnemySpawnTime = 0.5f;
    public static float configMaxEnemySpawnTime = 6f;
    public static float configEnemySpawnPointWaitTime = 3f;
    public static int[] configEnemyTypeWeights = { 40, 30, 10, 10, 10 }; // 100
    public static int configEnemyBonusWeight = 15; // 100
    public static float configEnemyMoveSpeed = 2f;
    public static float configMinEnemyFireTime = 0.5f;
    public static float configMaxEnemyFireTime = 6f;
    public static float configMinEnemyMoveTime = 0.5f;
    public static float configMaxEnemyMoveTime = 6f;
    // 玩家
    public static int configInitialPlayerTankCount = 3; // 初始生命条数
    // 道具
    public static float configBonusStopWatchTime = 10f;   // 秒表时长
    public static float configBonusShieldTime = 15f;      // 护盾时长
    public static float configBonusShovelTime = 40f;      // 铁锹时长
    public static int configMaxGameLevel = 5;               // 从0计算的最大关卡数


    // 使用数据
    // 难度系数
    public static int playerCount = 0;
    // 敌人
    public static int totalEnemyCount;
    public static int maxAliveEnemyCount;
    public static float minEnemySpawnTime;
    public static float maxEnemySpawnTime;
    public static float minEnemyFireTime;
    public static float maxEnemyFireTime;
    public static float minEnemyMoveTime;
    public static float maxEnemyMoveTime;
    public static float enemySpawnPointWaitTime;
    public static int[] enemyTypeWeights = { 40, 30, 10, 10, 10 }; // 100
    public static int enemyBonusWeight; // 100
    public static bool enemyCrizy;      // 是否生成疯狂的敌人
    // 玩家
    public static int initialPlayerTankCount; // 初始生命条数
    // 道具
    public static float bonusStopWatchTime;   // 秒表时长
    public static float bonusShieldTime;      // 护盾时长
    public static float bonusShovelTime;      // 铁锹时长

    // 动态数据
    public static bool isGamePlaying;
    public static bool isGamePausing;
    public static bool isInGameLevel;             // 控制敌人是否刷新
    public static int spawnedEnemyCount;            // 已经生成的敌人数量
    public static int killedEnemyCount;             // 已经击杀的敌人数量
    public static int aliveEnemyCount;              // 活着的敌人数量, 包括出生中
    public static int alivePlayerCount;             // 活着的玩家数量, 包括出生中
    public static int playerLifeCount;         // 玩家生命条数
    public static bool isStopWatchRunning;          // 是否秒表时间里
    public static int[] playerLevels = new int[] {0, 0, 0, 0}; // 每个玩家的等级 (关卡衔接用)
    // 关卡
    public static int maxGameLevel;                 // 最大关卡
    public static int gameLevel;                    // 游戏关卡数

    // // 本地客户端数据, 与服务器不相干
    // public static bool isLocalGamePlaying;          // 本地维护的游戏进行状态
    // public static bool isLocalGamePausing;          // 本地维护的游戏暂停状态

    // 缓存的用户数据
    public const string PLAYER_NAME_KEY = "PlayerName";
    public const string SKIP_TUTORIAL_KEY = "SkipTutorial";
    public const string MAIN_VOLUME_KEY = "MainVolume";
    public const string BACK_VOLUME_KEY = "BackVolume";
    public const string EFFECT_VOLUME_KEY = "EffectVolume";
    public const string ENGINE_VOLUME_KEY = "EngineVolume";
    public static string playerName;                // 用户自己输入的名字
    public static bool isSkipTutorial;              // 是否选择过跳过教程

    // 多人游戏数据
    public static bool isHost;                      // 是否作为主机
    // playerID 与 connection映射, 服务端保存
    public static List<NetworkConnection> networkPlayers = new List<NetworkConnection>();

    // 提供的数据
    public static float EnemySpawnTime { get{
        if (enemyCrizy) { return minEnemySpawnTime; }
        return UnityEngine.Random.Range(minEnemySpawnTime, maxEnemySpawnTime);
    }}
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
    public static float EnemySpeed { get {
        if (enemyCrizy) { return configEnemyMoveSpeed * 3f; }
        return configEnemyMoveSpeed;
    }}
    public static float EnemyFireTime { get {
        if (enemyCrizy) { return UnityEngine.Random.Range(minEnemyFireTime, minEnemyFireTime + 0.5f); }
        return UnityEngine.Random.Range(minEnemyFireTime, maxEnemyFireTime);
    }}
    public static float EnemyMoveTime { get {
        if (enemyCrizy) { return UnityEngine.Random.Range(0.2f, minEnemyMoveTime); }
        return UnityEngine.Random.Range(minEnemyMoveTime, maxEnemyMoveTime);
    }}
    public static int LeftEnemyCount { get { return totalEnemyCount - spawnedEnemyCount + aliveEnemyCount; }}
    public static bool EnemyBonus { get { return UnityEngine.Random.Range(0, 100) < enemyBonusWeight; }}
    public static bool CanSpawnPlayer { get { return playerLifeCount > 0; }}
    public static int PlayerTankCount { get { return playerLifeCount; }}
    public static bool IsLan { get { return mode == TankMode.LAN; } }
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
        Messager.Instance.Listen<int, bool>(MessageID.PLAYER_SPAWN, OnMsgPlayerSpawn);
        Messager.Instance.Listen<int>(MessageID.PLAYER_DIE, OnMsgPlayerDie);
        Messager.Instance.Listen(MessageID.BONUS_TANK_TRIGGER, OnMsgBonusTank);
        Messager.Instance.Listen(MessageID.GAME_WIN, OnMsgGameWin);
        Messager.Instance.Listen(MessageID.GAME_OVER, OnMsgGameOver);
        Messager.Instance.Listen(MessageID.GAME_PAUSE, OnMsgGamePause);
        Messager.Instance.Listen(MessageID.GAME_RESUME, OnMsgGameResume);
        Messager.Instance.Listen(MessageID.START_LEVEL, OnMsgStartLevel);
        Messager.Instance.Listen(MessageID.LEVEL_WIN, OnMsgLevelWin);
        // 读取缓存数据
        ReadCache();
    }
    public static void ReadCache() {
        playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, SystemInfo.deviceName);
        isSkipTutorial = PlayerPrefs.GetInt(SKIP_TUTORIAL_KEY, 0) > 0;
    }
    public static void ClearPrefDatas() {
        PlayerPrefs.DeleteAll();
    }
    private static void OnGameStart() {
        // 游戏开始, 初始化数据
        GameStartInitial();
        // 通知数据变动
        Messager.Instance.Send(MessageID.DATA_GAME_START);
    }
    private static void GameStartInitial() {
        // 游戏第一次开始, 关卡为0
        gameLevel = 0;
        maxGameLevel = configMaxGameLevel;
        // 敌人类型权重
        for (int i = 0; i < enemyTypeWeights.Length; ++i) {
            enemyTypeWeights[i] = configEnemyTypeWeights[i];
        }
        enemyBonusWeight = configEnemyBonusWeight; // 100
        // 玩家
        playerCount = networkPlayers.Count;
        initialPlayerTankCount = playerCount * 3; // 初始生命条数
        playerLifeCount = initialPlayerTankCount;   // 过关时不清空
        for (int i = 0; i < playerLevels.Length; ++i) { playerLevels[i] = 0; }
        // 道具
        bonusStopWatchTime = configBonusStopWatchTime;   // 秒表时长
        bonusShieldTime = configBonusShieldTime;      // 护盾时长
        bonusShovelTime = configBonusShovelTime;      // 铁锹时长
        // 动态数据
        spawnedEnemyCount = 0;
        aliveEnemyCount = 0;
        alivePlayerCount = 0;
        // 游戏状态
        isGamePlaying = true;
        isGamePausing = false;
        isInGameLevel = false;
    }
    private static void OnMsgStartLevel() {
        GameLevelDataInitial();
    }
    private static void OnMsgLevelWin() {
        isInGameLevel = false;
        Player[] players = GameObject.FindObjectsOfType<Player>();
        for (int i = 0; i < playerLevels.Length; ++i) { playerLevels[i] = 0; }
        foreach(Player p in players) {
            Debug.Log($"GameData level win: id[{p.id}]-level[{p.level}]");
            playerLevels[p.id] = p.level;
        }
        if (gameLevel < maxGameLevel) {
            ++gameLevel;
            Messager.Instance.Send(MessageID.DATA_LEVEL_WIN);
        } else {
            Messager.Instance.Send(MessageID.GAME_WIN);
        }
    }
    /// <summary>
    /// 开始新关卡前调用
    /// </summary>
    private static void SetLevelDifficulty() {
        // 敌人
        totalEnemyCount = configTotalEnemyCount + playerCount * 10 * (gameLevel + 1);
        maxAliveEnemyCount = playerCount + 3 + gameLevel;
        minEnemySpawnTime = configMinEnemySpawnTime;
        maxEnemySpawnTime = configMaxEnemySpawnTime - (playerCount + gameLevel + 1) * 0.5f;
        minEnemyFireTime = configMinEnemyFireTime;
        maxEnemyFireTime = configMaxEnemyFireTime - (playerCount + gameLevel + 1) * 0.5f;
        minEnemyMoveTime = configMinEnemyMoveTime;
        maxEnemyMoveTime = configMaxEnemyMoveTime - (playerCount + gameLevel + 1) * 0.5f;
        enemySpawnPointWaitTime = configEnemySpawnPointWaitTime - (playerCount + gameLevel + 1) * 0.25f;
    }
    // 开始新关卡
    private static void GameLevelDataInitial() {
        enemyCrizy = false;
        // 读取玩家数量 (可能有玩家退出?)
        playerCount = networkPlayers.Count;
        isStopWatchRunning = false;
        // 清空生成数据
        spawnedEnemyCount = 0;
        aliveEnemyCount = 0;
        alivePlayerCount = 0;
        SetLevelDifficulty();
    }
    private static void OnEnemySpawn() {
        ++spawnedEnemyCount;
        ++aliveEnemyCount;
        if (!enemyCrizy && totalEnemyCount - spawnedEnemyCount <= (gameLevel - 2) * 2) {
            enemyCrizy = true;
            Messager.Instance.Send(MessageID.ENEMY_CRIZY);
        }
        Messager.Instance.Send(MessageID.DATA_ENEMY_SPAWN);
    }
    private static void OnMsgEnemyDie() {
        --aliveEnemyCount;
        ++killedEnemyCount;
        Messager.Instance.Send(MessageID.DATA_ENEMY_DIE);
    }
    private static void OnMsgPlayerSpawn(int id, bool isFree) {
        ++alivePlayerCount;
        if (isFree) {
            return;
        }
        --playerLifeCount;
        Messager.Instance.Send<int>(MessageID.DATA_PLAYER_SPAWN, id);
    }
    private static void OnMsgPlayerDie(int id) {
        --alivePlayerCount;
        Messager.Instance.Send<int>(MessageID.DATA_PLAYER_DIE, id);
    }
    private static void OnMsgBonusTank() {
        ++playerLifeCount;
        Messager.Instance.Send(MessageID.DATA_BONUS_TANK);
    }
    private static void OnMsgGameWin() {
        isGamePlaying = false;
    }
    private static void OnMsgGameOver() {
        isGamePlaying = false;
    }
    private static void OnMsgGamePause() {
        isGamePausing = true;
    }
    private static void OnMsgGameResume() {
        isGamePausing = false;
    }
}
