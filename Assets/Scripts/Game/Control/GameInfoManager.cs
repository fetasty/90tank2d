using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏相关信息
/// </summary>
public class GameInfoManager : MonoBehaviour {
    // 游戏中动态改变的数据
    public bool IsGamePlaying { get; private set; }                 // 游戏是否运行中, 未出现胜负
    public bool IsGamePause { get; private set; }                   // 游戏是否暂停中
    public int AlivePlayerCount { get; private set; }               // 活着的玩家数量
    public int AliveEnemyCount { get; private set; }                // 活着的敌人数量
    public int SpawnedEnemyCount { get; private set; }              // 一共已经生成的敌人数量
    public int SpawnedPlayerCount { get; private set; }             // 一共已经成成的玩家数量
    public int PlayerTankCount { get; private set; }                // 玩家拥有的生命条数
    public int KilledEnemyCount { get; private set; }               // 击杀的敌人数量

    // 通过接口修改, 可以用来控制难度
    private int totalEnemyCount = 40;               // 一共需要生成的敌人数量
    private int maxAliveEnemyCount = 6;             // 允许同时或者的敌人数量
    private float minEnemySpawnTime = 0.5f;           // 刷新敌人的最短间隔
    private float maxEnemySpawnTime = 3f;           // 刷新敌人的最大间隔
    private float enemySpawnPointWaitTime = 1.5f;     // 每个敌人出生点的CD
    private int initialPlayerTankCount = 3;         // 玩家的初始生命条数
    private int[] enemyTypeWeights = { 40, 30, 10, 10, 10 };   // 每种类型敌人的权重分界(和不超过100)
    private int enemyBonusWeight = 20;              // 数值不超过100
    private float bonusStopTime = 10f;              // 道具暂停敌人时间
    private float bonusStopTimer;

    private void Start() {
        // InitParam();
        // Messager.Instance.Listen(MessageID.GAME_START, OnMsgGameStart);
        // Messager.Instance.Listen(MessageID.GAME_PAUSE, OnMsgGamePause);
        // Messager.Instance.Listen(MessageID.GAME_OVER, OnMsgGameEnd);
        // Messager.Instance.Listen(MessageID.GAME_WIN, OnMsgGameEnd);
        // Messager.Instance.Listen(MessageID.ENEMY_SPAWN, OnMsgEnemySpawn);
        // Messager.Instance.Listen(MessageID.ENEMY_DIE, OnMsgEnemyDie);
        // Messager.Instance.Listen(MessageID.PLAYER_SPAWN, OnMsgPlayerSpawn);
        // Messager.Instance.Listen(MessageID.PLAYER_DIE, OnMsgPlayerDie);
        // Messager.Instance.Listen(MessageID.BONUS_TANK_TRIGGER, OnMsgBonusTank);
        // Messager.Instance.Listen(MessageID.BONUS_STOP_WATCH_TRIGGER, OnMsgBonusPause);
        // Messager.Instance.Listen(MessageID.GAME_RESUME, OnMsgGameResume);
        // Messager.Instance.Listen(MessageID.GAME_RETRY, OnMsgGameRetry);
        // Messager.Instance.Listen(MessageID.HOME_DESTROY, OnMsgHomeDestroy);
    }
    private void Update() {
        if (bonusStopTimer > 0f) {
            bonusStopTimer -= Time.deltaTime;
        }
    }
    public bool IsBonusStop {
        get { return bonusStopTimer > 0f; }
    }
    public bool CanSpawnEnemy {
        get {
            return (AliveEnemyCount < maxAliveEnemyCount)
            && (SpawnedEnemyCount < totalEnemyCount);
        }
    }
    public float EnemySpawnTime {
        get { 
            return Random.Range(minEnemySpawnTime, maxEnemySpawnTime);
        }
    }
    /// <summary>
    /// 获取按权重随机的敌人类型
    /// </summary>
    /// <returns>敌人的类型值</returns>
    public int EnemyType {
        get {
            int num = Random.Range(0, 100);
            int start = 0;
            for (int i = 0; i < enemyTypeWeights.Length; ++i) {
                if (num > start && num < start + enemyTypeWeights[i]) { return i; }
                start += enemyTypeWeights[i];
            }
            return 0;
        }
    }
    /// <summary>
    /// 获取随机bool值, 表示敌人是否带有奖励
    /// </summary>
    /// <returns>敌人是否带有奖励</returns>
    public bool EnemyBonus {
        get { return Random.Range(0, 100) < enemyBonusWeight; }
    }
    /// <summary>
    /// 敌人出生点CD
    /// </summary>
    public float EnemySpawnPointWaitTime { get { return enemySpawnPointWaitTime; } }
    public bool CanSpawnPlayer { get { return SpawnedPlayerCount < PlayerTankCount; } }
    public int LeftEnemyCount { get { return totalEnemyCount - SpawnedEnemyCount; } }
    private void InitParam() {
        TankMode mode = GameData.mode;
        if (mode == TankMode.DOUBLE) {
            totalEnemyCount += 20;
            maxAliveEnemyCount += 2;
            initialPlayerTankCount += 3;
        }
        // todo
    }
    private void ClearData() {
        AliveEnemyCount = 0;
        AlivePlayerCount = 0;
        SpawnedEnemyCount = 0;
        SpawnedPlayerCount = 0;
        KilledEnemyCount = 0;
        PlayerTankCount = initialPlayerTankCount;
        IsGamePlaying = true;
        IsGamePause = false;
        bonusStopTimer = 0f;
        // GameController.Instance.PostMsg(new Msg(MsgID.GAME_INFO_UPDATE, null));
    }
    public void OnMsgGameStart() {
        ClearData();
    }
    public void OnMsgGameRetry() {
        ClearData();
    }
    public void OnMsgHomeDestroy() {
        if (IsGamePlaying) {
            // GameController.Instance.PostMsg(new Msg(MsgID.GAME_OVER, null));
        }
    }
    public void OnMsgGamePause() {
        IsGamePause = true;
    }
    public void OnMsgGameResume() {
        IsGamePause = false;
    }
    public void OnMsgEnemySpawn() {
        ++SpawnedEnemyCount;
        ++AliveEnemyCount;
    }
    public void OnMsgGameEnd() {
        IsGamePlaying = false;
    }
    public void OnMsgEnemyDie() {
        --AliveEnemyCount;
        ++KilledEnemyCount;
        if (KilledEnemyCount >= totalEnemyCount) {
            // GameController.Instance.PostMsg(new Msg(MsgID.GAME_WIN, null));
        }
    }
    public void OnMsgPlayerSpawn() {
        ++AlivePlayerCount;
        ++SpawnedPlayerCount;
    }
    public void OnMsgPlayerDie() {
        --AlivePlayerCount;
        if (!CanSpawnPlayer && AlivePlayerCount <= 0) {
            // GameController.Instance.PostMsg(new Msg(MsgID.GAME_OVER, null));
        }
    }
    public void OnMsgBonusTank() {
        ++PlayerTankCount;
    }
    public void OnMsgBonusPause() {
        bonusStopTimer = bonusStopTime;
    }
}
