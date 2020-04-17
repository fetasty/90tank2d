using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home : MonoBehaviour
{
    public Sprite broken;
    private SpriteRenderer spriteRender;
    private BoxCollider2D boxCollider;
    public void TakeDamage() {
        spriteRender.sprite = broken;
        Destroy(boxCollider);
        // todo 失败处理
    }
    // Start is called before the first frame update
    void Start()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
