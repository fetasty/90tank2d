using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class RoomPlayer : NetworkBehaviour {
    public Text playerTitle;
    public Text playerName;
    [SyncVar(hook = nameof(OnTitleChange))]
    private string syncTitle;
    [SyncVar(hook = nameof(OnNameChange))]
    private string syncName;
    private void Start() {
        RectTransform rect = transform as RectTransform;
        rect.SetParent(RoomOnline.Current.playerListContent.transform);
    }
    public override void OnStartClient() {
        if (isLocalPlayer) {
            string title = isClientOnly ? "玩家" : "房主";
            string name = GameData.playerName;
            CmdSetInfo(title, name);
        }
    }
    [Command]
    private void CmdSetInfo(string title, string name) {
        syncTitle = title;
        syncName = name;
    }
    private void OnTitleChange(string _, string newTitle) {
        playerTitle.text = newTitle;
    }
    private void OnNameChange(string _, string newName) {
        playerName.text = newName;
    }
}