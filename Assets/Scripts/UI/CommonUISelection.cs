using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CommonUISelection : MonoBehaviour
{
    // ���̲���ʱ����ѡ��İ���
    public string upKey;

    // ���̲���ʱ����ѡ��İ���
    public string downKey;

    // ���̲���ʱ��ȷ�ϼ�
    public string confirmKey;

    // ѡ���¼�����������ͬ�İ�ťͨ��intֵ���֣�
    [HideInInspector]
    public UnityEvent<int> onSelectListeners;

    // ȷ���¼�����������ͬ�İ�ťͨ��intֵ���֣�
    [HideInInspector]
    public UnityEvent<int> onConfirmListeners;

    // ��ǰѡ�еİ�ťintֵ
    [HideInInspector]
    public int curIndex;

    // �ܵİ�ť����
    [HideInInspector]
    public int countSelections;

    // ��ť�б�
    private List<Transform> listSelections;

    // ��ʼ������
    // selections����ť�б�
    // onSelect��ѡ��ʱ�����¼�
    // onConfirm��ȷ��ʱ�����¼�
    public void Init(List<Transform> selections, UnityAction<int> onSelect, UnityAction<int> onConfirm)
    {
        // �Ƴ�ԭ�еĴ����¼�
        onSelectListeners.RemoveAllListeners();
        onConfirmListeners.RemoveAllListeners();
        // ��ť����
        countSelections = selections.Count;
        // ��Ӵ����¼�
        onSelectListeners.AddListener(onSelect);
        onConfirmListeners.AddListener(onConfirm);
        // ��ť�б�
        listSelections = selections;

        // ��ÿ����ť������ָ�����͵���Ĵ�����
        for (int i = 0; i < countSelections; ++i)
        {
            EventTrigger et = listSelections[i].GetComponent<EventTrigger>();
            if (!et)
            {
                // û�д����������
                et = listSelections[i].gameObject.AddComponent<EventTrigger>();
            }
            et.triggers = new List<EventTrigger.Entry>();
            // ���ָ����봥����
            var entry1 = new EventTrigger.Entry();
            entry1.eventID = EventTriggerType.PointerEnter;
            // ������ʱ�����ص�����
            entry1.callback.AddListener(OnPointerEnter);
            // �����������
            var entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerClick;
            // �����ʱ�����ص�����
            entry2.callback.AddListener(OnPointerClick);
            et.triggers.Add(entry1);
            et.triggers.Add(entry2);
        }
    }

    // �������¼��ص�����
    private void OnPointerEnter(BaseEventData _evt)
    {
        PointerEventData evt = (PointerEventData)_evt;
        // ��ȡ������İ�ť���б��е��±�
        int idx = listSelections.IndexOf(evt.pointerEnter.transform);
        // �����Զ���ļ�������
        onSelectListeners.Invoke(idx);
    }

    // ������¼��ص�����
    private void OnPointerClick(BaseEventData _evt)
    {
        PointerEventData evt = (PointerEventData)_evt;
        // ��ȡ������İ�ť���б��е��±�
        int idx = listSelections.IndexOf(evt.pointerClick.transform);
        // �����Զ���ļ�������
        onConfirmListeners.Invoke(idx);
    }

    private void Update()
    {
        // ���浱ǰ���±�
        int lastIdx = curIndex;

        // �ڴ˱�д���̲�����صĴ���
        if (Input.GetButtonDown(upKey))
        {
            curIndex--;
        }
        else if (Input.GetButtonDown(downKey))
        {
            curIndex++;
        }
        else if (Input.GetButtonDown(confirmKey))
        {
            onConfirmListeners.Invoke(curIndex);
            return;
        }
        else
        {
            return;
        }
        // �޶���Χ
        curIndex = curIndex < 0 ? 0 : curIndex >= countSelections ? countSelections - 1 : curIndex;
        if (curIndex != lastIdx)
        {
            onSelectListeners.Invoke(curIndex);
        }
    }
}