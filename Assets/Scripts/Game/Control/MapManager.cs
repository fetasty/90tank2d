using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BlockType {
    WALL,
    STEEL,
    WATER,
    GRASS,
    HOME
}
public class MapManager : MonoBehaviour {
    public GameObject WallPrefab;       // 墙壁
    public GameObject SteelPrefab;      // 铁墙
    public GameObject GrassPrefab;      // 草
    public GameObject WaterPrefab;      // 水
    public GameObject BonusPrefab;      // 道具
    public GameObject HomePrefab;       // home
    public GameObject WallChangePrefab; // 墙壁更换动画
    // 固定不变的数据
    public const int MAP_RADIUS = 6;    // 地图半径
    private Vector3 HomeSpawnPoint { get; } = new Vector3(0f, -6f, 0f);
    private Vector3 HomeWallCheckPoint { get; } = new Vector3(0f, -5.5f, 0f);
    private Vector3 HomeWallCheckSize { get; } = new Vector2(2.8f, 1.8f);
    private GameObject maps;
    private bool[,] existBlocks = new bool[MAP_RADIUS * 2 + 1, MAP_RADIUS * 2 + 1];
    private float homeWallTimer;
    private bool homeWallAnim; // 动画是否播放过
    private void Start() {
        maps = GameObject.Find("/Maps");
        GameController.Instance.AddListener(MsgID.BONUS_SPAWN, OnMsgSpawnBonus);
        GameController.Instance.AddListener(MsgID.GAME_RETRY, OnMsgGameStart);
        GameController.Instance.AddListener(MsgID.GAME_START, OnMsgGameStart);
        GameController.Instance.AddListener(MsgID.BONUS_SHOVEL_TRIGGER, OnMsgBonusShovel);
    }
    private void Update() {
        HomeWallUpdate();
    }
    private void HomeWallUpdate() {
        if (homeWallTimer > 0f) {
            homeWallTimer -= Time.deltaTime;
            if (homeWallTimer < 5f && !homeWallAnim) {
                homeWallAnim = true;
                CreateHomeWall(WallChangePrefab);
            }
            if (homeWallTimer <= 0f) {
                ClearHomeWall();
                CreateHomeWall(WallPrefab);
            }
        }
    }
    private void OnMsgSpawnBonus(Msg msg) {
        float x = (float) Random.Range(-MAP_RADIUS + 1, MAP_RADIUS - 1);
        float y = (float) Random.Range(-MAP_RADIUS + 2, MAP_RADIUS - 1);
        GameObject bonus = Instantiate(BonusPrefab, new Vector3(x, y, 1f), Quaternion.identity);
        bonus.transform.parent = maps.transform;
    }
    private void OnMsgGameStart(Msg msg) {
        ClearMap();
        CreateMap();
    }
    private void OnMsgBonusShovel(Msg msg) {
        ClearHomeWall();
        CreateHomeWall(GetPrefab(BlockType.STEEL));
        homeWallTimer = 40f;
        homeWallAnim = false;
    }
    private void ClearHomeWall() {
        // 清空home周围的墙壁
        Collider2D[] colliders = Physics2D.OverlapBoxAll(new Vector2(0f, -5.5f), new Vector2(2.8f, 1.8f), 0f);
        foreach (Collider2D collider in colliders) {
            if (collider.transform.parent != null && collider.transform.parent.tag == "Map") {
                Destroy(collider.transform.parent.gameObject);
            }
        }
    }
    public void ClearMap() {
        // 清理地图
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Map");
        foreach(GameObject block in blocks) {
            Destroy(block);
        }
        // 清理老家
        GameObject[] homes = GameObject.FindGameObjectsWithTag("Home");
        foreach(GameObject home in homes) {
            Destroy(home);
        }
        // 清理道具
        GameObject[] bonuses = GameObject.FindGameObjectsWithTag("Bonus");
        foreach(GameObject bonus in bonuses) {
            Destroy(bonus);
        }
        for (int i = 0; i < existBlocks.GetLength(0); ++i) {
            for (int j = 0; j < existBlocks.GetLength(1); ++j) {
                existBlocks[i, j] = false;
            }
        }
        homeWallTimer = 0f;
        homeWallAnim = true;
    }
    /// <summary>
    /// 随机生成地图
    /// </summary>
    public void CreateMap() {
        // 生成Home
        Instantiate(HomePrefab, HomeSpawnPoint, Quaternion.identity);
        // 生成HomeWall
        CreateHomeWall(GetPrefab(BlockType.WALL));
        // 生成其他方块
        CreateBlocks();
    }
    private void CreateBlocks() {
        // 土墙
        for (int i = 0; i < 30; ++i) {
            CreateBlock(WallPrefab, existBlocks);
        }
        // 铁
        for (int i = 0; i < 10; ++i) {
            CreateBlock(SteelPrefab, existBlocks);
        }
        // 水
        for (int i = 0; i < 5; ++i) {
            CreateBlock(WaterPrefab, existBlocks);
        }
        // 草
        for (int i = 0; i < 10; ++i) {
            CreateBlock(GrassPrefab, existBlocks);
        }
    }
    private void CreateBlock(GameObject pref, bool[,] existBlocks) {
        while (true) {
            int x = Random.Range(-MAP_RADIUS, MAP_RADIUS + 1);
            int y = Random.Range(-MAP_RADIUS + 1, MAP_RADIUS);
            if (existBlocks[x + MAP_RADIUS, y + MAP_RADIUS]) { continue; }
            if (!IsPositionAvailable(x, y)) { continue; }
            existBlocks[x + MAP_RADIUS, y + MAP_RADIUS] = true;
            Instantiate(pref, new Vector3((float) x, (float) y, 0f), Quaternion.identity)
            .transform.parent = maps.transform;
            return;
        }
    }
    private void CreateHomeWall(GameObject pref) {
        for (int i = -1; i <= 1; ++i) {
            for (int j = -6; j <= -5; ++j) {
                if (i == 0 && j == -6) { continue; }
                Instantiate(pref, new Vector3((float)i, (float)j, 0f), Quaternion.identity).transform.parent = maps.transform;
            }
        }
    }
    private GameObject GetPrefab(BlockType type) {
        switch (type) {
            case BlockType.STEEL:
                return SteelPrefab;
            case BlockType.GRASS:
                return GrassPrefab;
            case BlockType.WATER:
                return WaterPrefab;
            default:
                return WallPrefab;
        }
    }
    /// <summary>
    /// 玩家老家, 玩家出生点, 敌人出生点 都不可被占用
    /// </summary>
    /// <param name="x">坐标x值</param>
    /// <param name="y">坐标y值</param>
    /// <returns>是否可用</returns>
    private bool IsPositionAvailable(float x, float y) {
        if (y < -5f || y > 5f) { return false; }
        if (x < 2f && x > -2f && y < -4f) { return false; }
        return true;
    }
    private bool IsPositionAvailable(int x, int y) {
        if (y < -5 || y > 5) { return false; }
        if (x > -2 && x < 2 && y < -4) { return false; }
        return true;
    }
    /// <summary>
    /// 根据预设数据生成地图
    /// </summary>
    /// <param name="blockData">BlockType类型的二维数组, [0][0]表示左下角, [12][12]表示右上角</param>
    public void CreateMap(BlockType[][] blockData) {
        // todo
    }
    public void SetBlock(BlockType type, Vector3 position) {

    }
}
