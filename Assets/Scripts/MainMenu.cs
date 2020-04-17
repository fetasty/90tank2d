using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    public Button singleBtn;
    public Button doubleBtn;
    public Button exitBtn;
    // Start is called before the first frame update
    void Start()
    {
        singleBtn.onClick.AddListener(() => {
            GameManager.Instance.EnterGame(GameManager.GameMode.Single);
        });
        doubleBtn.onClick.AddListener(() => {
            GameManager.Instance.EnterGame(GameManager.GameMode.Double);
        });
        exitBtn.onClick.AddListener(() => {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
    }
}
