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
    private bool destroyFlag;
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
        DestroyUpdate();
    }
    [ServerCallback]
    private void DestroyUpdate() {
        if (destroyFlag) {
            NetworkServer.Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (destroyFlag) { return; }
        if (other.tag == "Player") {
            destroyFlag = true;
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
