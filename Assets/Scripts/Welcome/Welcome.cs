using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Welcome : MonoBehaviour {
    public GameObject tankNetworkManager;
    public Button singleBtn;    // 单人游戏
    public Button doubleBtn;    // 双人游戏
    public Button lanBtn;       // 局域网游戏 
    public Button exitBtn;      // 退出游戏
    public Button notShowBtn;   // 不再提示
    public Button skipBtn;      // 跳过教程
    public Text versionInfoText; // 版本信息
    public Image startPanel;     // 游戏教程
    public float enterTime = 2f; // 教程显示时间
    private float enterTimer;
    private void Awake() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start() {
        singleBtn.onClick.AddListener(() => ClickEnter(TankMode.SINGLE));
        doubleBtn.onClick.AddListener(() => ClickEnter(TankMode.DOUBLE));
        lanBtn.onClick.AddListener(() => ClickEnter(TankMode.LAN));
        exitBtn.onClick.AddListener(() => Global.Quit());
        skipBtn.onClick.AddListener(() => EnterGame());
        notShowBtn.onClick.AddListener(() => {
            SetShowTutorial(false);
            EnterGame();
        });
        ShowVersionInfo();
        if (GameData.isMobile) { // 直接隐藏, 单人游戏按钮下移动
            doubleBtn.gameObject.SetActive(false);
        }
    }
    private void Update() {
        StartPanelUpdate();
        BackOperationUpdate();
    }
    private void BackOperationUpdate() {
        if (Input.GetKeyDown(KeyCode.Escape)) { 
            GetComponent<EscQuit>().ClickQuit();
        }
    }
    private void StartPanelUpdate() {
        if (enterTimer > 0f) {
            enterTime -= Time.deltaTime;
            if (enterTime < 0f) {
                EnterGame();
            }
        }
    }
    private void ClickEnter(TankMode mode) {
        GameData.mode = mode;
        if (mode == TankMode.LAN) {
            NetworkServer.dontListen = false;
            Global.EnterRoomOffline();
            return;
        }
        NetworkServer.dontListen = true;
        GameData.isHost = true;
        Instantiate(tankNetworkManager);
        NetworkManager.singleton.StartHost();
        if (GameData.isMobile || !IsShowTutorial()) {
            EnterGame();
        } else {
            enterTimer = enterTime;
            startPanel.gameObject.SetActive(true);
        }
    }
    private void ShowVersionInfo() {
        // string packageName = Application.identifier;  //包名
        string version = Application.version;     //APK版本号
        string productionName = Application.productName;   //产品名，应用名称
        string companyName = Application.companyName;   //公司名称
        versionInfoText.text = $"{productionName} by {companyName}\nV{version}";
    }
    private bool IsShowTutorial() {
        return PlayerPrefs.GetInt("tutorial", 1) > 0;
    }
    private void SetShowTutorial(bool show) {
        if (!show) {
            PlayerPrefs.SetInt("tutorial", 0);
        } else {
            PlayerPrefs.DeleteKey("tutorial");
        }
        PlayerPrefs.Save();
    }
    private void EnterGame() {
        Global.EnterGame();
    }
}
