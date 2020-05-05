using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum BackAudio { START, WIN, OVER, LEVEL_WIN }
public enum EffectAudio { FIRE, HIT, DESTROY, EXPLOSION, BONUS, CRIZY }
public enum EngineAudio { IDLE, DRIVING, NONE }
public class AudioController : MonoBehaviour
{
    public static AudioController Cur { get; private set; }
    public AudioMixer audioMixer;
    public AudioSource backAudioSource; // 背景
    public AudioSource effectAudioSource; // 特效
    public AudioSource engineAudioSource; // 引擎
    public AudioClip[] backAudios; // start, win, over, levelwin
    public AudioClip[] effectAudios; // fire, hit, destroy, explosion, bonus
    public AudioClip[] engineAudios; // idle, driving
    [Range(-20f, 20f)]
    public float mainVolume;
    [Range(-20f, 20f)]
    public float backgroundVolume;
    [Range(-20f, 20f)]
    public float effectVolume;
    [Range(-20f, 20f)]
    public float engineVolume;
    private void Awake() {
        Cur = this;
    }
    public void PlayBack(BackAudio type) {
        backAudioSource.clip = backAudios[(int)type];
        backAudioSource.Play();
    }
    public void PlayEffect(EffectAudio type) {
        effectAudioSource.PlayOneShot(effectAudios[(int)type]);
    }
    public void SetEngine(EngineAudio type) {
        if (type == EngineAudio.NONE && engineAudioSource.isPlaying) {
            engineAudioSource.Stop();
        } else if (type != EngineAudio.NONE) {
            engineAudioSource.clip = engineAudios[(int)type];
            if (!engineAudioSource.isPlaying) {
                engineAudioSource.Play();
            }
        }
    }
    private void Start() {
        ReadVolumeCache();
        Debug.Log($"volume main {mainVolume}, back {backgroundVolume}, effect {effectVolume}, engine {engineVolume}");
    }
    private void Update() {
        EngineAudioUpdate();
    }
    private void EngineAudioUpdate() {
        // 读取player的移动情况
        EngineAudio type = EngineAudio.NONE;
        foreach (int i in Player.playerMoves) {
            if (i == 0) {
                type = EngineAudio.IDLE;
            }
            else if (i > 0) {
                type = EngineAudio.DRIVING;
                break;
            }
        }
        SetEngine(type);
    }
    public void VolumeChange() {
        SetVolume();
        SaveVolumeCache();
    }
    private void SetVolume() {
        audioMixer.SetFloat("MainVolume", mainVolume);
        audioMixer.SetFloat("BackgroundVolume", backgroundVolume);
        audioMixer.SetFloat("EffectVolume", effectVolume);
        audioMixer.SetFloat("EngineVolume", engineVolume);
    }
    private void ReadVolumeCache() {
        mainVolume = PlayerPrefs.GetFloat(GameData.MAIN_VOLUME_KEY, 0f);
        backgroundVolume = PlayerPrefs.GetFloat(GameData.BACK_VOLUME_KEY, 0f);
        effectVolume = PlayerPrefs.GetFloat(GameData.EFFECT_VOLUME_KEY, 0f);
        engineVolume = PlayerPrefs.GetFloat(GameData.ENGINE_VOLUME_KEY, 0f);
        SetVolume();
    }
    private void SaveVolumeCache() {
        PlayerPrefs.SetFloat(GameData.MAIN_VOLUME_KEY, mainVolume);
        PlayerPrefs.SetFloat(GameData.BACK_VOLUME_KEY, backgroundVolume);
        PlayerPrefs.SetFloat(GameData.EFFECT_VOLUME_KEY, effectVolume);
        PlayerPrefs.SetFloat(GameData.ENGINE_VOLUME_KEY, engineVolume);
    }
}
