using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameController : MonoBehaviour
{
    private static GameController controller;
    public static GameController Current {
        get { return controller; }
    }
    public GameObject spawnerPrefab;
    public GameObject homePrefab;
    public GameObject gameOverPrefab;
    public GameObject gameWinPrefab;
    public Text enemyCountText;
    public Text killedCountText;
    public Text tankCountText;
    public GameObject buttons;
    public Button backBtn;
    public Button exitBtn;
    public float minEnemySpawnTime = 3.0f; // 最短检测刷新敌人的时间
    public float maxEnemySpawnTime = 8.0f; // 最长检测刷新敌人的时间
    public int totalEnemyCount = 30; // 一共需要生成的敌人数量
    public int maxEnemyCount = 6; // 最多同时存在的敌人数量, 达到该数量则不再刷新
    public float minEnemySpawnDuration = 6.0f; // 每个位置该时间内只能生成一次
    public int totalPlayerTankCount = 5; // 玩家总共拥有的坦克数量
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
    public readonly Vector3 homeSpawnPoint = new Vector3(0f, -6f, 0f);
    private float[] enemySpawnDurationTimers; // 每个位置产生敌人的计时
    private float enemySpawnTimer; // 刷新敌人的计时器
    // 游戏相关
    private int currentEnemyCount;
    private int enemySpawnCount; // 总共生成的敌人数量
    private int EnemySpawnCount {
        get { return enemySpawnCount; }
        set {
            if (value >= 0 && value <= totalEnemyCount) {
                enemySpawnCount = value;
                enemyCountText.text = $"剩余敌人 : {totalEnemyCount - enemySpawnCount}";
            }
        }
    }
    private bool isGameOver;
    public bool IsGameOver { get{ return isGameOver; }}
    private int playerUsedTankCount; // 玩家已经使用的tank数量
    private int PlayerUsedTankCount {
        get { return playerUsedTankCount; }
        set {
            if (value >= 0 && value <= totalPlayerTankCount) {
                playerUsedTankCount = value;
                tankCountText.text = $"玩家生命 : {totalPlayerTankCount - playerUsedTankCount}";
            }
        }
    }
    private int killedCount;
    private int KilledCount {
        get { return killedCount; }
        set {
            if (value >= 0 && value <= totalEnemyCount) {
                killedCount = value;
                killedCountText.text = $"击杀得分 : {killedCount}";
            }
        }
    }
    // 效果相关
    public float enemyPauseTime; // 敌人被道具定身时长
    private float enemyPauseTimer;
    public bool IsEnemyPaused { get { return enemyPauseTimer > 0f; }}
    // todo 玩家home的保护特效
    private void Start() {
        controller = this;
        enemySpawnDurationTimers = new float[enemySpawnPoints.Length];
        Initial();
        isGameOver = false;
        buttons.SetActive(false);
        backBtn.onClick.AddListener(() => {
            GameManager.Instance.LoadSceneAsync("Welcome");
        });
        exitBtn.onClick.AddListener(() => {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
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
        if (EnemySpawnCount < totalEnemyCount && enemySpawnTimer <= 0f) {
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
                enemySpawnDurationTimers[i] = minEnemySpawnDuration;
                break;
            }
        }
        if (availableIndex < 0) { return; }
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
        spawner.SetEnemy(type, bonus, EnemyDie, TriggerBonus);
        ++currentEnemyCount;
        ++EnemySpawnCount;
    }
    private void SpawnPlayer(int index) {
        if (index < 0 || index >= playerSpawnPoints.Length) {
            return;
        }
        GameObject obj = Instantiate(spawnerPrefab, playerSpawnPoints[index], Quaternion.Euler(0f, 0f, 0f));
        Spawner spawner = obj.GetComponent<Spawner>();
        spawner.SetPlayer(index, PlayerDie);
        ++PlayerUsedTankCount;
    }
    private void SpawnHome() {
        GameObject obj = Instantiate(homePrefab, homeSpawnPoint, Quaternion.Euler(0f, 0f, 0f));
        Home home = obj.GetComponent<Home>();
        home.Set(HomeDamage);
    }
    private void InitUI() {
        EnemySpawnCount = EnemySpawnCount;
        KilledCount = KilledCount;
        PlayerUsedTankCount = PlayerUsedTankCount;
    }
    private void Initial() {
        InitUI();
        SpawnHome();
        for (int i = 0; i < enemySpawnPoints.Length; ++i) {
            SpawnEnemy();
        }
        SpawnPlayer(0);
    }
    public void PlayerDie(int id) {
        if (PlayerUsedTankCount < totalPlayerTankCount) {
            SpawnPlayer(0);
        } else {
            GameOver();
        }
    }
    public void EnemyDie(int type) {
        --currentEnemyCount;
        ++KilledCount;
        if (KilledCount >= totalEnemyCount) {
            GameWin();
        }
    }
    public void TriggerBonus(int enemyType) {
        // todo
        Debug.Log($"Trigger bonus, enemy type {enemyType}");
    }
    public void HomeDamage(bool destroyed) {
        GameOver();
    }
    private void GameOver() {
        isGameOver = true;
        buttons.SetActive(true);
        Instantiate(gameOverPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
    }
    private void GameWin() {
        isGameOver = true;
        buttons.SetActive(true);
        Instantiate(gameWinPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
    }
}
