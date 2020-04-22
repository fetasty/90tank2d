using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region fields
    // ------------生成子弹需要设置的变量-----------
    /// <summary>
    /// 是否为玩家的子弹, 防止互相伤害
    /// </summary>
    public bool isPlayerBullet;
    /// <summary>
    /// 子弹等级, 默认0级, 1级速度比较快, 2级速度快且可以破坏铁墙
    /// </summary>
    /// <value>整数, 取值范围[0, 2]</value>
    public int Level {
        get { return level; }
        set {
            if (value >= 0 && value <= 2) { level = value; }
            if (level > 0) { speed *= 1.5f; }
        }
    }
    // ----------------------------------------------------------------
    public AudioClip fireAudio;
    public AudioClip hitAudio;
    public AudioClip heartAudio;
    public Transform explosionPoint;
    private int level;
    /// <summary>
    /// 子弹最大存在时长
    /// </summary>
    public float maxLife = 3.0f;
    public float speed = 8.0f; // 子弹移动速度, 随等级变化
    private GameInfoManager info;
    private float lifeTimer;
    #endregion

    #region lifecall
    private void Start()
    {
        GameObject bullets = GameObject.Find("/Bullets");
        if (bullets != null) { transform.parent = bullets.transform; }
        info = GameController.Instance.InfoManager;
        AudioSource.PlayClipAtPoint(fireAudio, transform.position);
        lifeTimer = maxLife;
    }
    private void Update() {
        if (info.IsGamePause) { return; }
        LifeUpdate();
    }
    private void FixedUpdate() {
        if (info.IsGamePause) { return; }
        transform.Translate(transform.up * speed * Time.fixedDeltaTime, Space.World);
    }
    private void LifeUpdate() {
        if (lifeTimer > 0f) {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f) {
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        switch (other.tag) {
            case "Barrier":
                Destroy(gameObject);
                break;
            case "Player":
                if (!info.IsGamePlaying) { return; }
                if (!isPlayerBullet) {
                    Player player = other.GetComponent<Player>();
                    if (!player.TakeDamage()) {
                        AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                    }
                    Destroy(gameObject);
                }
                break;
            case "Enemy":
                if (!info.IsGamePlaying) { return; }
                if (isPlayerBullet) {
                    Enemy enemy = other.GetComponent<Enemy>();
                    if (!enemy.TakeDamage()) {
                        AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                    }
                    Destroy(gameObject);
                }
                break;
            case "Wall":
                if (!info.IsGamePlaying) { return; }
                AudioSource.PlayClipAtPoint(heartAudio, transform.position); // todo 合适音效
                DistroyWall("Wall");
                Destroy(gameObject);
                break;
            case "Steel":
                if (!info.IsGamePlaying) { return; }
                AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                if (Level >= 2) {
                    DistroyWall("Steel");
                }
                Destroy(gameObject);
                break;
            case "Bullet":
                if (!info.IsGamePlaying) { return; }
                if (isPlayerBullet == other.GetComponent<Bullet>().isPlayerBullet) { return; }
                AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                Destroy(other.gameObject);
                Destroy(gameObject);
                break;
            case "Home":
                if (!info.IsGamePlaying) { return; }
                if (!other.GetComponent<Home>().TakeDamage()) {
                    AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                }
                Destroy(gameObject);
                break;
        }
    }
    #endregion
    #region customfunc
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
    public void Set(bool isPlayerBullet, int level = 0) {
        this.isPlayerBullet = isPlayerBullet;
        this.Level = level;
    }
    #endregion
}
