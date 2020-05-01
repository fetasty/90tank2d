using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class Home : NetworkBehaviour
{
    public Sprite broken;
    public GameObject explosionPrefab;
    private SpriteRenderer spriteRender;
    private BoxCollider2D boxCollider;
    void Start()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    [ServerCallback]
    public bool TakeDamage() {
        if (!GameData.isGamePlaying) { return false; }
        Messager.Instance.Send(MessageID.HOME_DESTROY);
        RpcHomeDestroy();
        return true;
    }
    [ClientRpc]
    private void RpcHomeDestroy() {
        spriteRender.sprite = broken;
        Destroy(boxCollider);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
