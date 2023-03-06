using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionEffect : MonoBehaviour
{
    // 动画帧事件，特效播放完毕后销毁自身
    public void Destory()
    {
        Destroy(gameObject);
    }
}