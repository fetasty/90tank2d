using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class SmallBlock : NetworkBehaviour
{
    private int index; // 在父节点中的位置 [0, 3]
    private Action<int> destroyCallback;
    [ServerCallback]
    public void Set(int x, Action<int> destroyCallback) {
        this.index = x;
        this.destroyCallback = destroyCallback;
    }
    [ServerCallback]
    private void OnDestroy() {
        destroyCallback(index);
    }
    [ClientRpc]
    public void RpcSetParent(Transform parent) {
        transform.parent = parent;
    }
}
