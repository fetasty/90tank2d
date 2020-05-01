using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RoomOnline : MonoBehaviour
{
    public static RoomOnline Current { get; private set; }
    public GameObject roomListPanel;
    public GameObject playerListPanel;
    public Button roomPanelBackBtn;
    public Button playerPanelBackBtn;
    public Button startGameBtn;
    public Text roomName;
    public Text roomMember;
    public GameObject roomListContent;
    public GameObject playerListContent;
    public GameObject roomItemPrefab;
    public GameObject playerItemPrefab;

    private MyNetworkDiscovery networkDiscovery;
    private readonly Dictionary<long, MyServerResponse> discoveredServers = new Dictionary<long, MyServerResponse>();
    private void Awake() {
        Current = this;
    }
    void Start()
    {
        GameData.networkPlayers.Clear();
        discoveredServers.Clear();

        roomListPanel.SetActive(!GameData.isHost);
        playerListPanel.SetActive(GameData.isHost);
        networkDiscovery = GetComponent<MyNetworkDiscovery>();
        networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
        roomPanelBackBtn.onClick.AddListener(Back);
        playerPanelBackBtn.onClick.AddListener(Back);
        startGameBtn.gameObject.SetActive(false); // 开始不显示游戏开始按钮
        startGameBtn.onClick.AddListener(OnClickStartGame);
        MyNetworkRoomManager mamager = NetworkManager.singleton as MyNetworkRoomManager;
        mamager.onCreateRoomPlayer = OnCreateRoomPlayer;
        mamager.onClientDisconnect = OnClientDisconnect;
        mamager.onGameStartStateChange = OnGameStartStateChange;
        if (GameData.isHost) {
            roomName.text = $"[{GameData.playerName}的房间]";
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        } else {
            networkDiscovery.MyStartDiscovery();
        }
    }

    void Update()
    {
        DiscoveryedServerUpdate();
    }
    private void OnGUI() {
        DiscoveryedServerGUI();
        PlayerListMemberGUI();
    }
    private void DiscoveryedServerGUI() {
        if (roomListPanel != null && roomListPanel.activeSelf) {
            List<MyServerResponse> serverList = discoveredServers.Values.ToList();
            BalanceList(roomListContent, roomItemPrefab, serverList.Count);
            for (int i = 0; i < serverList.Count; ++i) {
                RoomItem item = roomListContent.transform.GetChild(i).GetComponent<RoomItem>();
                item.SetInfo(serverList[i], OnClickJoinRoom);
            }
        }
    }
    private void PlayerListMemberGUI() {
        if (playerListPanel != null && playerListPanel.activeSelf) {
            NetworkManager manager = NetworkManager.singleton;
            roomMember.text = $"玩家列表({playerListContent.transform.childCount}/{manager.maxConnections})";
        }
    }
    private void BalanceList(GameObject content, GameObject prefab, int count) {
        for (int i = content.transform.childCount; i < count; ++i) {
            Instantiate(prefab, content.transform, false);
        }
        for (int i = content.transform.childCount - 1; i >= count; --i) {
            Destroy(content.transform.GetChild(i).gameObject);
        }
    }
    private void DiscoveryedServerUpdate() {
        if (roomListPanel != null && roomListPanel.activeSelf) {
            foreach (long key in discoveredServers.Keys.ToList()) {
                if (discoveredServers[key].lifeTimer > 0f) {
                    discoveredServers[key].lifeTimer -= Time.deltaTime;
                } else {
                    discoveredServers.Remove(key);
                }
            }
        }
    }
    private void OnDiscoveredServer(MyServerResponse response) {
        discoveredServers[response.serverId] = response;
    }
    private void OnClickJoinRoom(long key) {
        if (!discoveredServers.ContainsKey(key)) { return; }
        MyServerResponse info = discoveredServers[key];
        if (info == null) { return; }
        roomListPanel.SetActive(false);
        playerListPanel.SetActive(true);
        roomName.text = $"[{info.name}的房间]";
        NetworkManager.singleton.StartClient(info.uri);
        networkDiscovery.StopDiscovery();
    }
    private void OnCreateRoomPlayer(NetworkConnection conn) {
        GameObject obj = Instantiate(playerItemPrefab);
        NetworkServer.AddPlayerForConnection(conn, obj);
    }
    private void OnClientDisconnect(NetworkConnection conn) {
        BackToOffline();
    }
    private void OnGameStartStateChange(bool isAbleStartGame) {
        startGameBtn.gameObject.SetActive(isAbleStartGame);
    }
    private void OnClickStartGame() {
        if (!GameData.isHost) { return; }
        MyNetworkRoomManager manager = NetworkManager.singleton as MyNetworkRoomManager;
        if (manager.numPlayers < manager.minPlayer) { return; }
        Global.EnterGame();
    }
    private void Back() {
        if (roomListPanel.activeSelf) {
            // 从房间列表返回
            BackToOffline();
        } else {
            // 从用户列表返回
            if (GameData.isHost) {
                // 到离线界面
                BackToOffline();
            } else {
                // 到房间列表
                playerListPanel.SetActive(false);
                roomListPanel.SetActive(true);
                NetworkManager.singleton.StopClient();
                discoveredServers.Clear();
                networkDiscovery.MyStartDiscovery();
            }
        }
    }
    private void BackToOffline() {
        if (networkDiscovery != null) {
            networkDiscovery.StopDiscovery();
        }
        NetworkManager.singleton.StopHost();
        Destroy(NetworkManager.singleton.gameObject);    // 这里必须显式调用
        Global.EnterRoomOffline();
    }
}
