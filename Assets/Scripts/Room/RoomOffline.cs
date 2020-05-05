using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomOffline : MonoBehaviour
{
    public InputField nameInput;
    public Button createBtn;
    public Button searchBtn;
    public Button backBtn;
    private string lastInput = null;
    private void Start()
    {
        lastInput = GameData.playerName;
        nameInput.placeholder.GetComponent<Text>().text = lastInput;
        nameInput.onValueChanged.AddListener(OnNameInputChanged);
        createBtn.onClick.AddListener(() => EnterRoomOnline(true));
        searchBtn.onClick.AddListener(() => EnterRoomOnline(false));
        backBtn.onClick.AddListener(() => Global.EnterWelcome());
    }
    private void Update() {
        BackOperationUpdate();
    }
    private void BackOperationUpdate() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Global.EnterWelcome();
        }
    }
    private void OnNameInputChanged(string input) {
        if (input.Length <= 10) {
            lastInput = input;
            PlayerPrefs.SetString(GameData.PLAYER_NAME_KEY, input);
        } else {
            nameInput.text = lastInput;
        }
    }
    private void EnterRoomOnline(bool isHost) {
        GameData.isHost = isHost;
        Global.EnterRoomOnline();
    }
}
