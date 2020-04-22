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
    private void Start() {
        GameController.Instance.AddListener(MsgID.BONUS_SPAWN, OnMsgSpawnBonus);
        GameController.Instance.AddListener(MsgID.GAME_RETRY, OnMsgGameRetry);
    }
    private void OnMsgSpawnBonus(Msg msg) {
        float x = (float) Random.Range(-MAP_RADIUS + 1, MAP_RADIUS - 1);
        float y = (float) Random.Range(-MAP_RADIUS + 2, MAP_RADIUS - 1);
        Instantiate(BonusPrefab, new Vector3(x, y, 1f), Quaternion.identity);
    }
    private void OnMsgGameRetry(Msg msg) {
        ClearMap();
        // todo create map
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
    }
    /// <summary>
    /// 随机生成地图
    /// </summary>
    public void CreateMap() {
        // todo
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
