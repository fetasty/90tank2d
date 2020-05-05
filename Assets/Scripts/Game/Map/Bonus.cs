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
    public Sprite[] sprites;
    public float lifeTime = 30f;
    public float warnLifeTime = 5f;
    [SyncVar(hook = nameof(TypeChange))]
    public int type;
    private void TypeChange(int _, int newType) {
        if (spriteRender != null) {
            spriteRender.sprite = sprites[newType];
        }
    }
    private Animation anim;
    private float lifeTimer;
    private SpriteRenderer spriteRender;
    private void Start() {
        spriteRender = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animation>();
        spriteRender.sprite = sprites[type];
        if (isServer) {
            lifeTimer = lifeTime;
        }
    }
    [ServerCallback]
    private void Update() {
        if (GameData.isGamePausing && isServer) { return; }
        LifeUpdate();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            if (isClient) { AudioController.Cur.PlayEffect(EffectAudio.BONUS); }
            if (isServer) {
                Player p = other.GetComponent<Player>();
                switch ((BonusType)type) {
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
