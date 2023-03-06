using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour
{
    public Thread prefabNeedleThread;
    public float speed;
    public bool moveLeft = true;
    private float initSpeed = 60f;          // 初始速度
    private float acceleration = 90f;       // 加速度
    private float movedTime = 0;            // 移动的时间
    private bool hasThread = false;         // 是否已创建丝线

    private void Awake()
    {
        speed = initSpeed;
    }

    private void FixedUpdate()
    {
        speed = initSpeed - acceleration * movedTime;
        Vector3 move = transform.right * Time.fixedDeltaTime * speed;
        transform.position -= move;
        movedTime += Time.fixedDeltaTime;
        if (Mathf.Abs(speed) < 1f && !hasThread)
        {
            hasThread = true;
            Thread thread = Instantiate(prefabNeedleThread, transform.position + transform.right * 8, Quaternion.identity);
            thread.transform.SetParent(transform);
            if (!moveLeft)
            {
                thread.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}