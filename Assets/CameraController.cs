using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GameController.Instance.AddListener(MsgID.GAME_START, OnMsgGameStart);
        GameController.Instance.AddListener(MsgID.GAME_RETRY, OnMsgGameRetry);
    }
    private void OnMsgGameStart(Msg msg) {
        audioSource.Play();
    }
    private void OnMsgGameRetry(Msg msg) {
        audioSource.Play();
    }
}
