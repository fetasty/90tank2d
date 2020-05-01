using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
public class Block : NetworkBehaviour
{
    public GameObject smallBlockPrefab;
    /// <summary>
    /// 一个大的墙由四小块组成, 每一块都有下标
    /// 0 1
    /// 2 3
    /// </summary>
    /// <value>对应编号位置是否存在小方块</value>
    private bool[] parts = new bool[4] {true, true, true, true};
    private Action<int, int> destroyCallback;
    public int x;
    public int y;
    private void Start() {
        transform.parent = GameObject.Find("/Maps").transform;
        if (isServer) {
            for (int i = 0; i < 4; ++i) {
                if (parts[i]) {
                    float x = (i % 2 == 0) ? -0.25f : 0.25f;
                    float y = (i > 1) ? -0.25f : 0.25f;
                    GameObject part = Instantiate(smallBlockPrefab,
                    transform.position + new Vector3(x, y, 0f), Quaternion.identity);
                    SmallBlock small = part.GetComponent<SmallBlock>();
                    small.Set(i, OnPartDestroy);
                    NetworkServer.Spawn(part);
                    small.RpcSetParent(transform);
                }
            }
        }
    }
    [ServerCallback]
    private void OnDestroy() {
        if (destroyCallback != null) {
            destroyCallback(x, y);
        }
    }
    [ServerCallback]
    private void OnPartDestroy(int index) {
        parts[index] = false;
        bool isEmpty = true;
        for (int i = 0; i < 4; ++i) {
            if (parts[i]) {
                isEmpty = false;
                break;
            }
        }
        if (isEmpty) {
            NetworkServer.Destroy(gameObject);
        }
    }
    /// <summary>
    /// 设置墙壁的初始情况
    /// </summary>
    /// <param name="parts">墙壁每个位置的小block是否存在, 长度为4, 不要全初始化为false</param>
    /// <param name="destroyCallback">被完全摧毁的回调, 参数为墙体位置</param>
    [ServerCallback]
    public void Set(int x, int y, bool[] parts, Action<int, int> destroyCallback = null) {
        this.x = x;
        this.y = y;
        for (int i = 0; i < 4; ++i) { this.parts[i] = parts[i]; }
        this.destroyCallback = destroyCallback;
    }
    
}
