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
            if (value < 0) { level = 0; }
            else if (value > 2) { level = 2; }
            else { level = value; }
            if (level > 0) { speed *= 1.5f; }
        }
    }
    // ----------------------------------------------------------------
    public AudioClip fireAudio;
    public AudioClip hitAudio;
    public AudioClip heartAudio;
    public Transform explosionPoint;
    public GameObject explosionPrefab;
    private int level;
    /// <summary>
    /// 子弹最大存在时长
    /// </summary>
    public float maxLife = 3.0f;
    public float speed = 6.0f; // 子弹移动速度, 随等级变化
    public const int wallDestroy = 2; // 每次拆一半的墙(2小块)
    #endregion

    #region lifecall
    private void Start()
    {
        AudioSource.PlayClipAtPoint(fireAudio, transform.position);
        Destroy(gameObject, maxLife);
    }
    private void FixedUpdate() {
        transform.Translate(transform.up * speed * Time.fixedDeltaTime, Space.World);
    }
    private void OnTriggerEnter2D(Collider2D other) {
        switch (other.tag) {
            case "Barrier":
                AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                Destroy(gameObject);
                break;
            case "Player":
                if (!isPlayerBullet) {
                    Player player = other.GetComponent<Player>();
                    if (!player.TakeDamage()) {
                        AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                    } else {
                        Instantiate(explosionPrefab, other.transform.position, other.transform.rotation);
                    }
                    Destroy(gameObject);
                }
                break;
            case "Enemy":
                if (isPlayerBullet) {
                    Enemy enemy = other.GetComponent<Enemy>();
                    if (!enemy.TakeDamage()) {
                        AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                    } else {
                        Instantiate(explosionPrefab, other.transform.position, other.transform.rotation);
                    }
                    Destroy(gameObject);
                }
                break;
            case "Wall":
                AudioSource.PlayClipAtPoint(heartAudio, transform.position); // todo 合适音效
                DistroyWall("Wall");
                Destroy(gameObject);
                break;
            case "Steel":
                AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                if (Level >= 2) {
                    DistroyWall("Steel");
                }
                Destroy(gameObject);
                break;
            case "Bullet":
                AudioSource.PlayClipAtPoint(hitAudio, transform.position);
                Destroy(other.gameObject);
                Destroy(gameObject);
                break;
            case "Home":
                if (other.GetComponent<Home>().TakeDamage()) {
                    Instantiate(explosionPrefab, other.transform.position, other.transform.rotation);
                }
                Destroy(gameObject);
                break;
        }
    }
    #endregion
    #region customfunc
    private void DistroyWall(string tag) {// 拆墙
        Vector2 centerPoint = new Vector2(explosionPoint.position.x, explosionPoint.position.y);
        Vector2 size = new Vector2(0.51f, 0.1f);
        float angle = transform.rotation.eulerAngles.z;
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = explosionPoint.position;
        cube.transform.localScale = new Vector3(0.51f, 0.1f, 1.0f);
        cube.transform.rotation = transform.rotation;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(centerPoint, size, angle);
        int count = 0;
        for (int i = 0; i < colliders.Length; ++i) {
            if (colliders[i].tag == tag) {
                Destroy(colliders[i].gameObject);
                ++count;
                if (count >= wallDestroy) { break; }
            }
        }
    }
    public void Set(bool isPlayerBullet, int level = 0) {
        this.isPlayerBullet = isPlayerBullet;
        this.Level = level;
    }
    #endregion
}
