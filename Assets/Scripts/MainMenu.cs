using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    public Button singleBtn;
    public Button doubleBtn;
    
    // Start is called before the first frame update
    void Start()
    {
        singleBtn.onClick.AddListener(() => {
            GameManager.Instance.EnterGame(GameManager.GameMode.Single);
        });
        doubleBtn.onClick.AddListener(() => {
            GameManager.Instance.EnterGame(GameManager.GameMode.Double);
        });
    }
}
