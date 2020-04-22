using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public AudioClip explosionAudio;
    void Start()
    {
        AudioSource.PlayClipAtPoint(explosionAudio, transform.position);
        Destroy(gameObject, 0.5f);
    }
}
