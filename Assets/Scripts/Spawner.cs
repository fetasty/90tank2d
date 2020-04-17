using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 3f;
    private float spawnTime;
    // Player生成相关参数
    private bool spawnPlayer;
    private int playerID;
    private System.Action<int> playerDieAction;
    // Enemy生成相关参数
    private int enemyType = 0;
    private bool enemyBonus = false;
    private System.Action<int> enemyDieAction;
    private System.Action<int> bonusAction;
    private void Start() {
        spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }
    private void Update() {
        SpawnUpdate();
    }
    /// <summary>
    /// 设置生成敌人参数
    /// </summary>
    /// <param name="enemyType">生成的敌人类型</param>
    /// <param name="enemyBonus">生成的敌人是否带有奖励</param>
    public void SetEnemy(int enemyType = 0, bool enemyBonus = false,
    System.Action<int> enemyDieAction = null, System.Action<int> bonusAction = null) {
        this.spawnPlayer = false;
        this.enemyType = enemyType;
        this.enemyBonus = enemyBonus;
        this.enemyDieAction = enemyDieAction;
        this.bonusAction = bonusAction;
    }
    public void SetPlayer(int id = 0, System.Action<int> playerDieAction = null) {
        this.spawnPlayer = true;
        this.playerID = id;
        this.playerDieAction = playerDieAction;
    }
    private void SpawnUpdate() {
        if (spawnTime > 0f) {
            spawnTime -= Time.deltaTime;
        }
        if (spawnTime <= 0f) {
            if (spawnPlayer) {
                GameObject obj = Instantiate(playerPrefab, transform.position, Quaternion.Euler(0f, 0f, 0f));
                Player player = obj.GetComponent<Player>();
                player.Set(playerID, playerDieAction);
            } else {
                GameObject obj = Instantiate(enemyPrefab, transform.position, Quaternion.Euler(0f, 0f, 180f));
                Enemy enemy = obj.GetComponent<Enemy>();
                enemy.Set(enemyType, enemyBonus, enemyDieAction, bonusAction);
            }
            Destroy(gameObject);
        }
    }
}
