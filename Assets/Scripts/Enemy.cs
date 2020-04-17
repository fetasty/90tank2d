using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject bulletPrefab;
    private Animator animator;
    public float moveSpeed = 2.0f;
    public float minFireTime = 0.5f;
    public float maxFireTime = 10.0f;
    public float minMoveTime = 0.5f;
    public float maxMoveTime = 10.0f;
    public int Type {
        get { return type; }
        set {
            if (value < 0) { type = 0; }
            else if (value > 4) { type = 4; }
            type = value;
            if (type == 1) { moveSpeed *= 2.5f; }
            if (animator != null) { animator.SetInteger("type", type); }
        }
    }
    public bool Bonus {
        get { return bonus; }
        set {
            bonus = value;
            if (animator != null) { animator.SetBool("bonus", bonus); }
        }
    }
    private int type;
    private bool bonus;
    private float fireTimer;
    private float moveTimer;
    private bool isMove;
    private System.Action<int> bonusAction;
    private System.Action<int> dieAction;

    private void Start() {
        animator = GetComponent<Animator>();
        fireTimer = Random.Range(minFireTime, maxFireTime);
        moveTimer = Random.Range(minMoveTime, maxMoveTime / 2.0f);
        animator.SetInteger("type", Type);
        animator.SetBool("bonus", Bonus);
    }
    private void Update() {
        Fire();
    }
    private void FixedUpdate() {
        Move();
    }
    public bool TakeDamage() {
        if (Bonus) {
            if (bonusAction != null) { bonusAction(Type); }
            Bonus = false;
            return false;
        }
        if (Type > 2) {
            --Type;
            return false;
        }
        if (dieAction != null) { dieAction(Type); }
        Destroy(gameObject);
        return true;
    }
    private void Fire() {
        if (fireTimer > 0.0f) {
            fireTimer -= Time.deltaTime;
        }
        if (fireTimer <= 0.0f) {
            GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
            Bullet bullet = obj.GetComponent<Bullet>();
            bullet.isPlayerBullet = false;
            bullet.Level = 0;
            fireTimer = Random.Range(minFireTime, maxFireTime);
        }
    }
    private void Move() {
        if (moveTimer > 0.0f) {
            moveTimer -= Time.fixedDeltaTime;
        }
        if (moveTimer <= 0.0f) {
            int random = Random.Range(0, 6);
            isMove = random > 0;
            float angle = 0f;
            switch (random) {
                case 0:
                    break;
                case 1:
                    angle = 0f;
                    break;
                case 2:
                    angle = 90f;
                    break;
                case 3:
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
}
