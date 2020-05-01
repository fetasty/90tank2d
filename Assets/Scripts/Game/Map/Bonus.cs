using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum BonusType {
    BOOM = 0, SHIELD, TANK, LEVEL, STOP_WATCH, SHOVEL
}
public class Bonus : NetworkBehaviour
{
    public const int BONUS_TYPE_COUNT = 6;
    public AudioClip bonusAudio;
    public Sprite[] sprites;
    public float lifeTime = 30f;
    public float warnLifeTime = 5f;
    [SyncVar(hook = nameof(TypeChange))]
    public BonusType type;
    private void TypeChange(BonusType _, BonusType newType) {
        spriteRender.sprite = sprites[(int) type];
    }
    private Animation anim;
    private float lifeTimer;
    private SpriteRenderer spriteRender;
    private void Start() {
        spriteRender = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animation>();
        lifeTimer = lifeTime;
        spriteRender.sprite = sprites[(int) type];
        if (isServer) {
            type = (BonusType) Random.Range(0, BONUS_TYPE_COUNT);
        }
    }
    [ServerCallback]
    private void Update() {
        if (GameData.isGamePausing) { return; }
        LifeUpdate();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            if (isClient) { AudioSource.PlayClipAtPoint(bonusAudio, transform.position); }
            if (isServer) {
                Player p = other.GetComponent<Player>();
                switch (type) {
                    case BonusType.BOOM:
                        Messager.Instance.Send(MessageID.BONUS_BOOM_TRIGGER);
                        break;
                    case BonusType.LEVEL:
                        Messager.Instance.Send<int>(MessageID.BONUS_LEVEL_TRIGGER, p.id);
                        break;
                    case BonusType.SHIELD:
                        Messager.Instance.Send<int>(MessageID.BONUS_SHIELD_TRIGGER, p.id);
                        break;
                    case BonusType.SHOVEL:
                        Messager.Instance.Send(MessageID.BONUS_SHOVEL_TRIGGER);
                        break;
                    case BonusType.STOP_WATCH:
                        Messager.Instance.Send(MessageID.BONUS_STOP_WATCH_TRIGGER);
                        break;
                    case BonusType.TANK:
                        Messager.Instance.Send(MessageID.BONUS_TANK_TRIGGER);
                        break;
                    default:
                        return;
                }
                NetworkServer.Destroy(gameObject);
            }
        }
    }
    [ServerCallback]
    private void LifeUpdate() {
        if (lifeTimer > 0f) {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= warnLifeTime && !anim.isPlaying) {
                RpcAni();
            }
            if (lifeTimer <= 0f) {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
    [ClientRpc]
    private void RpcAni() {
        anim.Play();
    }
}
