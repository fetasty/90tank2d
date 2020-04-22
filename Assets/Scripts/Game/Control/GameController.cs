using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Msg {
    public MsgID ID { get; set; }
    public System.Object Param { get; set; }
    public Msg(MsgID msgID, System.Object param) {
        ID = msgID;
        Param = param;
    }
}
public delegate void MsgListener(Msg msg); // 消息监听器
/// <summary>
/// 游戏逻辑管理器, 同时作为消息中心
/// </summary>
public class GameController : MonoBehaviour {
    public static GameController Instance { get; private set; }
    public GameInfoManager InfoManager { get { return infoManager; } }
    private Dictionary<MsgID, MsgListener> msgDic = new Dictionary<MsgID, MsgListener>();    // 消息监听器字典
    private Queue<Msg> msgs = new Queue<Msg>(); // 消息队列

    private UIManager uiManager;
    private MapManager mapManager;
    private TankManager tankManager;
    private GameInfoManager infoManager;

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        infoManager = GetComponent<GameInfoManager>();
        uiManager = GetComponent<UIManager>();
        mapManager = GetComponent<MapManager>();
        tankManager = GetComponent<TankManager>();
        PostMsg(new Msg(MsgID.GAME_START, Global.Instance.SelectedGameMode));
    }
    private void Update() {
        MsgUpdate();
        PauseUpdate();
    }
    private void OnApplicationFocus(bool focusStatus) {
        if (!focusStatus && infoManager.IsGamePlaying && !infoManager.IsGamePause) {
            PostMsg(new Msg(MsgID.GAME_PAUSE, null));
        }
    }
    private void PauseUpdate() {
        if (infoManager.IsGamePause || !infoManager.IsGamePlaying) { return; }
        if (!Global.Instance.IsMobile && Global.Instance.SelectedGameMode != GameMode.LAN) {
            if (Input.GetAxisRaw("Cancel") > 0f) {
                GameController.Instance.PostMsg(new Msg(MsgID.GAME_PAUSE, null));
            }
        }
    }
    private void MsgUpdate() {
        while(msgs.Count > 0) {
            Msg msg = msgs.Dequeue();
            if (msgDic.ContainsKey(msg.ID) && msgDic[msg.ID] != null) {
                msgDic[msg.ID](msg);
            }
        }
    }
    public void AddListener(MsgID msgID, MsgListener listener) {
        if (listener == null) { return; }
        if (!msgDic.ContainsKey(msgID)) {
            msgDic.Add(msgID, listener);
        } else {
            msgDic[msgID] += listener;
        }
    }
    public void RemoveListener(MsgID msgID, MsgListener listener) {
        if (listener == null) { return; }
        if (msgDic.ContainsKey(msgID)) {
            if (msgDic[msgID] == null) { msgDic.Remove(msgID); }
            else { msgDic[msgID] -= listener; }
        }
    }
    public void PostMsg(Msg msg) {
        msgs.Enqueue(msg);
    }
}
