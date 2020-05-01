using UnityEngine;
using Mirror;

/// <summary>
/// ServerOnly!!
/// 游戏逻辑
/// </summary>
[DisallowMultipleComponent]
public class GameController : NetworkBehaviour {
    private void Start() {
        if (isServer) { Invoke(nameof(StartGame), 1f); }
    }
    [ServerCallback]
    private void StartGame() {
        // 通知服务端的其它模块, 游戏开始
        Messager.Instance.Send(MessageID.GAME_START);
    }
    private void Update() {
        PauseUpdate();
    }
    private void OnApplicationFocus(bool focusStatus) {
        if (!focusStatus && GameData.isGamePlaying && !GameData.isGamePausing) {
            Messager.Instance.Send(MessageID.GAME_PAUSE);
        }
    }
    private void PauseUpdate() {
        if (GameData.isGamePausing || !GameData.isGamePlaying) { return; }
        if (!GameData.isMobile) {
            if (Input.GetAxisRaw("Cancel") > 0f) { // todo 网络游戏虽然不能暂停, 但是可以调出菜单的
                Messager.Instance.Send(MessageID.GAME_PAUSE);
            }
        }
    }
}
