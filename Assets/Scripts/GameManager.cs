using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Image whiteImg;                      // 退出战场过渡白色

    private void Awake()
    {
        Instance = this;
    }

    public void FreezeTime(float duration)
    {
        Time.timeScale = 0.1f;
        StartCoroutine(UnfreezeTime(duration));
    }

    private IEnumerator UnfreezeTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        Time.timeScale = 1.0f;
    }

    public void QuitFight()
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(whiteImg.DOFade(1, 2));
        seq.AppendCallback(() =>
        {
            SceneManager.LoadScene("StartPage");
        });
    }
}