using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BonusType {
    BOOM = 0, SHIELD, TANK, LEVEL, STOP_WATCH, SHOVEL
}
public class Bonus : MonoBehaviour
{
    public const int BONUS_TYPE_COUNT = 6;
    public AudioClip bonusAudio;
    public Sprite[] sprites;
    public BonusType Type { get; set; }
    private void Start() {
        Type = (BonusType) Random.Range(0, BONUS_TYPE_COUNT);
        GetComponent<SpriteRenderer>().sprite = sprites[(int) Type];
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            Player p = other.GetComponent<Player>();
            Msg msg = new Msg(MsgID.BONUS_BOOM_TRIGGER, p.ID);
            switch (Type) {
                case BonusType.BOOM:
                    msg.ID = MsgID.BONUS_BOOM_TRIGGER;
                    break;
                case BonusType.LEVEL:
                    msg.ID = MsgID.BONUS_LEVEL_TRIGGER;
                    break;
                case BonusType.SHIELD:
                    msg.ID = MsgID.BONUS_SHIELD_TRIGGER;
                    break;
                case BonusType.SHOVEL:
                    msg.ID = MsgID.BONUS_SHOVEL_TRIGGER;
                    break;
                case BonusType.STOP_WATCH:
                    msg.ID = MsgID.BONUS_STOP_WATCH_TRIGGER;
                    break;
                case BonusType.TANK:
                    msg.ID = MsgID.BONUS_TANK_TRIGGER;
                    break;
                default:
                    Destroy(gameObject);
                    return;
            }
            GameController.Instance.PostMsg(msg);
            AudioSource.PlayClipAtPoint(bonusAudio, transform.position);
            Destroy(gameObject);
        }
    }
}
