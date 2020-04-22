using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Home : MonoBehaviour
{
    public Sprite broken;
    public GameObject explosionPrefab;
    private SpriteRenderer spriteRender;
    private BoxCollider2D boxCollider;
    private GameInfoManager info;
    void Start()
    {
        info = GameController.Instance.InfoManager;
        spriteRender = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    public bool TakeDamage() {
        if (!info.IsGamePlaying) { return false; }
        GameController.Instance.PostMsg(new Msg(MsgID.HOME_DESTROY, null));
        spriteRender.sprite = broken;
        Destroy(boxCollider);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        return true;
    }
}
