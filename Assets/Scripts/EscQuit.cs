using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscQuit : MonoBehaviour
{
    public float quitWaitTime = 2f; // 两秒内双击退出游戏
    public string quitTip = "再次操作退出游戏!";
    public float quitTimer;
    public void ClickQuit() {
        if (quitTimer <= 0f) {
            quitTimer = quitWaitTime;
            Global.Toast(quitTip);
        } else {
            Global.Quit();
        }
    }
    void Update()
    {
        if (quitTimer > 0f) {
            quitTimer -= Time.deltaTime;
        }
    }
}
