using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    /// <summary>
    /// ��ɫ������
    /// </summary>
    public Vector2 axis;
    /// <summary>
    /// ��ɫ�ܵ�������
    /// </summary>
    public float gravity = 10;
    /// <summary>
    /// ��ɫ���ƶ��ٶ�
    /// </summary>
    public float moveSpeed = 30;
    /// <summary>
    /// ��ɫ��Ծ��
    /// </summary>
    public float jumpForce;
    /// <summary>
    /// ���ĵ�ƫ��
    /// </summary>
    public Vector3 offset;
    /// <summary>
    /// ����ٶ�����
    /// </summary>
    public float maxSpeed = 100;
    /// <summary>
    /// ǰ���ȣ���ֵ������ɫ��ǰǽ����ײ����
    /// </summary>
    public float forwardLength = 1;
    /// <summary>
    /// �׳��ȣ���ֵ������ɫ������ľ���
    /// </summary>
    public float bottonLnegth = 0.5f;
    /// <summary>
    /// ���µ������Աߵ���ƫ�ƾ���
    /// </summary>
    public float bottonForwardOffset = 0.25f;

    /// <summary>
    /// ��һ����Ծ�ĵ�����ʱ��
    /// </summary>
    public float JumpTime { get => jumpTime; }
    /// <summary>
    /// ����X��ľ���ֵ
    /// </summary>
    public float AxisAbs { get => axisAbs; }
    /// <summary>
    /// ��ɫ�Ƿ������X��ת
    /// </summary>
    public bool ReversalX { get => reversalX; }
    /// <summary>
    /// ��ɫ�Ƿ��ڵ���
    /// </summary>
    public bool OnGround { get => onGround; }
    /// <summary>
    /// ��ɫ��ʵ��λ���ٶ�
    /// </summary>
    public Vector3 Velocity { get => velocity; }
    /// <summary>
    /// ��ȡ������ʼ��
    /// </summary>
    public Vector3 RaycastStartPosi { get { return transform.position + offset; } }
    /// <summary>
    /// ��ȡX���ٶȳ���(Mathf.Abs(velocity.x) * Time.deltaTime;)
    /// </summary>
    public float SpeedLengthX { get { return Mathf.Abs(velocity.x) * Time.deltaTime; } }
    /// <summary>
    /// ��ȡY���ٶȳ���(Mathf.Abs(velocity.y) * Time.deltaTime;)
    /// </summary>
    public float SpeedLengthY { get { return Mathf.Abs(velocity.y) * Time.deltaTime; } }
    /// <summary>
    /// �Ƿ��ǰ�����
    /// </summary>
    public bool OpenForward { get => openForward; set => openForward = value; }
    /// <summary>
    /// �Ƿ�����ɸ���
    /// </summary>
    public bool FreeFloating { get => freeFloating; set => freeFloating = value; }
    /// <summary>
    /// ��ȡ��ɫǰ��
    /// </summary>
    public Vector3 Forward
    {
        get
        {
            Vector3 forward = transform.right;
            forward.x = ReversalX ? forward.x * -1 : forward.x;
            return forward;
        }
    }



    private bool reversalX;
    private float axisAbs;
    private bool onGround;
    private float jumpTime;
    private bool openForward = true;
    private bool freeFloating = true;
    private Vector3 velocity;
    private Transform downJumpPlatform;

    /// <summary>
    /// ������ײ������
    /// </summary>
    private void OnDrawGizmos()
    {
        //f
        Debug.DrawRay(RaycastStartPosi, Forward * forwardLength, Color.green);
        //b
        Vector3 forward = RaycastStartPosi + Forward * bottonForwardOffset;
        Debug.DrawRay(RaycastStartPosi, Vector2.down * bottonLnegth, Color.green);
        Debug.DrawRay(forward, Vector2.down * bottonLnegth, Color.green);
        //t
        Debug.DrawRay(RaycastStartPosi, Vector2.up * bottonLnegth, Color.green);
    }

    void Update()
    {
        if (axis.x < 0)
        {
            reversalX = true;
        }
        else if (axis.x > 0)
        {
            reversalX = false;
        }
        jumpTime += Time.deltaTime;
        axisAbs = Mathf.Abs(axis.x);
        //ˢ�½�ɫ���ٶ�
        VelocityUpdate();

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }


    /// <summary>
    /// ��Ծ
    /// </summary>
    public void Jump()
    {
        if (OnGround)
        {
            onGround = false;
            jumpTime = 0;
            RaycastHit2D bottonHit = Physics2D.Raycast(RaycastStartPosi, Vector2.down, bottonLnegth + SpeedLengthY, LayerMask.GetMask("Ground"));
            if (bottonHit && bottonHit.transform.tag == "Plane" && axis.y < 0)
            {
                downJumpPlatform = bottonHit.transform;
            }
            else
            {
                velocity.y = jumpForce;
            }

        }
    }

    private void VelocityUpdate()
    {
        RaycastHit2D bottonHit, forwardHit;

        GetBottonRaycastHit(out bottonHit, out forwardHit);

        if (jumpTime > 0.2 && bottonHit && forwardHit)
        {


            //�������ƽ̨��,��������ƽ̨��Ϊ��
            if (downJumpPlatform)
            {
                if (downJumpPlatform != bottonHit.transform)
                {
                    downJumpPlatform = null;
                }
            }
            else
            {
                onGround = true;

                //�ٶȷ���
                Vector2 speedDirection = (forwardHit.point - bottonHit.point).normalized;

                //�¶ȼ��
                if (Vector3.Dot(speedDirection, Vector2.up) < 0.6f)
                {
                    velocity = speedDirection * AxisAbs * moveSpeed;
                }

                velocity.y = 0;
                transform.position = new Vector3(
                    transform.position.x,
                  (bottonHit.point.y <= forwardHit.point.y ? bottonHit.point.y : forwardHit.point.y) + bottonLnegth - bottonLnegth * 0.1f - offset.y,
                    transform.position.z);
            }


        }
        //�ڿ���ʱ
        else
        {
            onGround = false;
            if (freeFloating)
            {
                velocity.x = axis.x * moveSpeed;
            }
            velocity.y -= gravity * Time.deltaTime;
        }

        RaycastHit2D topHit = Physics2D.Raycast(RaycastStartPosi, Vector2.up, bottonLnegth + SpeedLengthY, LayerMask.GetMask("Ground"));
        //�������ײ��ǽ��ûײ������ײ���Ĳ���ƽ̨
        if (!topHit || topHit.transform.tag != "Plane")
        {
            if (topHit)
            {
                velocity.y = 0;
            }
            FudgeForward();
        }
    }



    private void FudgeForward()
    {
        if (openForward)
        {
            //�ж��Ƿ�ײǽX
            RaycastHit2D rf = GetForwardHit();
            if (rf)
            {

                velocity.x = 0;
                float x = reversalX ? forwardLength - 0.1f : -forwardLength + 0.1f;
                transform.position = new Vector3(
                rf.point.x + x - offset.x,
                transform.position.y,
                transform.position.z);
            }
        }
    }

    private void GetBottonRaycastHit(out RaycastHit2D bottonHit, out RaycastHit2D forwardHit)
    {
        //ǰ����λ��
        Vector3 forward = RaycastStartPosi + Forward * bottonForwardOffset;
        //��������
        bottonHit = Physics2D.Raycast(RaycastStartPosi, Vector2.down, bottonLnegth + SpeedLengthY, LayerMask.GetMask("Ground"));
        forwardHit = Physics2D.Raycast(forward, Vector2.down, bottonLnegth + SpeedLengthY, LayerMask.GetMask("Ground"));
    }

    public RaycastHit2D GetForwardHit()
    {
        RaycastHit2D rf = Physics2D.Raycast(RaycastStartPosi, Forward, forwardLength + SpeedLengthX, LayerMask.GetMask("Ground"));
        return rf;
    }
}