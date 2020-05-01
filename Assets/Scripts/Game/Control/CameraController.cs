using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Messager.Instance.Listen(MessageID.GAME_START, OnMsgGameStart);
        Messager.Instance.Listen(MessageID.GAME_RETRY, OnMsgGameRetry);
    }
    private void OnMsgGameStart() {
        audioSource.Play();
    }
    private void OnMsgGameRetry() {
        audioSource.Play();
    }
}
