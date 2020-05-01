using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public enum BlockType {
    NONE,
    WALL,
    STEEL,
    WATER,
    GRASS,
    HOME
}

/// <summary>
/// 自定义地图数据
/// </summary>
public class MapInfo {
    public const int MAX_LENGTH = 13;
    private BlockInfo[,] infos = new BlockInfo[MAX_LENGTH, MAX_LENGTH]; // 13 * 13 !!!
    public BlockInfo GetBlock(int x, int y) { return infos[x, y]; }
    public MapInfo() {
        for (int x = 0; x < infos.GetLength(0); ++x) {
            for (int y = 0; y < infos.GetLength(1); ++y) {
                infos[x, y] = new BlockInfo();
            }
        }
    }
    public void Clear() {
        foreach (BlockInfo info in infos) {
            info.Clear();
        }
    }
    public void Clear(int x, int y) {
        infos[x, y].Clear();
    }
    /// <summary>
    /// 给定位置是否可以随机生成地图块 (x, y取值为[0, 12])
    /// </summary>
    public bool RandomAvailable(int x, int y) {
        if (infos[x, y].type != BlockType.NONE) { return false; }
        if (y >= MAX_LENGTH - 1 || y <= 0) { return false; }
        if (y <= 1 && x >= 5 && x <= 7) { return false; }
        return true;
    }
    /// <summary>
    /// 在非生成位置, 非基地位置, 随机一个空地方设置方块; 方块较多时, 可能失败
    /// </summary>
    /// <param name="type">方块类型</param>
    /// <param name="smallBlocks">type为Wall和Steel有效, null表示全false</param>
    public void RandomSet(BlockType type, bool[] smallBlocks) {
        int count = 0;
        while (count < 1000) { // 小心死循环
            ++count;
            int x = Random.Range(0, MAX_LENGTH);
            int y = Random.Range(1, MAX_LENGTH - 1);
            if (RandomAvailable(x, y)) {
                Set(x, y, type, smallBlocks);
                break;
            }
        }
    }
    /// <summary>
    /// 随机获取一个可用的道具生成位置
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RandomBonusPosition(out int x, out int y) {
        while (true) {
            x = Random.Range(0, MAX_LENGTH);
            y = Random.Range(1, MAX_LENGTH - 1);
            if (infos[x, y].type == BlockType.NONE
            || infos[x, y].type == BlockType.GRASS
            || infos[x, y].type == BlockType.WALL) {
                return;
            }
        }
    }
    /// <summary>
    /// 设置一个地图区块信息
    /// </summary>
    /// <param name="x">横坐标</param>
    /// <param name="y">纵坐标</param>
    /// <param name="type">方块类型</param>
    /// <param name="smallBlocks">如果是Wall或者Steel有效, 可以填null, 表示全false</param>
    public void Set(int x, int y, BlockType type, bool[] smallBlocks) {
        infos[x, y].Set(type, smallBlocks);
    }
}

public class BlockInfo {
    public BlockType type = BlockType.NONE;
    public bool[] smallBlocks = new bool[4]; // length == 4!
    public BlockInfo() {}
    public BlockInfo(BlockType type) {
        this.type = type;
    }
    public BlockInfo(BlockType type, bool[] smallBlocks) {
        Set(type, smallBlocks);
    }
    public void Set(BlockType type, bool[] smallBlocks) {
        this.type = type;
        if (smallBlocks != null) {
            for (int i = 0; i < this.smallBlocks.Length; ++i) {
                this.smallBlocks[i] = smallBlocks[i];
            }
        } else {
            for (int i = 0; i < this.smallBlocks.Length; ++i) {
                this.smallBlocks[i] = false;
            }
        }
    }
    public void Clear() {
        this.type = BlockType.NONE;
        for (int i = 0; i < this.smallBlocks.Length; ++i) {
            this.smallBlocks[i] = false;
        }
    }
}

public class MapController : NetworkBehaviour {
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
    private bool[,] existBlocks = new bool[MAP_RADIUS * 2 + 1, MAP_RADIUS * 2 + 1];
    private MapInfo mapInfo = new MapInfo();
    private float homeWallTimer;
    private bool homeWallAnim; // 动画是否播放过
    private void Start() {
        if (isServer) {
            // 事件监听
            Messager.Instance.Listen(MessageID.BONUS_SPAWN, OnMsgSpawnBonus);
            Messager.Instance.Listen(MessageID.GAME_RETRY, OnMsgGameStart);
            Messager.Instance.Listen(MessageID.GAME_START, OnMsgGameStart);
            Messager.Instance.Listen(MessageID.BONUS_SHOVEL_TRIGGER, OnMsgBonusShovel);
        }
    }
    private void Update() {
        if (isServer) {
            HomeWallUpdate();
        }
    }
    [ServerCallback]
    private void HomeWallUpdate() {
        if (homeWallTimer > 0f) {
            homeWallTimer -= Time.deltaTime;
            if (homeWallTimer < 5f && !homeWallAnim) {
                homeWallAnim = true;
                RpcHomeWallChangeAni();
            }
            if (homeWallTimer <= 0f) {
                ClearHomeWall();
                CreateHomeWall(false);
            }
        }
    }
    [ClientRpc]
    private void RpcHomeWallChangeAni() {
        for (int x = 5; x <= 7; ++x) {
            for (int y = 0; y <= 1; ++y) {
                if (x == 6 && y == 0) { continue; }
                GameObject obj = Instantiate(WallChangePrefab, new Vector3(x - 6, y - 6, -1f), Quaternion.identity);
            }
        }
    }
    [ServerCallback]
    private void OnMsgSpawnBonus() {
        int x, y;
        mapInfo.RandomBonusPosition(out x, out y);
        GameObject bonus = Instantiate(BonusPrefab, new Vector3(x - 6, y - 6, 1f), Quaternion.identity);
        NetworkServer.Spawn(bonus);
    }
    [ServerCallback]
    private void OnMsgGameStart() {
        ClearMap();
        GenerateRandomMapInfo();
        CreateMap(mapInfo);
    }
    /// <summary>
    /// 生成随机地图信息
    /// </summary>
    private void GenerateRandomMapInfo() {
        mapInfo.Clear();
        // 基地墙壁
        bool[] smallBlocks = new bool[4] {true, true, true, true};
        for (int x = 5; x <= 7; ++x) {
            for (int y = 0; y <= 1; ++y) {
                mapInfo.Set(x, y, BlockType.WALL, smallBlocks);
            }
        }
        // 基地
        mapInfo.Set(6, 0, BlockType.HOME, null);
        // 随机地形
        // 土墙
        int num = Random.Range(20, 40);
        for (int i = 0; i < num; ++i) {
            mapInfo.RandomSet(BlockType.WALL, smallBlocks);
        }
        // 铁
        num = Random.Range(7, 15);
        for (int i = 0; i < num; ++i) {
            mapInfo.RandomSet(BlockType.STEEL, smallBlocks);
        }
        // 水
        num = Random.Range(5, 10);
        for (int i = 0; i < 5; ++i) {
            mapInfo.RandomSet(BlockType.WATER, null);
        }
        // 草
        num = Random.Range(5, 15);
        for (int i = 0; i < 10; ++i) {
            mapInfo.RandomSet(BlockType.GRASS, null);
        }
    }

    /// <summary>
    /// 根据地图信息生成地图
    /// </summary>
    public void CreateMap(MapInfo info) {
        for (int i = 0; i < MapInfo.MAX_LENGTH; ++i) {
            for (int j = 0; j < MapInfo.MAX_LENGTH; ++j) {
                CreateBlock(i, j, info.GetBlock(i, j));
            }
        }
    }
    private void CreateBlock(int x, int y, BlockInfo info) {
        if (info.type == BlockType.NONE) { return; }
        GameObject obj = null;
        Vector3 position = new Vector3(x - 6, y - 6, 0f);
        switch (info.type) {
            case BlockType.GRASS:
                obj = Instantiate(GrassPrefab, position, Quaternion.identity);
                break;
            case BlockType.WALL:
                obj = Instantiate(WallPrefab, position, Quaternion.identity);
                obj.GetComponent<Block>().Set(x, y, info.smallBlocks, OnBlockDestroy);
                break;
            case BlockType.STEEL:
                obj = Instantiate(SteelPrefab, position, Quaternion.identity);
                obj.GetComponent<Block>().Set(x, y, info.smallBlocks, OnBlockDestroy);
                break;
            case BlockType.WATER:
                obj = Instantiate(WaterPrefab, position, Quaternion.identity);
                break;
            case BlockType.HOME:
                obj = Instantiate(HomePrefab, position, Quaternion.identity);
                break;
            default:
                break;
        }
        if (obj != null) { NetworkServer.Spawn(obj); }
    }
    private void OnBlockDestroy(int x, int y) {
        mapInfo.Clear(x, y);
    }
    [ServerCallback]
    private void OnMsgBonusShovel() {
        ClearHomeWall();
        CreateHomeWall(true);
        homeWallTimer = 40f;
        homeWallAnim = false;
    }
    private void ClearHomeWall() {
        // 清空home周围的墙壁
        Collider2D[] colliders = Physics2D.OverlapBoxAll(new Vector2(0f, -5.5f), new Vector2(2.8f, 1.8f), 0f);
        foreach (Collider2D collider in colliders) {
            if (collider.transform.parent != null && collider.transform.parent.tag == "Map") {
                NetworkServer.Destroy(collider.transform.parent.gameObject);
            }
        }
    }
    public void ClearMap() {
        // 清理地图
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Map");
        foreach(GameObject block in blocks) {
            NetworkServer.Destroy(block);
        }
        // 清理老家
        GameObject[] homes = GameObject.FindGameObjectsWithTag("Home");
        foreach(GameObject home in homes) {
            NetworkServer.Destroy(home);
        }
        // 清理道具
        GameObject[] bonuses = GameObject.FindGameObjectsWithTag("Bonus");
        foreach(GameObject bonus in bonuses) {
            NetworkServer.Destroy(bonus);
        }
        // 重置地图信息
        mapInfo.Clear();
        // 基地墙壁
        homeWallTimer = 0f;
        homeWallAnim = true;
    }
    private void CreateHomeWall(bool isSteel) {
        GameObject pref = isSteel ? SteelPrefab : WallPrefab;
        bool[] smallBlocks = new bool[] { true, true, true, true };
        for (int x = 5; x <= 7; ++x) {
            for (int y = 0; y <= 1; ++y) {
                if (x == 6 && y == 0) { continue; }
                GameObject obj = Instantiate(pref, new Vector3(x - 6, y - 6, 0), Quaternion.identity);
                obj.GetComponent<Block>().Set(x, y, smallBlocks, OnBlockDestroy);
                NetworkServer.Spawn(obj);
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
}
