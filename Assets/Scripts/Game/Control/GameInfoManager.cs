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
    private int totalEnemyCount = 30;               // 一共需要生成的敌人数量
    private int maxAliveEnemyCount = 6;             // 允许同时或者的敌人数量
    private float minEnemySpawnTime = 2f;           // 刷新敌人的最短间隔
    private float maxEnemySpawnTime = 4f;           // 刷新敌人的最大间隔
    private float enemySpawnPointWaitTime = 2f;     // 每个敌人出生点的CD
    private int initialPlayerTankCount = 5;         // 玩家的初始生命条数
    private int[] enemyTypeWeights = { 40, 30, 10, 10, 10 };   // 每种类型敌人的权重分界(和不超过100)
    private int enemyBonusWeight = 20;              // 数值不超过100
    private float bonusStopTime = 15f;              // 道具暂停敌人时间
    private float bonusStopTimer;

    private void Start() {
        GameController.Instance.AddListener(MsgID.GAME_START, OnMsgGameStart);
        GameController.Instance.AddListener(MsgID.GAME_PAUSE, OnMsgGamePause);
        GameController.Instance.AddListener(MsgID.GAME_OVER, OnMsgGameEnd);
        GameController.Instance.AddListener(MsgID.GAME_WIN, OnMsgGameEnd);
        GameController.Instance.AddListener(MsgID.ENEMY_SPAWN, OnMsgEnemySpawn);
        GameController.Instance.AddListener(MsgID.ENEMY_DIE, OnMsgEnemyDie);
        GameController.Instance.AddListener(MsgID.PLAYER_SPAWN, OnMsgPlayerSpawn);
        GameController.Instance.AddListener(MsgID.PLAYER_DIE, OnMsgPlayerDie);
        GameController.Instance.AddListener(MsgID.BONUS_TANK_TRIGGER, OnMsgBonusTank);
        GameController.Instance.AddListener(MsgID.BONUS_STOP_WATCH_TRIGGER, OnMsgBonusPause);
        GameController.Instance.AddListener(MsgID.GAME_RESUME, OnMsgGameResume);
        GameController.Instance.AddListener(MsgID.GAME_RETRY, OnMsgGameRetry);
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
        GameController.Instance.PostMsg(new Msg(MsgID.GAME_INFO_UPDATE, null));
    }
    public void OnMsgGameStart(Msg msg) {
        ClearData();
    }
    public void OnMsgGameRetry(Msg msg) {
        ClearData();
    }
    public void OnMsgGamePause(Msg msg) {
        IsGamePause = true;
    }
    public void OnMsgGameResume(Msg msg) {
        IsGamePause = false;
    }
    public void OnMsgEnemySpawn(Msg msg) {
        ++SpawnedEnemyCount;
        ++AliveEnemyCount;
    }
    public void OnMsgGameEnd(Msg msg) {
        IsGamePlaying = false;
    }
    public void OnMsgEnemyDie(Msg msg) {
        --AliveEnemyCount;
        ++KilledEnemyCount;
        if (KilledEnemyCount >= totalEnemyCount) {
            GameController.Instance.PostMsg(new Msg(MsgID.GAME_WIN, null));
        }
    }
    public void OnMsgPlayerSpawn(Msg msg) {
        ++AlivePlayerCount;
        ++SpawnedPlayerCount;
    }
    public void OnMsgPlayerDie(Msg msg) {
        --AlivePlayerCount;
        if (!CanSpawnPlayer && AlivePlayerCount <= 0) {
            GameController.Instance.PostMsg(new Msg(MsgID.GAME_OVER, null));
        }
    }
    public void OnMsgBonusTank(Msg msg) {
        ++PlayerTankCount;
    }
    public void OnMsgBonusPause(Msg msg) {
        bonusStopTimer = bonusStopTime;
    }
}
