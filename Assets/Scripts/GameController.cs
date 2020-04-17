using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameController : MonoBehaviour
{
    public GameObject spawnerPrefab;
    public float minEnemySpawnTime = 1.0f; // 最短检测刷新敌人的时间
    public float maxEnemySpawnTime = 10.0f; // 最长检测刷新敌人的时间
    public int maxEnemyCount = 6; // 最多同时存在的敌人数量, 达到该数量则不再刷新
    public readonly float minEnemySpawnDuration = 10.0f; // 每个位置该时间内只能生成一次
    public readonly Vector3[] enemySpawnPoints = new Vector3[] {
        new Vector3(-6f, 6f, 0f), // left
        new Vector3(0f, 6f, 0f), // middle
        new Vector3(6f, 6f, 0f), // right
    };
    public readonly Vector3[] playerSpawnPoints = new Vector3[] {
        new Vector3(-2f, -6f, 0f), // 1p
        new Vector3(2f, -6f, 0f), // 2p
        new Vector3(-3f, -6f, 0f), // todo 3p
        new Vector3(4f, -6f, 0f), // todo 4p
    };
    private float[] enemySpawnDurationTimers; // 每个位置产生敌人的计时
    private float enemySpawnTimer; // 刷新敌人的计时器
    private int currentEnemyCount;
    private int currentPlayerCount;
    private void Start() {
        enemySpawnDurationTimers = new float[enemySpawnPoints.Length];
        InitialSpawn();
    }
    private void Update() {
        EnemyUpdate();
    }
    private void EnemyUpdate() {
        // 更新敌人出生点计时器
        for(int i = 0; i < enemySpawnDurationTimers.Length; ++i) {
            if (enemySpawnDurationTimers[i] > 0f) {
                enemySpawnDurationTimers[i] -= Time.deltaTime;
            }
        }
        // 更新刷新间隔计时器
        if (enemySpawnTimer > 0f) {
            enemySpawnTimer -= Time.deltaTime;
        }
        // 检测刷新敌人
        if (enemySpawnTimer <= 0f) {
            enemySpawnTimer = Random.Range(minEnemySpawnTime, maxEnemySpawnTime);
            if (currentEnemyCount < maxEnemyCount) {
                SpawnEnemy();
            }
        }
    }
    private void SpawnEnemy() {
        // 随机生成位置
        int randomIndex = Random.Range(0, enemySpawnPoints.Length);
        int availableIndex = -1;
        for (int i = 0; i < enemySpawnPoints.Length; ++i) {
            int index = (randomIndex + i) % enemySpawnPoints.Length;
            if (enemySpawnDurationTimers[i] <= 0f) {
                availableIndex = index;
                break;
            }
        }
        if (availableIndex < 0) { return; }
        // 设置计时器
        enemySpawnDurationTimers[availableIndex] = minEnemySpawnDuration;
        // 随机属性
        int type = 0; // 0 - 4
        bool bonus = false;
        int rand1 = Random.Range(0, 10);
        int rand2 = Random.Range(0, 10);
        if (rand1 < 4) { type = 0; }
        else if (rand1 < 7) { type = 1; }
        else { type = rand1 - 5; }
        bonus = rand2 < 2;
        // 实例化
        GameObject obj = Instantiate(spawnerPrefab, enemySpawnPoints[availableIndex], Quaternion.Euler(0f, 0f, 0f));
        Spawner spawner = obj.GetComponent<Spawner>();
        if (spawner != null) {
            spawner.SetSpawnParam(false, type, bonus);
            ++currentEnemyCount;
        }
    }
    private void SpawnPlayer(int index) {
        if (index < 0 || index >= playerSpawnPoints.Length) {
            return;
        }
        GameObject obj = Instantiate(spawnerPrefab, playerSpawnPoints[index], Quaternion.Euler(0f, 0f, 0f));
        Spawner spawner = obj.GetComponent<Spawner>();
        if (spawner != null) {
            spawner.SetSpawnParam(true);
            ++currentPlayerCount;
        }
    }
    private void InitialSpawn() {
        for (int i = 0; i < enemySpawnPoints.Length; ++i) {
            SpawnEnemy();
        }
        SpawnPlayer(0);
    }
    // todo 胜负判断
    // todo 死亡处理
}
