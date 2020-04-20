using System;
using UnityEngine;

public class Player : MonoBehaviour {
    public const int MIN_ID = 0;
    public const int MAX_ID = 3;
    public const int MIN_LEVEL = 0;
    public const int MAX_LEVEL = 3;
    public int ID { get; private set; }     // 与出生点位置对应, 保持唯一性

    public AudioClip idleAudio;
    public AudioClip drivingAudio;
    public GameObject shield;
    public GameObject bulletPrefab;
    public GameObject explosionPrefab;

    public float bornShieldTime = 5f;       // 出生的护盾时间
    public float fireDuration = 0.2f;       // 两发子弹的最小时间间隔

    public float initMoveSpeed = 3f;        // 初始移动速度
    public float initFillTime = 1f;         // 初始子弹填充时间
    public int initBulletCapacity = 1;      // 初始容弹量
    public float bonusShieldTime = 15f;     // 道具护盾时间

    private Animator animator;
    private AudioSource audioSource;
    private int level = 0;                  // 玩家等级
    /// <summary>
    /// 玩家等级
    /// </summary>
    public int Level {
        get { return level; }
        set {
            if (value >= MIN_LEVEL && value <= MAX_LEVEL) {
                level = value;
                animator.SetInteger("level", value);
            }
        }
    }
    /// <summary>
    /// 剩余护盾时间
    /// </summary>
    public float ShieldTimer { get; private set; }
    /// <summary>
    /// 当前剩余子弹数量
    /// </summary>
    public int BulletCount { get; private set; }
    /// <summary>
    /// 填充计时器
    /// </summary>
    public float FillTimer { get; private set; }
    /// <summary>
    /// 开火计时器
    /// </summary>
    public float FireTimer { get; private set; }
    /// <summary>
    /// 玩家子弹等级
    /// </summary>
    public int BulletLevel {
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
    /// <summary>
    /// 玩家容弹量
    /// </summary>
    public int BulletCapacity {
        get {
            if (Level < 2) { return initBulletCapacity; }
            else { return initBulletCapacity * 2; }
        }
    }
    void Start() {
        GameController.Instance.PostMsg(new Msg(MsgID.PLAYER_BORN, ID));
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        animator.SetInteger("type", ID);
        animator.SetInteger("level", Level);
        SetShield(bornShieldTime);
        BulletCount = BulletCapacity;
        GameController.Instance.AddListener(MsgID.BONUS_LEVEL_TRIGGER, OnMsgLevelUp);
        GameController.Instance.AddListener(MsgID.BONUS_SHIELD_TRIGGER, OnMsgShield);
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
        if (ShieldTimer > 0f) { return false; }
        if (Level >= 2) {
            Level -= 2;
            return false;
        } else {
            GameController.Instance.PostMsg(new Msg(MsgID.PLAYER_DIE, ID));
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return true;
        }
    }
    public void LevelUp() {
        ++Level;
    }
    public void SetShield(float time) {
        if (time < 0.0f) { return; }
        ShieldTimer = time;
        shield.SetActive(true);
    }
    private void CloseShield() {
        ShieldTimer = 0.0f;
        shield.SetActive(false);
    }
    private void Move() {
        // todo 操作优化
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
            transform.Translate(new Vector3(h, v, 0.0f) * initMoveSpeed * Time.fixedDeltaTime, Space.World);
        }
        audioSource.clip = isMove ? drivingAudio : idleAudio;
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }
    private void ShieldUpdate() {
        if (ShieldTimer > 0f) {
            ShieldTimer -= Time.deltaTime;
            if (ShieldTimer <= 0.0f) {
                CloseShield();
            }
        }
    }
    private void FireUpdate() {
        bool allowFire = true;
        if (FireTimer > 0.0f) {
            allowFire = false;
            FireTimer -= Time.deltaTime;
        }
        if (FillTimer > 0.0f) {
            FillTimer -= Time.deltaTime;
        }
        if (FillTimer <= 0.0f && BulletCount < BulletCapacity) {
            BulletCount = BulletCapacity;
        }
        // 按了开火键
        if (allowFire && Input.GetAxisRaw("Fire1") > 0.01f) {
            // 是否有子弹
            if (BulletCount > 0) {
                GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
                Bullet bullet = obj.GetComponent<Bullet>();
                bullet.Set(true, BulletLevel);
                --BulletCount;
                FireTimer = fireDuration;
                if (FillTimer <= 0.0f) {
                    FillTimer = initFillTime;
                }
            }
        }
    }
    /// <summary>
    /// Start之前调用
    /// </summary>
    /// <param name="playerID">唯一标识</param>
    public void Set(int playerID) {
        ID = playerID;
    }
    public void OnMsgLevelUp(Msg msg) {
        if (ID == (int) msg.Param) {
            LevelUp();
        }
    }
    public void OnMsgShield(Msg msg) {
        if (ID == (int) msg.Param) {
            SetShield(bonusShieldTime);
        }
    }
}
