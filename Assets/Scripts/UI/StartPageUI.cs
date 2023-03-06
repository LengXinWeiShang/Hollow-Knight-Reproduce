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

    // �����ı�
    private Text title;

    // �������
    private Outline titleOutline;

    // ����ҳ�ڱ�
    private Image darkImg;

    // ������Ϸǰ�İ�ɫ����
    private Image whiteImg;

    private void Start()
    {
        // �������
        Cursor.lockState = CursorLockMode.Locked;
        // ��ʼ��ͨ��UI���
        select = GetComponent<CommonUISelection>();
        select.Init(listSelections, OnSelectChange, OnConfirm);
        // �������
        title = gameObject.transform.Find("Text_Title").GetComponent<Text>();
        darkImg = gameObject.transform.Find("TitleImg").GetComponent<Image>();
        whiteImg = gameObject.transform.Find("LoadWhite").GetComponent<Image>();
        // ��ʼ�����棨��Ч��
        Init();
    }

    // ��ʼ����ʼ����
    private void Init()
    {
        foreach (var o in listSelections)
        {
            // ��������ѡ���ı�
            o.gameObject.SetActive(false);
        }
        // �����������Ϊ͸��
        titleOutline = title.gameObject.GetComponent<Outline>();
        titleOutline.effectColor *= new Color(1, 1, 1, 0);

        // �����ı���ͼƬ����Ϊ͸��
        title.color *= new Color(1, 1, 1, 0);
        darkImg.color *= new Color(1, 1, 1, 0);
        whiteImg.color *= new Color(1, 1, 1, 0);

        Sequence seq = DOTween.Sequence();
        // �����ı�����Ч��
        seq.Append(title.DOFade(1, 2));
        // ������ߵ���Ч����ͼƬ����Ч��һ��ִ��
        seq.Append(titleOutline.DOFade(0.5f, 2));
        // ͼƬ����Ч��
        seq.Join(darkImg.DOFade(1, 2));
        // ����ִ�еĻص�
        seq.AppendCallback(() =>
        {
            // ��ʾ����ѡ���ı�
            foreach (var o in listSelections)
            {
                o.gameObject.SetActive(true);
            }
        });
    }

    // �������¼�
    private void OnSelectChange(int index)
    {
        // ѡ�е��ı����ѡ�п�
        for (int i = 0; i < listSelections.Count; ++i)
        {
            listSelections[i].transform.GetChild(0).gameObject.SetActive(i == index);
        }
    }

    // ������¼�
    private void OnConfirm(int index)
    {
        // ����ѡ�е��ı�ִ�в���
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