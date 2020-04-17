using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 4f;
    private float spawnTime;
    private bool spawnPlayer;
    private int enemyType = 0;
    private bool enemyBonus = false;
    private void Start() {
        spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }
    private void Update() {
        SpawnUpdate();
    }
    /// <summary>
    /// 设置生成参数, 如果生成Player, 则后面两个enemyXX参数无效
    /// </summary>
    /// <param name="spawnPlayer">是否生成玩家</param>
    /// <param name="enemyType">生成的敌人类型</param>
    /// <param name="enemyBonus">生成的敌人是否带有奖励</param>
    public void SetSpawnParam(bool spawnPlayer, int enemyType = 0, bool enemyBonus = false) {
        this.spawnPlayer = spawnPlayer;
        if (!spawnPlayer) {
            this.enemyType = enemyType;
            this.enemyBonus = enemyBonus;
        }
    }
    private void SpawnUpdate() {
        if (spawnTime > 0f) {
            spawnTime -= Time.deltaTime;
        }
        if (spawnTime <= 0f) {
            if (spawnPlayer) {
                Instantiate(playerPrefab, transform.position, Quaternion.Euler(0f, 0f, 0f));
            } else {
                GameObject obj = Instantiate(enemyPrefab, transform.position, Quaternion.Euler(0f, 0f, 180f));
                Enemy enemy = obj.GetComponent<Enemy>();
                enemy.Type = enemyType;
                enemy.Bonus = enemyBonus;
            }
            Destroy(gameObject);
        }
    }
}
