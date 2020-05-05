using UnityEngine;
using Mirror;

public class Enemy : NetworkBehaviour {
    public const int MIN_ENEMY_TYPE = 0;
    public const int MAX_ENEMY_TYPE = 4;

    public GameObject bulletPrefab;
    public GameObject explosionPrefab;
    public float moveSpeed = 2.0f;

    private Animator animator;
    [SyncVar(hook = nameof(TypeChange))]
    public int type;
    private void TypeChange(int _, int newType) {
        if (animator != null) { animator.SetInteger("type", newType); }
    }
    [SyncVar(hook = nameof(BonusChange))]
    private bool bonus;
    private void BonusChange(bool _, bool newBonus) {
        if (animator != null) { animator.SetBool("bonus", newBonus); }
    }
    private bool Bonus {
        get { return bonus; }
        set {
            bonus = value;
            if (animator != null) { animator.SetBool("bonus", bonus); }
        }
    }
    private float fireTimer;
    private float moveTimer;
    private bool isMove;
    private void Start() {
        GameObject tanks = GameObject.Find("/Tanks");
        if (tanks != null) {
            transform.parent = tanks.transform;
        }
        Messager.Instance.Send(MessageID.ENEMY_BORN);
        animator = GetComponent<Animator>();
        animator.SetInteger("type", type);
        animator.SetBool("bonus", bonus);
        if (isServer) {
            moveSpeed = GameData.EnemySpeed;
            if (!GameData.enemyCrizy && type == 1) { moveSpeed *= 2f; }
            fireTimer = GameData.EnemyFireTime;
            moveTimer = GameData.EnemyMoveTime;
        }
    }
    [ServerCallback]
    private void Update() {
        if ((GameData.isGamePausing && isServer) || GameData.isStopWatchRunning) { return; }
        Fire();
    }
    [ServerCallback]
    private void FixedUpdate() {
        if ((GameData.isGamePausing && isServer) || GameData.isStopWatchRunning) { return; }
        Move();
    }
    [ServerCallback]
    public bool TakeDamage() {
        if (bonus) {
            Messager.Instance.Send(MessageID.BONUS_SPAWN);
            bonus = false;
            return false;
        }
        if (type > 2) {
            --type;
            return false;
        }
        Die();
        return true;
    }
    [ServerCallback]
    public void Die() {
        Messager.Instance.Send(MessageID.ENEMY_DIE);
        NetworkServer.Spawn(Instantiate(explosionPrefab, transform.position, Quaternion.identity));
        NetworkServer.Destroy(gameObject);
    }
    [ServerCallback]
    private void Fire() {
        if (fireTimer > 0.0f) {
            fireTimer -= Time.deltaTime;
        }
        if (fireTimer <= 0.0f) {
            GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
            Bullet bullet = obj.GetComponent<Bullet>();
            bullet.Set(false, type > 2 ? 1 : 0);
            NetworkServer.Spawn(obj);   // 服务器生成子弹
            fireTimer = GameData.EnemyFireTime;
        }
    }
    [ServerCallback]
    private void Move() {
        if (moveTimer > 0.0f) {
            moveTimer -= Time.fixedDeltaTime;
        }
        if (moveTimer <= 0.0f) {
            int random = Random.Range(0, 7);
            isMove = random > 1;
            float angle = 0f;
            switch (random) {
                case 0:
                    break;
                case 1:
                    transform.rotation = Quaternion.Euler(0f, 0f, 90f * Random.Range(0, 3));
                    break;
                case 2:
                    angle = 0f;
                    break;
                case 3:
                    angle = 90f;
                    break;
                case 4:
                    angle = -90f;
                    break;
                default:
                    angle = 180f;
                    break;
            }
            if (isMove) {
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            moveTimer = GameData.EnemyMoveTime;
        }
        if (isMove) {
            transform.Translate(transform.up * moveSpeed * Time.fixedDeltaTime, Space.World);
        }
    }
    public void Set(int enemyType, bool enemyBonus) {
        type = enemyType;
        bonus = enemyBonus;
    }
}
