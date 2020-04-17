using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AudioClip idleAudio;
    public AudioClip drivingAudio;
    public GameObject shield;
    public GameObject bulletPrefab;
    public float moveSpeed = 2.0f;
    public float bornShieldTime = 5.0f; // 出身的护盾时间
    public float bonusShieldTime = 10.0f; // 护盾道具时间
    public float fillTime = 1.0f; // 子弹填充时间
    public int bulletCapacity = 1; // 容弹量
    public float fireDuration = 0.2f; // 两发子弹的最小时间间隔
    public const int MAX_LEVEL = 3;
    public const int MIN_LEVEL = 0;
    private int type = 0; // 0, 1p样式; 1, 2p样式
    private int level = 0; // 使用属性修改器修改等级
    private Animator animator;
    private AudioSource audioSource;
    private bool isInShield;
    private float shieldTime; // 剩余的护盾时间
    private int bulletNum; //当前存弹量
    private float fireTimer; // 开火cd
    private float fillTimer; // 填充cd
    private Action<int> dieAction;
    private int playerID; // 0 = 1p, 1 = 2p
    public int PlayerID {
        get { return playerID; }
        set {
            if (value < 0) { playerID = 0; }
            else if (value > 3) { playerID = 3; }
            if (animator != null) { animator.SetInteger("type", playerID); }
        }
    }
    public int Level {
        get { return level; }
        set {
            if (value >= MIN_LEVEL && value <= MAX_LEVEL) {
                this.level = value;
                if (animator != null) { animator.SetInteger("level", value); }
            }
        }
    }
    public int Type {
        get { return type; }
        set {
            if (value == 0) { this.type = 0; }
            else { this.type = 1; }
            animator.SetInteger("type", this.type);
        }
    }
    private int BulletLevel {
        get {
            if (level < 1) {
                return 0;
            } else if (level < 3) {
                return 1;
            } else {
                return 2;
            }
        }
    }
    private int BulletCapacity {
        get {
            if (Level < 2) { return bulletCapacity; }
            else { return bulletCapacity * 2; }
        }
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        SetShield(bornShieldTime);
        bulletNum = BulletCapacity;
        animator.SetInteger("type", PlayerID);
        animator.SetInteger("level", Level);
    }
    private void Update() {
        ShieldUpdate();
        FireUpdate();
    }
    private void FixedUpdate() {
        Move();
    }
    /// <summary>
    /// 玩家中弹, 返回玩家受伤后是否死亡
    /// </summary>
    /// <returns>受伤是否死亡, true, 死亡; false, 未死亡</returns>
    public bool TakeDamage() {
        if (GameController.Current.IsGameOver) { return false; }
        if (isInShield) { return false; }
        if (Level >= 2) {
            Level -= 2;
            return false;
        } else {
            Destroy(gameObject);
            if (dieAction != null) {
                dieAction(PlayerID);
            }
            return true;
        }
    }
    public void LevelUp() {
        ++Level;
    }
    public void SetShield(float time) {
        if (time < 0.0f) { return; }
        shieldTime = time;
        isInShield = true;
        shield.SetActive(true);
    }
    private void CloseShield() {
        shieldTime = 0.0f;
        isInShield = false;
        shield.SetActive(false);
    }
    private void Move() {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float hAbs = Mathf.Abs(h);
        float vAbs = Mathf.Abs(v);
        bool isMove = true;
        float rotationAngle = 0.0f;
        if (hAbs > vAbs && hAbs > 0.01f) {
            v = 0.0f;
            if (h > 0.01f) { rotationAngle = -90.0f; }
            else if (h < -0.01f) { rotationAngle = 90.0f; }
        } else if (vAbs > hAbs && vAbs > 0.01f) {
            h = 0.0f;
            if (v > 0.01f) { rotationAngle = 0.0f; }
            else if (v < -0.01f) { rotationAngle = 180.0f; }
        } else { isMove = false; }
        if (isMove) {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationAngle);
            transform.Translate(new Vector3(h, v, 0.0f) * moveSpeed * Time.fixedDeltaTime, Space.World);
        }
        audioSource.clip = isMove ? drivingAudio : idleAudio;
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }
    private void ShieldUpdate() {
        if (isInShield) {
            shieldTime -= Time.deltaTime;
            if (shieldTime <= 0.0f) {
                CloseShield();
            }
        }
    }
    private void FireUpdate() {
        bool allowFire = true;
        if (fireTimer > 0.0f) {
            allowFire = false;
            fireTimer -= Time.deltaTime;
        }
        if (fillTimer > 0.0f) {
            fillTimer -= Time.deltaTime;
        }
        if (fillTimer <= 0.0f && bulletNum < BulletCapacity) {
            bulletNum = BulletCapacity;
        }
        // 按了开火键
        if (allowFire && Input.GetAxisRaw("Fire1") > 0.01f) {
            // 是否有子弹
            if (bulletNum > 0) {
                GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
                Bullet bullet = obj.GetComponent<Bullet>();
                bullet.Set(true, BulletLevel);
                --bulletNum;
                fireTimer = fireDuration;
                if (fillTimer <= 0.0f) {
                    fillTimer = fillTime;
                }
            }
        }
    }
    public void Set(int id, System.Action<int> dieAction) {
        PlayerID = id;
        this.dieAction = dieAction;
        // this.Level = 3; // todo debug
    }
}
