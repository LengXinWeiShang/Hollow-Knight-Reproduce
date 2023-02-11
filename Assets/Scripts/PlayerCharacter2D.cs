using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter2D : MonoBehaviour
{
    // 角色所受的重力
    public float gravity = 10f;
    // 角色的移动速度
    public float moveSpeed = 6f;
    // 角色的跳跃力
    public float jumpForce = 8f;
    // 最大速度
    public float maxSpeed = 10f;
    // 角色前方碰撞检测射线的长度
    public float forwardLength = 0.1f;
    // 角色底部碰撞检测射线的长度
    public float bottonLength = 0.1f;
    // 角色顶部碰撞检测射线的长度
    public float topLength = 0.1f;
    // 长按跳跃键的最长生效时间
    public float jumpholdmaxtime = 0.5f;

    // 角色控制器
    private PlayerController controller;
    // 动画状态机
    private Animator anim;
    // 角色是否朝向左方
    private bool faceLeft = true;
    // 角色的速度
    private Vector3 velocity;
    // 角色前方碰撞检测射线的起点
    private Vector3 FwdRaycastStartPos { get { return transform.position + new Vector3(-0.25f, 0, 0); } }
    // 角色底部碰撞检测射线的起点
    private Vector3 BottonRaycastStartPos { get { return transform.position + new Vector3(0, -0.6f, 0); } }
    // 角色顶部碰撞检测射线的起点
    private Vector3 TopRaycastStartPos { get { return transform.position + new Vector3(0, 0.25f, 0); } }
    // 获取角色前方方向向量
    private Vector3 Forward
    {
        get
        {
            Vector3 forward = transform.right;
            forward.x = faceLeft ? forward.x * -1 : forward.x;
            return forward;
        }
    }
    // 角色是否在地面
    private bool onGround;
    // 角色上一次跳跃到现在的时间
    private float jumpTime;
    
    private void OnDrawGizmos()
    {
        // 前方射线
        Debug.DrawRay(FwdRaycastStartPos, Forward * forwardLength, Color.green);
        // 底部射线
        Debug.DrawRay(BottonRaycastStartPos, Vector2.down * bottonLength, Color.red);
        // 顶部射线
        Debug.DrawRay(TopRaycastStartPos, Vector2.up * topLength, Color.yellow);
    }
    void Start()
    {
        controller = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 执行移动
        Move(controller.h);
        // 攻击
        Attack();
    }

    // 处理角色移动相关
    private void Move(float h)
    {
        // 检测是否在地面
        CheckGround();
        // 控制角色翻转
        Flip(h);
        // 处理跳跃
        Jump();
        // 刷新角色速度
        UpdateVelocity(h);
        // 根据速度向量移动角色
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }

    // 判断角色是否在地面上
    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(BottonRaycastStartPos, Vector2.down, bottonLength, LayerMask.GetMask("Ground"));
        onGround = hit ? true : false;
    }

    // 根据横向输入翻转角色
    private void Flip(float h)
    {
        if (h > 0.1f)
        {
            faceLeft = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (h < -0.1f)
        {
            faceLeft = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // 处理跳跃
    private void Jump()
    {
        jumpTime += Time.deltaTime;
        // 起跳动作
        if (onGround && controller.startJump)
        {
            onGround = false;
            jumpTime = 0;
            // 在地面上才能跳跃
            velocity.y = jumpForce;
            // 更新动画
            anim.SetBool("IsJump", true);
        }
        if (velocity.y > 0)
        {
            // 上升过程中持续按下跳跃键时加大跳跃高度（不能超过最大时间限制），松开按键时向上的速度置0
            velocity.y = controller.endJump || jumpTime > jumpholdmaxtime ? 0 : jumpForce;
        }
    }

    // 根据输入刷新角色速度
    private void UpdateVelocity(float h)
    {
        if (onGround)
        {
            // 在地面，y轴速度置为0
            velocity.y = 0;
            anim.SetBool("IsJump", false);
        }
        else
        {
            // 在空中，模拟重力影响
            velocity.y -= gravity * Time.deltaTime;
        }
        // 横向输入
        velocity.x = h * moveSpeed;
        // 更新动画
        anim.SetBool("IsGround", onGround);
        anim.SetFloat("Speed", Mathf.Abs(controller.h));
    }

    // 处理攻击相关
    private void Attack()
    {
        if (controller.normalSlash)
        {
            anim.SetTrigger("Slash");
        }
    }
}
