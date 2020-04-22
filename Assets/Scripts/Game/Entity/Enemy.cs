using UnityEngine;

public class Enemy : MonoBehaviour {
    public const int MIN_ENEMY_TYPE = 0;
    public const int MAX_ENEMY_TYPE = 4;

    public GameObject bulletPrefab;
    public GameObject explosionPrefab;

    public float moveSpeed = 2.0f;
    public float minFireTime = 0.5f;
    public float maxFireTime = 4.0f;
    public float minMoveTime = 0.5f;
    public float maxMoveTime = 3.0f;

    private Animator animator;
    public int Type {
        get { return type; }
        set {
            if (value < 0 || value > 4) { return; }
            type = value;
            animator.SetInteger("type", type);
        }
    }
    public bool Bonus {
        get { return bonus; }
        set {
            bonus = value;
            animator.SetBool("bonus", bonus);
        }
    }
    private int type;
    private bool bonus;
    private float fireTimer;
    private float moveTimer;
    private bool isMove;
    private GameInfoManager info;
    private void Start() {
        info = GameController.Instance.InfoManager;
        GameController.Instance.PostMsg(new Msg(MsgID.ENEMY_BORN, Type));
        animator = GetComponent<Animator>();
        animator.SetInteger("type", Type);
        animator.SetBool("bonus", Bonus);
        if (type == 1) { moveSpeed *= 2f; }
        fireTimer = Random.Range(minFireTime, maxFireTime);
        moveTimer = Random.Range(minMoveTime, maxMoveTime / 2f);
    }
    private void Update() {
        if (info.IsGamePause || info.IsBonusStop) { return; }
        Fire();
    }
    private void FixedUpdate() {
        if (info.IsGamePause || info.IsBonusStop) { return; }
        Move();
    }
    public bool TakeDamage() {
        if (Bonus) {
            GameController.Instance.PostMsg(new Msg(MsgID.BONUS_SPAWN, null));
            Bonus = false;
            return false;
        }
        if (Type > 2) {
            --Type;
            return false;
        }
        Die();
        return true;
    }
    public void Die() {
        GameController.Instance.PostMsg(new Msg(MsgID.ENEMY_DIE, Type));
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    private void Fire() {
        if (fireTimer > 0.0f) {
            fireTimer -= Time.deltaTime;
        }
        if (fireTimer <= 0.0f) {
            GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
            Bullet bullet = obj.GetComponent<Bullet>();
            bullet.Set(false, 0);
            fireTimer = Random.Range(minFireTime, maxFireTime);
        }
    }
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
            moveTimer = Random.Range(minMoveTime, maxMoveTime);
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
