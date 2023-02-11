using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    /// <summary>
    /// 角色输入轴
    /// </summary>
    public Vector2 axis;
    /// <summary>
    /// 角色受到的重力
    /// </summary>
    public float gravity = 10;
    /// <summary>
    /// 角色的移动速度
    /// </summary>
    public float moveSpeed = 30;
    /// <summary>
    /// 角色跳跃力
    /// </summary>
    public float jumpForce;
    /// <summary>
    /// 中心点偏移
    /// </summary>
    public Vector3 offset;
    /// <summary>
    /// 最大速度向量
    /// </summary>
    public float maxSpeed = 100;
    /// <summary>
    /// 前长度：该值决定角色到前墙的碰撞距离
    /// </summary>
    public float forwardLength = 1;
    /// <summary>
    /// 底长度：该值决定角色到地面的距离
    /// </summary>
    public float bottonLnegth = 0.5f;
    /// <summary>
    /// 底下的先与旁边的线偏移距离
    /// </summary>
    public float bottonForwardOffset = 0.25f;

    /// <summary>
    /// 上一次跳跃的到现在时间
    /// </summary>
    public float JumpTime { get => jumpTime; }
    /// <summary>
    /// 返回X轴的绝对值
    /// </summary>
    public float AxisAbs { get => axisAbs; }
    /// <summary>
    /// 角色是否进行了X反转
    /// </summary>
    public bool ReversalX { get => reversalX; }
    /// <summary>
    /// 角色是否在地面
    /// </summary>
    public bool OnGround { get => onGround; }
    /// <summary>
    /// 角色的实际位移速度
    /// </summary>
    public Vector3 Velocity { get => velocity; }
    /// <summary>
    /// 获取射线起始点
    /// </summary>
    public Vector3 RaycastStartPosi { get { return transform.position + offset; } }
    /// <summary>
    /// 获取X的速度长度(Mathf.Abs(velocity.x) * Time.deltaTime;)
    /// </summary>
    public float SpeedLengthX { get { return Mathf.Abs(velocity.x) * Time.deltaTime; } }
    /// <summary>
    /// 获取Y的速度长度(Mathf.Abs(velocity.y) * Time.deltaTime;)
    /// </summary>
    public float SpeedLengthY { get { return Mathf.Abs(velocity.y) * Time.deltaTime; } }
    /// <summary>
    /// 是否打开前方检测
    /// </summary>
    public bool OpenForward { get => openForward; set => openForward = value; }
    /// <summary>
    /// 是否打开自由浮空
    /// </summary>
    public bool FreeFloating { get => freeFloating; set => freeFloating = value; }
    /// <summary>
    /// 获取角色前面
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
    /// 绘制碰撞检测距离
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
        //刷新角色的速度
        VelocityUpdate();

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }


    /// <summary>
    /// 跳跃
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


            //如果是在平台上,并且下跳平台不为空
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

                //速度方向
                Vector2 speedDirection = (forwardHit.point - bottonHit.point).normalized;

                //坡度检查
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
        //在空中时
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
        //如果顶部撞到墙并没撞到、或撞到的不是平台
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
            //判断是否撞墙X
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
        //前射线位置
        Vector3 forward = RaycastStartPosi + Forward * bottonForwardOffset;
        //发射射线
        bottonHit = Physics2D.Raycast(RaycastStartPosi, Vector2.down, bottonLnegth + SpeedLengthY, LayerMask.GetMask("Ground"));
        forwardHit = Physics2D.Raycast(forward, Vector2.down, bottonLnegth + SpeedLengthY, LayerMask.GetMask("Ground"));
    }

    public RaycastHit2D GetForwardHit()
    {
        RaycastHit2D rf = Physics2D.Raycast(RaycastStartPosi, Forward, forwardLength + SpeedLengthX, LayerMask.GetMask("Ground"));
        return rf;
    }
}