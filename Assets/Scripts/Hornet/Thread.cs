using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thread : MonoBehaviour
{
    // 动画帧事件，播放完后即销毁自身
    private void Destroy()
    {
        Destroy(gameObject);
    }
}