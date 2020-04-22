using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public float spawnTime = 1f;
    // Player生成相关参数
    public bool SpawnPlayer { get; private set; }
    public int PlayerID { get; private set; }
    // Enemy生成相关参数
    public int EnemyType {get; private set;}
    public bool EnemyBonus { get; private set; }
    private float spawnTimer;
    private void Start() {
        GameObject spawners = GameObject.Find("/Spawners");
        if (spawners != null) {
            transform.parent = spawners.transform;
        }
        if (SpawnPlayer) {
            GameController.Instance.PostMsg(new Msg(MsgID.PLAYER_SPAWN, this));
        } else {
            GameController.Instance.PostMsg(new Msg(MsgID.ENEMY_SPAWN, this));
        }
        spawnTimer = spawnTime;
    }
    private void Update() {
        SpawnUpdate();
    }
    /// <summary>
    /// 设置生成敌人参数
    /// </summary>
    /// <param name="enemyType">生成的敌人类型</param>
    /// <param name="enemyBonus">生成的敌人是否带有奖励</param>
    public void SetEnemy(int enemyType = 0, bool enemyBonus = false) {
        this.SpawnPlayer = false;
        this.EnemyType = enemyType;
        this.EnemyBonus = enemyBonus;
    }
    public void SetPlayer(int id = 0) {
        this.SpawnPlayer = true;
        this.PlayerID = id;
    }
    private void SpawnUpdate() {
        if (spawnTimer > 0f) {
            spawnTimer -= Time.deltaTime;
        }
        if (spawnTimer <= 0f) {
            if (SpawnPlayer) {
                GameObject obj = Instantiate(playerPrefab, transform.position, Quaternion.identity);
                Player player = obj.GetComponent<Player>();
                player.Set(PlayerID);
            } else {
                GameObject obj = Instantiate(enemyPrefab, transform.position, Quaternion.Euler(0f, 0f, 180f));
                Enemy enemy = obj.GetComponent<Enemy>();
                enemy.Set(EnemyType, EnemyBonus);
            }
            Destroy(gameObject);
        }
    }
}
