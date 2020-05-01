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
    public AudioClip[] audios;  // fire, hit, heart
    public AudioClip hitAudio;
    public AudioClip heartAudio;
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
    #endregion

    #region lifecall
    private void Start()
    {
        GameObject bullets = GameObject.Find("/Bullets");
        if (bullets != null) { transform.parent = bullets.transform; }
        AudioSource.PlayClipAtPoint(audios[0], transform.position);
        if (isServer) {
            lifeTimer = maxLife;
        }
    }
    [ServerCallback]
    private void Update() {
        if (GameData.isGamePausing) { return; }
        LifeUpdate();
    }
    [ServerCallback]
    private void FixedUpdate() {
        if (GameData.isGamePausing) { return; }
        transform.Translate(transform.up * speed * Time.fixedDeltaTime, Space.World);
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
    // 网络行为处理, 广播客户端
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other) {
        switch (other.tag) {
            case "Barrier":
                NetworkServer.Destroy(gameObject);
                break;
            case "Player":
                if (!GameData.isGamePlaying) { return; }
                if (!isPlayerBullet) {
                    Player player = other.GetComponent<Player>();
                    if (!player.TakeDamage()) {
                        RpcPlayAudio(1);
                    }
                    NetworkServer.Destroy(gameObject);
                }
                break;
            case "Enemy":
                if (!GameData.isGamePlaying) { return; }
                if (isPlayerBullet) {
                    Enemy enemy = other.GetComponent<Enemy>();
                    if (!enemy.TakeDamage()) {
                        RpcPlayAudio(1);
                    }
                    NetworkServer.Destroy(gameObject);
                }
                break;
            // todo ---
            case "Wall":
                if (!GameData.isGamePlaying) { return; }
                AudioSource.PlayClipAtPoint(heartAudio, transform.position); // todo 合适音效
                DistroyWall("Wall");
                Destroy(gameObject);
                break;
            case "Steel":
                if (!GameData.isGamePlaying) { return; }
                AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                if (level >= 2) {
                    DistroyWall("Steel");
                }
                Destroy(gameObject);
                break;
            case "Bullet":
                if (!GameData.isGamePlaying) { return; }
                if (isPlayerBullet == other.GetComponent<Bullet>().isPlayerBullet) { return; }
                AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                Destroy(other.gameObject);
                Destroy(gameObject);
                break;
            case "Home":
                if (!GameData.isGamePlaying) { return; }
                if (!other.GetComponent<Home>().TakeDamage()) {
                    AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                }
                Destroy(gameObject);
                break;
        }
    }
    [ClientRpc]
    private void RpcPlayAudio(int index) {
        AudioSource.PlayClipAtPoint(audios[index], transform.position);
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
                Destroy(colliders[i].gameObject);
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
