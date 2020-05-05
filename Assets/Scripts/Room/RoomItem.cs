using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RoomItem : MonoBehaviour
{
    public Text roomName;
    public Text roomIP;
    public Text member;
    public Button join;
    
    public void SetInfo(MyServerResponse info, Action<string> joinRoom) {
        roomName.text = info.name;
        roomIP.text = info.EndPoint.Address.ToString();
        member.text = $"{info.playerCount}/{info.maxPlayerCount}";
        join.onClick.RemoveAllListeners();
        join.onClick.AddListener(() => joinRoom(info.deviceUniqueIdentifier));
    }
}
