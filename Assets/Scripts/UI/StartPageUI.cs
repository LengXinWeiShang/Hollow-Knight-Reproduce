using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class StartPageUI : MonoBehaviour
{
    public List<Transform> listSelections;

    private CommonUISelection select;

    // 标题文本
    private Text title;

    // 标题描边
    private Outline titleOutline;

    // 标题页黑边
    private Image darkImg;

    // 进入游戏前的白色过渡
    private Image whiteImg;

    private void Start()
    {
        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        // 初始化通用UI组件
        select = GetComponent<CommonUISelection>();
        select.Init(listSelections, OnSelectChange, OnConfirm);
        // 界面相关
        title = gameObject.transform.Find("Text_Title").GetComponent<Text>();
        darkImg = gameObject.transform.Find("TitleImg").GetComponent<Image>();
        whiteImg = gameObject.transform.Find("LoadWhite").GetComponent<Image>();
        // 初始化界面（动效）
        Init();
    }

    // 初始化开始界面
    private void Init()
    {
        foreach (var o in listSelections)
        {
            // 隐藏所有选择文本
            o.gameObject.SetActive(false);
        }
        // 标题描边设置为透明
        titleOutline = title.gameObject.GetComponent<Outline>();
        titleOutline.effectColor *= new Color(1, 1, 1, 0);

        // 标题文本和图片设置为透明
        title.color *= new Color(1, 1, 1, 0);
        darkImg.color *= new Color(1, 1, 1, 0);
        whiteImg.color *= new Color(1, 1, 1, 0);

        Sequence seq = DOTween.Sequence();
        // 标题文本淡入效果
        seq.Append(title.DOFade(1, 2));
        // 标题描边淡入效果和图片淡入效果一起执行
        seq.Append(titleOutline.DOFade(0.5f, 2));
        // 图片淡入效果
        seq.Join(darkImg.DOFade(1, 2));
        // 最终执行的回调
        seq.AppendCallback(() =>
        {
            // 显示所有选择文本
            foreach (var o in listSelections)
            {
                o.gameObject.SetActive(true);
            }
        });
    }

    // 鼠标进入事件
    private void OnSelectChange(int index)
    {
        // 选中的文本添加选中框
        for (int i = 0; i < listSelections.Count; ++i)
        {
            listSelections[i].transform.GetChild(0).gameObject.SetActive(i == index);
        }
    }

    // 鼠标点击事件
    private void OnConfirm(int index)
    {
        // 根据选中的文本执行操作
        switch (index)
        {
            case 0:
                Sequence seq = DOTween.Sequence();
                seq.Append(whiteImg.DOFade(1, 1));
                seq.AppendCallback(() =>
                {
                    SceneManager.LoadScene("Game");
                });
                break;

            case 1:
                Application.Quit();
                break;
        }
    }
}