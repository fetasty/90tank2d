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
    void Start()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    public bool TakeDamage() {
        if (!GameData.isGamePlaying) { return false; }
        Messager.Instance.Send(MessageID.HOME_DESTROY);
        spriteRender.sprite = broken;
        Destroy(boxCollider);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        return true;
    }
}
