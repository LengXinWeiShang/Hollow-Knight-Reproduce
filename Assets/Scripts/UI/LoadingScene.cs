using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    public Slider loadingSlider;

    // �첽������Ϸ����
    private AsyncOperation async;

    // ���ؽ���
    private int curProcess;

    private void Start()
    {
        loadingSlider = GameObject.Find("Loading Bar").GetComponent<Slider>();
        curProcess = 0;

        StartCoroutine(LoadScene());
    }

    private void Update()
    {
        if (async == null)
        {
            return;
        }
        curProcess = (int)async.progress * 100;

        loadingSlider.value = curProcess;
    }

    private IEnumerator LoadScene()
    {
        // �첽������Ϸ����
        async = SceneManager.LoadSceneAsync("Game");
        yield return async;
    }
}