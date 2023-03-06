using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Transform[] playerHpballs;
    public Image darkVignette;              // ÊÜ»÷ÆÁÄ»±ä°µ
    public TextMeshProUGUI bossNameText;    // BOSSÃû

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowBossName("Hornet");
    }

    public void LostHpBall(int hp)
    {
        playerHpballs[hp].gameObject.SetActive(false);
    }

    public void FadeHurtImage()
    {
        darkVignette.gameObject.SetActive(true);
        DOTween.Sequence()
            .Append(darkVignette.DOFade(0.7f, 0.05f))
            .AppendInterval(0.3f)
            .Append(darkVignette.DOFade(0.0f, 0.5f))
            .SetEase(Ease.OutCubic);
    }

    public void ShowBossName(string bossName)
    {
        bossNameText.gameObject.SetActive(true);
        bossNameText.text = bossName;
        bossNameText.color = new Color(1, 1, 1, 0);
        DOTween.Sequence()
            .Append(bossNameText.DOFade(1.0f, 0.5f))
            .AppendInterval(2.0f)
            .Append(bossNameText.DOFade(0.0f, 0.5f));
    }
}