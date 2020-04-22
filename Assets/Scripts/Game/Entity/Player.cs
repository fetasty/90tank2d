using System;
using UnityEngine;
using UnityEngine.UI;

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
    private bool isMobileFireDown;          // 虚拟开火按钮是否按下
    private Vector2 mobileInput;            // 虚拟摇杆输入

    public float bornShieldTime = 5f;       // 出生的护盾时间
    public float fireDuration = 0.1f;       // 两发子弹的最小时间间隔

    public float initMoveSpeed = 2f;        // 初始移动速度
    public float initFillTime = 0.8f;         // 初始子弹填充时间
    public int initBulletCapacity = 1;      // 初始容弹量
    public float bonusShieldTime = 15f;     // 道具护盾时间
    private bool horizontalInputLast;        // 最后的轴向输入是否为水平方向 (移动优化)

    private Animator animator;
    private AudioSource audioSource;
    private int level = 0;                  // 玩家等级
    private GameInfoManager info;
    public float FillTime {
        get {
            if (level < 1) { return initFillTime; }
            if (level < 2) { return initFillTime - 0.1f; }
            return initFillTime - 0.2f;
        }
    }
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
    public float MoveSpeed {
        get {
            if (level < 1) {
                return initMoveSpeed;
            }
            return initMoveSpeed + 1f;
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
        GameObject tanks = GameObject.Find("/Tanks");
        if (tanks != null) {
            transform.parent = tanks.transform;
        }
        GameController.Instance.PostMsg(new Msg(MsgID.PLAYER_BORN, ID));
        info = GameController.Instance.InfoManager;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        animator.SetInteger("type", ID);
        animator.SetInteger("level", Level);
        SetShield(bornShieldTime);
        BulletCount = BulletCapacity;
        GameController.Instance.AddListener(MsgID.BONUS_LEVEL_TRIGGER, OnMsgLevelUp);
        GameController.Instance.AddListener(MsgID.BONUS_SHIELD_TRIGGER, OnMsgShield);
        GameController.Instance.AddListener(MsgID.MOBILE_MOVE_INPUT, OnMsgMobileMove);
        GameController.Instance.AddListener(MsgID.MOBILE_FIRE_INPUT, OnMsgMobileFire);
    }
    private void OnDestroy() {
        // 游戏中会动态销毁的实例, 必须在销毁时注销监听
        GameController.Instance.RemoveListener(MsgID.BONUS_LEVEL_TRIGGER, OnMsgLevelUp);
        GameController.Instance.RemoveListener(MsgID.BONUS_SHIELD_TRIGGER, OnMsgShield);
        GameController.Instance.RemoveListener(MsgID.MOBILE_MOVE_INPUT, OnMsgMobileMove);
        GameController.Instance.RemoveListener(MsgID.MOBILE_FIRE_INPUT, OnMsgMobileFire);
    }
    private void Update() {
        if (info.IsGamePause) { return; }
        ShieldUpdate();
        FireUpdate();
    }
    private void FixedUpdate() {
        if (info.IsGamePause) { return; }
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
        if (Level >= MAX_LEVEL) { return; }
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
    private Vector2 GetMoveInput() {
        Vector2 result = Vector2.zero;
        // 本地双人
        if (Global.Instance.SelectedGameMode == GameMode.DOUBLE) {
            if (ID == 0) {
                result.x = Input.GetAxisRaw("Horizontal");
                result.y = Input.GetAxisRaw("Vertical");
            } else {
                result.x = Input.GetAxisRaw("Horizontal2");
                result.y = Input.GetAxisRaw("Vertical2");
            }
            return result;
        }
        // 移动平台
        if (Global.Instance.IsMobile) {
            return mobileInput;
        }
        // win平台
        result.x = Input.GetAxisRaw("Horizontal");
        result.y = Input.GetAxisRaw("Vertical");
        return result;
    }
    private void Move() {
        Vector2 moveInput = GetMoveInput();
        float h = moveInput.x;
        float v = moveInput.y;
        float hAbs = Mathf.Abs(h);
        float vAbs = Mathf.Abs(v);
        bool isMove = true;
        float rotationAngle = 0f;
        if (hAbs > vAbs) { // h
            v = 0f;
            if (h > 0f) { rotationAngle = -90f; }
            else if (h < 0f) { rotationAngle = 90f; }
            horizontalInputLast = true;
        } else if (vAbs > hAbs) { // v
            h = 0f;
            if (v > 0f) { rotationAngle = 0f; }
            else if (v < 0f) { rotationAngle = 180f; }
            horizontalInputLast = false;
        } else if (hAbs > 0f) { // h + v
            // 同时按住了水平移动与垂直移动
            if (!horizontalInputLast) {
                v = 0f;
                if (h > 0f) { rotationAngle = -90f; }
                else if (h < 0f) { rotationAngle = 90f; }
            } else {
                h = 0f;
                if (v > 0f) { rotationAngle = 0f; }
                else if (v < 0f) { rotationAngle = 180f; }
            }
        } else { 
            isMove = false;
        }
        if (isMove) {
            transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);
            transform.Translate(new Vector3(h, v, 0f).normalized * MoveSpeed * Time.fixedDeltaTime, Space.World);
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
    private bool FireInput() {
        if (Global.Instance.SelectedGameMode == GameMode.DOUBLE) {
            if (ID == 0) {
                return Input.GetAxisRaw("Fire1") > 0f;
            }
            return Input.GetAxisRaw("Fire2") > 0f;
        }
        if (Global.Instance.IsMobile) {
            return isMobileFireDown;
        }
        return Input.GetAxisRaw("Fire1") > 0f;
    }
    private void FireUpdate() {
        bool allowFire = true;
        if (FireTimer > 0f) {
            allowFire = false;
            FireTimer -= Time.deltaTime;
        }
        if (FillTimer > 0f) {
            FillTimer -= Time.deltaTime;
        }
        if (FillTimer <= 0f && BulletCount < BulletCapacity) {
            BulletCount = BulletCapacity;
        }
        // 按了开火键
        if (allowFire && FireInput()) {
            // 是否有子弹
            if (BulletCount > 0) {
                GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
                Bullet bullet = obj.GetComponent<Bullet>();
                bullet.Set(true, BulletLevel);
                --BulletCount;
                FireTimer = fireDuration;
                if (FillTimer <= 0.0f) {
                    FillTimer = FillTime;
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
    public void OnMsgMobileMove(Msg msg) {
        mobileInput = (Vector2) msg.Param;
    }
    public void OnMsgMobileFire(Msg msg) {
        isMobileFireDown = (bool) msg.Param;
    }
}
