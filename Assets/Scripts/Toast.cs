using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    public string text = "A little tip!";
    public float life = 1.5f;
    public void Set(string text, float life = 1.5f) {
        this.text = text;
        this.life = life;
    }
    private void Start() {
        transform.GetChild(0).GetComponent<Text>().text = text;
        transform.localPosition = new Vector3(0f, -70f, 2f);
        transform.localScale = new Vector3(1f, 1f, 1f);
        Invoke(nameof(SelfDestroy), life);
    }
    private void SelfDestroy() {
        Destroy(gameObject);
    }
}
