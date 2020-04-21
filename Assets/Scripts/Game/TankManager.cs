using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour {
    public GameObject SpawnerPrefab;    // tank生成器
    public Vector3[] PlayerSpawnPoints { get; } = new Vector3[] {
        new Vector3(-2f, -6f, 0f), // 1p
        new Vector3(2f, -6f, 0f), // 2p
        new Vector3(-3f, -6f, 0f), // todo 3p
        new Vector3(4f, -6f, 0f), // todo 4p
    };
    public Vector3[] EnemySpawnPoints { get; } = new Vector3[] {
        new Vector3(-6f, 6f, 0f), // left
        new Vector3(0f, 6f, 0f), // middle
        new Vector3(6f, 6f, 0f), // right
    };
    private float[] enemySpawnPointTimers;  // 敌人出生点计时器, 减小到0允许使用
    private float enemySpawnTimer;          // 自然刷新敌人计时器, 减小到0允许刷新
    private bool[] playerExists;           // 已经存在的playerID
    private GameInfoManager info;
    private void Start() {
        enemySpawnPointTimers = new float[EnemySpawnPoints.Length];
        playerExists = new bool[PlayerSpawnPoints.Length];
        info = GameController.Instance.InfoManager;
        GameController.Instance.AddListener(MsgID.GAME_START, OnMsgGameStart);
        GameController.Instance.AddListener(MsgID.PLAYER_DIE, OnMsgPlayerDie);
        GameController.Instance.AddListener(MsgID.BONUS_BOOM_TRIGGER, OnMsgBonusTrigger);
    }
    private void Update() {
        EnemySpawnUpdate();
    }
    /// <summary>
    /// 每帧调用, 自动刷新生成敌人
    /// </summary>
    private void EnemySpawnUpdate() {
        if (!info.IsGamePlaying) { return; }
        for (int i = 0; i < enemySpawnPointTimers.Length; ++i) {
            if (enemySpawnPointTimers[i] > 0f) { enemySpawnPointTimers[i] -= Time.deltaTime; }
        }
        if (enemySpawnTimer > 0f) {
            enemySpawnTimer -= Time.deltaTime;
            return;
        }
        enemySpawnTimer = info.EnemySpawnTime;
        if (!info.CanSpawnEnemy) { return; }
        SpawnEnemy();
    }
    /// <summary>
    /// 游戏开始时, 在每个出生点生成敌人
    /// </summary>
    public void EnemyInitialSpawn() {
        for (int i = 0; i < EnemySpawnPoints.Length; ++i) {
            if (info.CanSpawnEnemy) {
                SpawnEnemy(i);
            }
        }
    }
    /// <summary>
    /// 生成一个敌人(生成器)
    /// </summary>
    /// <param name="index">出生点的下标, 不填写则随机一个值</param>
    /// <returns>敌人是否生成成功</returns>
    public bool SpawnEnemy(int index = -1) {
        if (index < 0) {
            index = Random.Range(0, EnemySpawnPoints.Length);
            bool available = false;
            for (int i = 0; i < EnemySpawnPoints.Length; ++i) {
                index = (index + i) % EnemySpawnPoints.Length;
                if (enemySpawnPointTimers[index] <= 0f) {
                    available = true;
                    break;
                }
            }
            if (!available) { return false; }
        } else {
            index = index % EnemySpawnPoints.Length;
            if (enemySpawnPointTimers[index] > 0f) { return false; }
        }
        GameObject obj = Instantiate(SpawnerPrefab, EnemySpawnPoints[index], Quaternion.identity);
        Spawner spawner = obj.GetComponent<Spawner>();
        spawner.SetEnemy(info.EnemyType, info.EnemyBonus);
        enemySpawnPointTimers[index] = info.EnemySpawnPointWaitTime;
        return true;
    }
    /// <summary>
    /// 生成玩家
    /// </summary>
    /// <param name="playerID">玩家的唯一标识</param>
    /// <returns>是否生成成功</returns>
    public bool SpawnPlayer(int playerID) {
        if (playerID < Player.MIN_ID || playerID > Player.MAX_ID) { return false; } // 未设计这种玩家
        if (IsPlayerExist(playerID)) { return false; }
        GameObject obj = Instantiate(SpawnerPrefab, PlayerSpawnPoints[playerID], Quaternion.identity);
        Spawner spawner = obj.GetComponent<Spawner>();
        spawner.SetPlayer(playerID);
        playerExists[playerID] = true;
        return true;
    }
    private bool IsPlayerExist(int playerID) {
        return playerExists[playerID];
    }
    private void Clear() {
        // spawner
        Spawner[] spawners = FindObjectsOfType<Spawner>();
        foreach (Spawner spawner in spawners) {
            Destroy(spawner.gameObject);
        }
        // player
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players) {
            Destroy(player.gameObject);
        }
        // enemy
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies) {
            Destroy(enemy.gameObject);
        }
        // bullet
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet bullet in bullets) {
            Destroy(bullet.gameObject);
        }
        enemySpawnTimer = info.EnemySpawnTime; // 一开始不允许刷新
        for (int i = 0; i < playerExists.Length; ++i) {
            playerExists[i] = false;
        }
        for (int i = 0; i < enemySpawnPointTimers.Length; ++i) {
            enemySpawnPointTimers[i] = 0f;
        }
    }
    /// <summary>
    /// 接收到游戏开始事件
    /// </summary>
    public void OnMsgGameStart(Msg msg) {
        Clear();
        EnemyInitialSpawn();
        GameMode mode = (GameMode) msg.Param;
        if (mode == GameMode.SINGLE) {
            SpawnPlayer(0);
        } else if (mode == GameMode.DOUBLE) {
            SpawnPlayer(0);
            SpawnPlayer(1);
        } else {
            // todo 局域网游戏
            Debug.Log("Start Lan Game!!!");
        }
    }
    public void OnMsgPlayerDie(Msg msg) {
        int playerID = (int)msg.Param;
        playerExists[playerID] = false;
        if (info.CanSpawnPlayer) {
            SpawnPlayer(playerID);
        }
    }
    public void OnMsgBonusTrigger(Msg msg) {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies) {
            enemy.Die();
        }
    }
}
