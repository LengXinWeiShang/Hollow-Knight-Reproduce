using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CommonUISelection : MonoBehaviour
{
    // 键盘操作时向上选择的按键
    public string upKey;

    // 键盘操作时向下选择的按键
    public string downKey;

    // 键盘操作时的确认键
    public string confirmKey;

    // 选中事件监听器（不同的按钮通过int值区分）
    [HideInInspector]
    public UnityEvent<int> onSelectListeners;

    // 确认事件监听器（不同的按钮通过int值区分）
    [HideInInspector]
    public UnityEvent<int> onConfirmListeners;

    // 当前选中的按钮int值
    [HideInInspector]
    public int curIndex;

    // 总的按钮数量
    [HideInInspector]
    public int countSelections;

    // 按钮列表
    private List<Transform> listSelections;

    // 初始化函数
    // selections：按钮列表
    // onSelect：选中时触发事件
    // onConfirm：确认时触发事件
    public void Init(List<Transform> selections, UnityAction<int> onSelect, UnityAction<int> onConfirm)
    {
        // 移除原有的触发事件
        onSelectListeners.RemoveAllListeners();
        onConfirmListeners.RemoveAllListeners();
        // 按钮总数
        countSelections = selections.Count;
        // 添加触发事件
        onSelectListeners.AddListener(onSelect);
        onConfirmListeners.AddListener(onConfirm);
        // 按钮列表
        listSelections = selections;

        // 给每个按钮添加鼠标指针进入和点击的触发器
        for (int i = 0; i < countSelections; ++i)
        {
            EventTrigger et = listSelections[i].GetComponent<EventTrigger>();
            if (!et)
            {
                // 没有触发器则添加
                et = listSelections[i].gameObject.AddComponent<EventTrigger>();
            }
            et.triggers = new List<EventTrigger.Entry>();
            // 鼠标指针进入触发器
            var entry1 = new EventTrigger.Entry();
            entry1.eventID = EventTriggerType.PointerEnter;
            // 鼠标进入时触发回调函数
            entry1.callback.AddListener(OnPointerEnter);
            // 鼠标点击触发器
            var entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerClick;
            // 鼠标点击时触发回调函数
            entry2.callback.AddListener(OnPointerClick);
            et.triggers.Add(entry1);
            et.triggers.Add(entry2);
        }
    }

    // 鼠标进入事件回调函数
    private void OnPointerEnter(BaseEventData _evt)
    {
        PointerEventData evt = (PointerEventData)_evt;
        // 获取鼠标进入的按钮在列表中的下标
        int idx = listSelections.IndexOf(evt.pointerEnter.transform);
        // 调用自定义的监听函数
        onSelectListeners.Invoke(idx);
    }

    // 鼠标点击事件回调函数
    private void OnPointerClick(BaseEventData _evt)
    {
        PointerEventData evt = (PointerEventData)_evt;
        // 获取鼠标进入的按钮在列表中的下标
        int idx = listSelections.IndexOf(evt.pointerClick.transform);
        // 调用自定义的监听函数
        onConfirmListeners.Invoke(idx);
    }

    private void Update()
    {
        // 保存当前的下标
        int lastIdx = curIndex;

        // 在此编写键盘操作相关的触发
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
        // 限定范围
        curIndex = curIndex < 0 ? 0 : curIndex >= countSelections ? countSelections - 1 : curIndex;
        if (curIndex != lastIdx)
        {
            onSelectListeners.Invoke(curIndex);
        }
    }
}