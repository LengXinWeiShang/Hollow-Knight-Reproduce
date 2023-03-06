using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour
{
    public Thread prefabNeedleThread;
    public float speed;
    public bool moveLeft = true;
    private float initSpeed = 60f;          // ��ʼ�ٶ�
    private float acceleration = 90f;       // ���ٶ�
    private float movedTime = 0;            // �ƶ���ʱ��
    private bool hasThread = false;         // �Ƿ��Ѵ���˿��

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