using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Home : MonoBehaviour
{
    public Sprite broken;
    private SpriteRenderer spriteRender;
    private BoxCollider2D boxCollider;
    private Action<bool> homeDamageAction;
    // Start is called before the first frame update
    void Start()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    public void Set(Action<bool> homeDamage) {
        homeDamageAction = homeDamage;
    }
    public bool TakeDamage() {
        //if (GameController.Instance.IsGameOver) { return false; }
        spriteRender.sprite = broken;
        Destroy(boxCollider);
        if (homeDamageAction != null) {
            homeDamageAction(true);
        }
        return true;
    }
}
