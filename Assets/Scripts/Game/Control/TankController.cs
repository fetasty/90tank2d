using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/// <summary>
/// ServerOnly!
/// 管理玩家与敌人的生成
/// </summary>
public class TankController : NetworkBehaviour {
    public GameObject SpawnerPrefab;    // tank生成器
    public Vector3[] PlayerSpawnPoints { get; } = new Vector3[] {
        new Vector3(-2f, -6f, 0f), // 1p
        new Vector3(2f, -6f, 0f), // 2p
        new Vector3(-3f, -6f, 0f), // 3p
        new Vector3(4f, -6f, 0f), // 4p
    };
    public Vector3[] EnemySpawnPoints { get; } = new Vector3[] {
        new Vector3(-6f, 6f, 0f), // left
        new Vector3(0f, 6f, 0f), // middle
        new Vector3(6f, 6f, 0f), // right
    };
    private float[] enemySpawnPointTimers;  // 敌人出生点计时器, 减小到0允许使用
    private float enemySpawnTimer;          // 自然刷新敌人计时器, 减小到0允许刷新
    private bool[] playerExists;           // 已经存在的playerID
    [ServerCallback]
    private void Start() {
        enemySpawnPointTimers = new float[EnemySpawnPoints.Length];
        playerExists = new bool[PlayerSpawnPoints.Length];
        // 事件监听
        Messager.Instance.Listen(MessageID.DATA_GAME_START, OnMsgGameStart);
        Messager.Instance.Listen<int>(MessageID.DATA_PLAYER_DIE, OnMsgPlayerDie);
        Messager.Instance.Listen(MessageID.BONUS_BOOM_TRIGGER, OnMsgBonusBoomTrigger);
        Messager.Instance.Listen(MessageID.BONUS_TANK_TRIGGER, OnMsgBonusTankTrigger);
        Messager.Instance.Listen(MessageID.GAME_RETRY, OnMsgGameRetry);
    }
    [ServerCallback]
    private void Update() {
        if (GameData.isGamePausing) { return; }
        EnemySpawnUpdate();
    }
    /// <summary>
    /// 每帧调用, 自动刷新生成敌人
    /// </summary>
    private void EnemySpawnUpdate() {
        if (!GameData.isGamePlaying) { return; }
        for (int i = 0; i < enemySpawnPointTimers.Length; ++i) {
            if (enemySpawnPointTimers[i] > 0f) { enemySpawnPointTimers[i] -= Time.deltaTime; }
        }
        if (enemySpawnTimer > 0f) {
            enemySpawnTimer -= Time.deltaTime;
            return;
        }
        enemySpawnTimer = GameData.EnemySpawnTime;
        if (!GameData.CanSpawnEnemy) { return; }
        SpawnEnemy();
    }
    /// <summary>
    /// 游戏开始时, 在每个出生点生成敌人
    /// </summary>
    public void EnemyInitialSpawn() {
        for (int i = 0; i < EnemySpawnPoints.Length; ++i) {
            if (GameData.CanSpawnEnemy) {
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
        spawner.SetEnemy(GameData.EnemyType, GameData.EnemyBonus);
        NetworkServer.Spawn(obj);
        enemySpawnPointTimers[index] = GameData.enemySpawnPointWaitTime;
        return true;
    }
    /// <summary>
    /// 生成玩家
    /// </summary>
    /// <param name="playerID">玩家的唯一标识</param>
    /// <returns>是否生成成功</returns>
    public bool SpawnPlayer(int playerID) {
        if (playerID < Player.MIN_ID || playerID > Player.MAX_ID) { return false; } // 未设计这种玩家
        if (playerExists[playerID]) { return false; }
        GameObject obj = Instantiate(SpawnerPrefab, PlayerSpawnPoints[playerID], Quaternion.identity);
        Spawner spawner = obj.GetComponent<Spawner>();
        spawner.SetPlayer(playerID);
        NetworkServer.Spawn(obj);
        playerExists[playerID] = true;
        return true;
    }
    private void Clear() {
        // spawner
        Spawner[] spawners = FindObjectsOfType<Spawner>();
        foreach (Spawner spawner in spawners) {
            NetworkServer.Destroy(spawner.gameObject);
        }
        // player
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players) {
            NetworkServer.Destroy(player.gameObject);
        }
        // enemy
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies) {
            NetworkServer.Destroy(enemy.gameObject);
        }
        // bullet
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet bullet in bullets) {
            NetworkServer.Destroy(bullet.gameObject);
        }
        enemySpawnTimer = GameData.EnemySpawnTime; // 一开始不允许刷新
        for (int i = 0; i < playerExists.Length; ++i) {
            playerExists[i] = false;
        }
        for (int i = 0; i < enemySpawnPointTimers.Length; ++i) {
            enemySpawnPointTimers[i] = 0f;
        }
    }
    private void PlayerInitialSpawn() {
        TankMode mode = GameData.mode;
        if (mode == TankMode.SINGLE) {
            // todo
            SpawnPlayer(0);
        } else if (mode == TankMode.DOUBLE) {
            // todo
            SpawnPlayer(0);
            SpawnPlayer(1);
        } else {
            // todo 局域网游戏
            Debug.Log("Start Lan Game!!!");
            for (int i = 0; i < GameData.networkPlayers.Count; ++i) {
                SpawnPlayer(i);
            }
        }
    }
    /// <summary>
    /// 接收到游戏开始事件
    /// </summary>
    public void OnMsgGameStart() {
        Clear();
        EnemyInitialSpawn();
        PlayerInitialSpawn();
    }
    private void OnMsgGameRetry() {
        Clear();
        EnemyInitialSpawn();
        PlayerInitialSpawn();
    }
    public void OnMsgPlayerDie(int id) {
        playerExists[id] = false;
        if (GameData.CanSpawnPlayer) {
            SpawnPlayer(id);
        }
    }
    public void OnMsgBonusBoomTrigger() {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies) {
            enemy.Die();
        }
    }
    public void OnMsgBonusTankTrigger() {
        // 看看有没有需要重生的玩家
        int id = GetNeedSpawnPlayerID();
        if (id < 0) { return; }
        if (GameData.CanSpawnPlayer) { SpawnPlayer(id); }
    }
    /// <summary>
    /// 需要被重生的玩家ID
    /// </summary>
    /// <returns>小于0说明没有玩家需要重生, 否则返回需要重生的玩家ID</returns>
    private int GetNeedSpawnPlayerID() {
        TankMode mode = GameData.mode;
        // todo 单机模式下的改进
        if (!playerExists[0] && (mode == TankMode.SINGLE || mode == TankMode.DOUBLE)) {
            return 0;
        }
        if (!playerExists[1] && (mode == TankMode.DOUBLE)) {
            return 1;
        }
        for (int i = 0; i < GameData.networkPlayers.Count; ++i) {
            if (!playerExists[i]) {
                return i;
            }
        }
        return -1;
    }
}
