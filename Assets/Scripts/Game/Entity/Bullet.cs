using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour {
    #region fields
    // ------------生成子弹需要设置的变量-----------
    /// <summary>
    /// 是否为玩家的子弹, 防止互相伤害
    /// </summary>
    [SyncVar]
    public bool isPlayerBullet;
    // ----------------------------------------------------------------
    public Transform explosionPoint;
    /// <summary>
    /// 子弹等级, 默认0级, 1级速度比较快, 2级速度快且可以破坏铁墙
    /// </summary>
    [SyncVar(hook = nameof(LevelChange))]
    private int level;
    private void LevelChange(int _, int newLevel) {
        if (newLevel > 0) { speed *= 1.5f; }
    }
    /// <summary>
    /// 子弹最大存在时长
    /// </summary>
    public float maxLife = 3.0f;
    public float speed = 8.0f; // 子弹移动速度, 随等级变化
    private float lifeTimer;
    private bool destroyFlag; // 删除标记
    #endregion

    #region lifecall
    private void Start()
    {
        GameObject bullets = GameObject.Find("/Bullets");
        if (bullets != null) { transform.parent = bullets.transform; }
        if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.FIRE); }
        if (isServer) {
            lifeTimer = maxLife;
            destroyFlag = false;
        }
    }
    [ServerCallback]
    private void Update() {
        DestroyUpdate();
        if (GameData.isGamePausing && isServer) { return; }
        LifeUpdate();
    }
    [ServerCallback]
    private void FixedUpdate() {
        if (GameData.isGamePausing && isServer) { return; }
        transform.Translate(transform.up * speed * Time.fixedDeltaTime, Space.World);
    }
    [ServerCallback]
    private void DestroyUpdate() {
        if (destroyFlag) {
            NetworkServer.Destroy(gameObject);
        }
    }
    [ServerCallback]
    private void LifeUpdate() {
        if (lifeTimer > 0f) {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f) {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
    [ClientRpc]
    private void RpcPlayAudio(EffectAudio type) {
        Debug.Log($"RpcPlayAudio {type}");
        AudioController.Cur.PlayEffect(type);
    }
    // todo 网络行为处理, 广播客户端 (做不到)
    // host提示rpc函数在no-spawned物体上调用, 可能是因为调用RPC之后立即调用了Destroy导致, 但是不调用Destroy, 可能多次进入检测
    // 用标志位, host上还是听不到声音, 只是消除了警告
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other) {
        if (destroyFlag) { return; }
        switch (other.tag) {
            case "Barrier":
                    if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.HIT); }
                    RpcPlayAudio(EffectAudio.HIT);
                    destroyFlag = true;
                break;
            case "Player":
                if (!GameData.isGamePlaying || !GameData.isInGameLevel) { return; }
                if (!isPlayerBullet) {
                    Player player = other.GetComponent<Player>();
                    if (!player.TakeDamage()) {
                        if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.HIT); }
                        RpcPlayAudio(EffectAudio.HIT);
                    }
                    destroyFlag = true;
                }
                break;
            case "Enemy":
                if (!GameData.isGamePlaying || !GameData.isInGameLevel) { return; }
                if (isPlayerBullet) {
                    Enemy enemy = other.GetComponent<Enemy>();
                    if (!enemy.TakeDamage()) {
                        if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.HIT); }
                        RpcPlayAudio(EffectAudio.HIT);
                    }
                    destroyFlag = true;
                }
                break;
            case "Wall":
                if (!GameData.isGamePlaying || !GameData.isInGameLevel) { return; }
                if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.DESTROY); }
                RpcPlayAudio(EffectAudio.DESTROY);
                DistroyWall("Wall");
                destroyFlag = true;
                break;
            case "Steel":
                if (!GameData.isGamePlaying || !GameData.isInGameLevel) { return; }
                if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.HIT); }
                RpcPlayAudio(EffectAudio.HIT);
                if (level >= 2) {
                    DistroyWall("Steel");
                }
                destroyFlag = true;
                break;
            case "Bullet":
                if (!GameData.isGamePlaying || !GameData.isInGameLevel) { return; }
                if (isPlayerBullet == other.GetComponent<Bullet>().isPlayerBullet) { return; }
                if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.HIT); }
                RpcPlayAudio(EffectAudio.HIT);
                destroyFlag = true;
                break;
            case "Home":
                if (!GameData.isGamePlaying || !GameData.isInGameLevel) { return; }
                if (!other.GetComponent<Home>().TakeDamage()) {
                    if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.HIT); }
                    RpcPlayAudio(EffectAudio.HIT);
                }
                destroyFlag = true;
                break;
        }
    }
    #endregion
    #region customfunc
    [ServerCallback]
    private void DistroyWall(string tag) {// 拆墙
        Vector2 centerPoint = new Vector2(explosionPoint.position.x, explosionPoint.position.y);
        Vector2 size = new Vector2(0.52f, 0.3f);
        float angle = transform.rotation.eulerAngles.z;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(centerPoint, size, angle);
        int count = 0;
        for (int i = colliders.Length - 1; i >= 0; --i) {
            if (colliders[i].tag == tag) {
                NetworkServer.Destroy(colliders[i].gameObject);
                ++count;
                if (count >= 2) { break; }
            }
        }
    }
    public void Set(bool isPlayerBullet, int level) {
        this.isPlayerBullet = isPlayerBullet;
        this.level = level;
    }
    #endregion
}
