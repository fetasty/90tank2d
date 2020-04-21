using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Welcome : MonoBehaviour {
    public Button singleBtn;    // 单人游戏
    public Button doubleBtn;    // 双人游戏
    public Button lanBtn;       // 局域网游戏 
    public Button exitBtn;      // 退出游戏
    public Text versionInfoText; // 版本信息

    private void Awake() {
        //Screen.fullScreen = false; // 取消全屏 todo 试一试效果
    }
    private void Start() {
        singleBtn.onClick.AddListener(() => EnterGame(GameMode.SINGLE));
        doubleBtn.onClick.AddListener(() => EnterGame(GameMode.DOUBLE));
        //lanBtn.onClick.AddListener(() => EnterGame(GameMode.LAN)); // todo
        lanBtn.interactable = false; // todo
        exitBtn.onClick.AddListener(() => Global.Instance.Quit());
        ShowVersionInfo();
        if (Global.Instance.IsMobile) {
            doubleBtn.interactable = false;
        }
    }
    private void EnterGame(GameMode mode) {
        Global.Instance.SelectedGameMode = mode;
        Global.Instance.EnterGame();
    }
    private void ShowVersionInfo() {
        // string packageName = Application.identifier;  //包名
        string version = Application.version;     //APK版本号
        string productionName = Application.productName;   //产品名，应用名称
        string companyName = Application.companyName;   //公司名称
        versionInfoText.text = $"{productionName} by {companyName}\nV{version}";
    }
}
