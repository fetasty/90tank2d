using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Player : NetworkBehaviour {
    public const int MIN_ID = 0;
    public const int MAX_ID = 3;
    public const int MIN_LEVEL = 0;
    public const int MAX_LEVEL = 3;
    [SyncVar(hook = nameof(IDChange))]
    public int id;
    private void IDChange(int _, int newID) {
        id = newID;
        if (animator) {
            animator.SetInteger("type", id);
            playerInfo.text = $"{id + 1}P 等级{Level}";
        }
    }
    public AudioClip idleAudio;
    public AudioClip drivingAudio;
    public GameObject shield;
    public GameObject bulletPrefab;
    public GameObject explosionPrefab;
    public Canvas playerInfoPanel;
    public Text playerInfo;
    public Color[] levelColors;
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
    public int level = 0;                  // 玩家等级
    private enum Direction { NONE, UP, RIGHT, DOWN, LEFT }
    private int Level {
        get { return level; }
        set {
            if (value >= MIN_LEVEL && value <= MAX_LEVEL) {
                level = value;
                if (animator != null) { animator.SetInteger("level", level); }
                playerInfo.color = levelColors[level];
                playerInfo.text = $"{id + 1}P 等级{Level}";
            }
        }
    }
    public float FillTime {
        get {
            if (Level < 1) { return initFillTime; }
            if (Level < 2) { return initFillTime - 0.1f; }
            return initFillTime - 0.2f;
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
            if (Level < 1) {
                return 0;
            } else if (Level < 3) {
                return 1;
            } else {
                return 2;
            }
        }
    }
    public float MoveSpeed {
        get {
            if (Level < 1) {
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
        Debug.Log($"player start id {id}");
        GameObject tanks = GameObject.Find("/Tanks");
        if (tanks != null) {
            transform.parent = tanks.transform;
        }
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        animator.SetInteger("type", id);
        Level = Level;  // 初始化样式, 玩家信息
        InfoPositionUpdate();   // 玩家信息位置调整
        if (isLocalPlayer) {
            BulletCount = BulletCapacity;
        }
        if (isServer) {
            Messager.Instance.Send<int>(MessageID.PLAYER_BORN, id);
            ServerSetShield(bornShieldTime);
            Messager.Instance.Listen<int>(MessageID.BONUS_LEVEL_TRIGGER, OnMsgLevelUp);
            Messager.Instance.Listen<int>(MessageID.BONUS_SHIELD_TRIGGER, OnMsgShield);
        }
        if (GameData.isMobile && isLocalPlayer) {
            Messager.Instance.Listen<Vector2>(MessageID.MOBILE_MOVE_INPUT, OnMsgMobileMove);
            Messager.Instance.Listen<bool>(MessageID.MOBILE_FIRE_INPUT, OnMsgMobileFire);
        }
    }
    private void BeforeDestroy() {
        if (isServer) {
            // 游戏中会动态销毁的实例, 必须在销毁时注销监听
            Messager.Instance.CancelListen<int>(MessageID.BONUS_LEVEL_TRIGGER, OnMsgLevelUp);
            Messager.Instance.CancelListen<int>(MessageID.BONUS_SHIELD_TRIGGER, OnMsgShield);
        }
    }
    private void OnDestroy() {
        Debug.Log($"Player OnDestroy id = {id}"); // todo debug
        // Important: isServer在OnDestroy中不能使用
        if (NetworkServer.active) {
            // 游戏中会动态销毁的实例, 必须在销毁时注销监听
            Messager.Instance.CancelListen<int>(MessageID.BONUS_LEVEL_TRIGGER, OnMsgLevelUp);
            Messager.Instance.CancelListen<int>(MessageID.BONUS_SHIELD_TRIGGER, OnMsgShield);
        }
        if (GameData.isMobile && isLocalPlayer) {
            Messager.Instance.CancelListen<Vector2>(MessageID.MOBILE_MOVE_INPUT, OnMsgMobileMove);
            Messager.Instance.CancelListen<bool>(MessageID.MOBILE_FIRE_INPUT, OnMsgMobileFire);
        }
    }
    private void Update() {
        if (GameData.isGamePausing) { return; }
        if (isServer) {
            ShieldUpdate();
        }
        if (isLocalPlayer) {
            FireUpdate();
        }
    }
    private void FireUpdate() {
        // 子弹填充
        if (FillTimer > 0f) {
            FillTimer -= Time.deltaTime;
            if (FillTimer <= 0f) {
                BulletCount = BulletCapacity;
            }
        }
        // 射击最小间隔
        if (FireTimer > 0f) {
            FireTimer -= Time.deltaTime;
            return;
        }
        // 按了开火键
        if (FireInput()) {
            // 是否有子弹
            if (BulletCount > 0) {
                --BulletCount;
                FireTimer = fireDuration;
                // 只有打出一发子弹之后才会重置填充计时, 否则可能出现连发
                if (FillTimer <= 0f) {
                    FillTimer = FillTime;
                }
                CmdFire();
            }
        }
    }
    [Command]
    private void CmdFire() {
        GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
        Bullet bullet = obj.GetComponent<Bullet>();
        bullet.Set(true, BulletLevel);
        NetworkServer.Spawn(obj);
    }
    private void FixedUpdate() {
        if (GameData.isGamePausing) { return; }
        if (isLocalPlayer) {
            ClientMove();
        }
        if (isClient) { InfoPositionUpdate(); }
    }
    private Vector2 GetMoveInput() {
        Vector2 result = Vector2.zero;
        // 本地双人
        if (GameData.mode == TankMode.DOUBLE) {
            if (id == 0) {
                result.x = Input.GetAxisRaw("Horizontal");
                result.y = Input.GetAxisRaw("Vertical");
            } else {
                result.x = Input.GetAxisRaw("Horizontal2");
                result.y = Input.GetAxisRaw("Vertical2");
            }
            return result;
        }
        // 移动平台
        if (GameData.isMobile) {
            return mobileInput;
        }
        // win平台
        result.x = Input.GetAxisRaw("Horizontal");
        result.y = Input.GetAxisRaw("Vertical");
        return result;
    }
    private void ClientMove() {
        Vector2 moveInput = GetMoveInput();
        float h = moveInput.x;
        float v = moveInput.y;
        float hAbs = Mathf.Abs(h);
        float vAbs = Mathf.Abs(v);
        bool isMove = true;
        Direction direction = Direction.NONE;
        if (hAbs < 0.01f && vAbs < 0.01f) {
            direction = Direction.NONE;
            isMove = false;
        } else if (hAbs > vAbs) {
            horizontalInputLast = true;
            if (h > 0f) { direction = Direction.RIGHT; }
            else { direction = Direction.LEFT; }
        } else if (vAbs > hAbs) {
            horizontalInputLast = false;
            if (v > 0f) { direction = Direction.UP; }
            else { direction = Direction.DOWN; }
        } else { // h + v 同时按下
            if (horizontalInputLast) {
                if (v > 0f) { direction = Direction.UP; }
                else { direction = Direction.DOWN; }
            } else {
                if (h > 0f) { direction = Direction.RIGHT; }
                else { direction = Direction.LEFT; }
            }
        }
        if (isMove) {
            float angle = ((float) direction - 1f) * 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, -angle);
            transform.Translate(transform.up * Time.fixedDeltaTime * MoveSpeed, Space.World);
        }
        audioSource.clip = isMove ? drivingAudio : idleAudio;
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }
    private void InfoPositionUpdate() {
        playerInfoPanel.transform.position = transform.position + new Vector3(0f, 0.7f, 0f);
        playerInfoPanel.transform.rotation = Quaternion.identity;
    }
    /// <summary>
    /// 玩家中弹, 返回玩家受伤后是否死亡
    /// </summary>
    /// <returns>受伤是否死亡, true, 死亡; false, 未死亡</returns>
    [ServerCallback]
    public bool TakeDamage() {
        Debug.Log($"Player damage id {id}");
        if (ShieldTimer > 0f) { return false; }
        if (Level >= 2) {
            RpcLevelDown(2);
            return false;
        } else {
            Messager.Instance.Send<int>(MessageID.PLAYER_DIE, id);
            NetworkServer.Spawn(Instantiate(explosionPrefab, transform.position, Quaternion.identity));
            NetworkServer.Destroy(gameObject);
            return true;
        }
    }
    [ClientRpc]
    public void RpcLevelDown(int down) {
        Level -= down;
    }
    [ClientRpc]
    public void RpcLevelUp() {
        if (Level >= MAX_LEVEL) { return; }
        ++Level;
    }
    [ServerCallback]
    public void ServerSetShield(float time) {
        if (time < 0.0f) { return; }
        ShieldTimer = time;
        RpcSetShield(true);
    }
    [ServerCallback]
    private void ServerCloseShield() {
        ShieldTimer = 0.0f;
        RpcSetShield(false);
    }
    [ClientRpc]
    private void RpcSetShield(bool active) {
        shield.SetActive(active);
    }
    [ServerCallback]
    private void ShieldUpdate() {
        if (ShieldTimer > 0f) {
            ShieldTimer -= Time.deltaTime;
            if (ShieldTimer <= 0.0f) {
                ServerCloseShield();
            }
        }
    }
    private bool FireInput() {
        if (GameData.mode == TankMode.DOUBLE) {
            if (id == 0) {
                return Input.GetAxisRaw("Fire1") > 0f;
            }
            return Input.GetAxisRaw("Fire2") > 0f;
        }
        if (GameData.isMobile) {
            return isMobileFireDown;
        }
        return Input.GetAxisRaw("Fire1") > 0f;
    }
    [ServerCallback]
    public void OnMsgLevelUp(int playerID) {
        if (id == playerID) {
            RpcLevelUp();
        }
    }
    [ServerCallback]
    public void OnMsgShield(int playerID) {
        if (id == playerID) {
            ServerSetShield(bonusShieldTime);
        }
    }
    public void OnMsgMobileMove(Vector2 input) {
        mobileInput = input;
    }
    public void OnMsgMobileFire(bool isDown) {
        isMobileFireDown = isDown;
    }
}
