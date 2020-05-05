using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    void Start()
    {
        AudioController.Cur.PlayEffect(EffectAudio.EXPLOSION);
        Destroy(gameObject, 0.5f);
    }
}
