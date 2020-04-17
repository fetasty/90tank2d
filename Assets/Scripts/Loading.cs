using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    public Slider progressBar;
    public Text progressText;
    private AsyncOperation async;
    // Start is called before the first frame update
    void Start()
    {
        progressBar.value = 0.0f;
        string nextScene = GameManager.Instance.NextScene;
        async = SceneManager.LoadSceneAsync(nextScene);
    }

    // Update is called once per frame
    void Update()
    {
        progressBar.value = async.progress;
        progressText.text = $"Loading: {progressBar.value / progressBar.maxValue:#0.##%}";
    }
}
