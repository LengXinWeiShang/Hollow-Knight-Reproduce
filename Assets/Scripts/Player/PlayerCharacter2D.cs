using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Move,           // 移动
    Dash,           // 冲刺
    Jump,           // 跳跃
    Air,            // 空中的降落阶段（包括跳跃后的下降和落下平台的下降）
    Heal,           // 聚集，聚集结束时回复血量
    Stun,           // 受击硬直
    Attack,         // 攻击
    Skill,          // 释放技能
    Death,          // 死亡
}

public class PlayerCharacter2D : MonoBehaviour
{
    // 最大生命值
    public int maxHp = 5;

    // 角色的移动速度
    public float moveSpeed = 8.3f;

    // 角色的受击硬直（物理帧）
    public int hitOutOfControlTime = 15;

    // 角色受击后的无敌时间（物理帧）
    public int invincibleTime = 30;

    // 角色的冲刺速度
    public float dashSpeed = 16f;

    // 角色的冲刺时长
    public float dashTime = 0.15f;

    // 角色起跳时的速度
    public float jumpYSpeed = 15.7f;

    // 角色跳跃和下落时所受的重力加速度
    public float accelerationY = 47.5f;

    // 最大下落速度
    public float maxYSpeed = 20.9f;

    // 角色前方碰撞检测射线的长度
    public float forwardLength = 0.1f;

    // 角色底部碰撞检测射线的长度
    public float bottonLength = 0.1f;

    // 角色顶部碰撞检测射线的长度
    public float topLength = 0.1f;

    // 满速上升时间
    public float maxSpeedJumpTime = 0.2f;

    // 长按跳跃键的最长生效时间
    public float jumpMaxTime = 0.52f;

    // 跳跃的最短时长
    public float jumpMinTime = 0.08f;

    // 受击特效贴图
    public Transform prefabHurtFlash;

    // 当前血量
    private int hp;

    // 角色当前状态
    private PlayerState state;

    // 角色控制器
    private PlayerController controller;

    // 动画状态机
    private Animator anim;

    // 角色是否朝向左方
    public bool faceLeft = false;

    // 角色的速度
    private Vector3 velocity;

    // 是否处于按住跳跃的上升状态
    private bool isJump = false;

    // 存储是否松开跳跃键
    private bool endJump = false;

    // 受击时的硬直时间（物理帧）
    private int outControlTime = 0;

    // 受击后的无敌时间（物理帧）
    private int curInvincibleTime = 0;

    // 角色前方碰撞检测射线的起点
    private Vector3 FwdRaycastStartPos
    { get { return transform.position + new Vector3(-0.25f, 0, 0); } }

    // 角色底部碰撞检测射线的起点
    private Vector3 BottonRaycastStartPos
    { get { return transform.position + new Vector3(0, -1.2f, 0); } }

    // 角色顶部碰撞检测射线的起点
    private Vector3 TopRaycastStartPos
    { get { return transform.position + new Vector3(0, 0.4f, 0); } }

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

    // 开始跳跃到现在的时间
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

    private void Start()
    {
        hp = maxHp;
        controller = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case PlayerState.Move:
                Move();
                break;

            case PlayerState.Dash:
                Dash();
                break;

            case PlayerState.Jump:
                Jump();
                break;

            case PlayerState.Air:
                Fall();
                break;

            case PlayerState.Attack:
                Attack();
                break;

            case PlayerState.Stun:
                Stun();
                break;
        }
        if (curInvincibleTime > 0)
        {
            curInvincibleTime--;
        }
        // 更新横向输入大小
        anim.SetFloat("Speed", Mathf.Abs(controller.h));
        // 根据速度向量移动角色
        velocity.y = Mathf.Clamp(velocity.y, -1 * maxYSpeed, jumpYSpeed);
        transform.position += velocity * Time.fixedDeltaTime;
    }

    // 处理角色移动相关
    private void Move()
    {
        // 控制角色翻转
        Flip(controller.h);
        // 横向输入
        velocity.x = controller.h * moveSpeed;
    }

    // 冲刺
    private void Dash()
    {
    }

    // 跳跃的上升阶段
    private void Jump()
    {
        // 控制角色翻转
        Flip(controller.h);
        // 横向输入
        velocity.x = controller.h * moveSpeed;
        // 松开跳跃键速度归零（最少上升0.08s）
        if (isJump)
        {
            if ((endJump && jumpTime > jumpMinTime) || jumpTime > jumpMaxTime)
            {
                endJump = false;
                isJump = false;
                velocity.y = 0;
            }
            else if (jumpTime <= maxSpeedJumpTime)
            {
                // 跳跃时前一段时间不减速
                velocity.y = jumpYSpeed;
            }
            else
            {
                velocity.y -= accelerationY * Time.fixedDeltaTime;
            }
        }
        else
        {
            velocity.y -= accelerationY * Time.fixedDeltaTime;
        }
        jumpTime += Time.fixedDeltaTime;
    }

    // 下落
    private void Fall()
    {
        // 控制角色翻转
        Flip(controller.h);
        // 横向输入
        velocity.x = controller.h * moveSpeed;
        velocity.y -= accelerationY * Time.fixedDeltaTime;
    }

    // 攻击
    private void Attack()
    {
        // 控制角色翻转
        Flip(controller.h);
        // 横向输入
        velocity.x = controller.h * moveSpeed;
        if (onGround)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y -= accelerationY * Time.fixedDeltaTime;
        }
    }

    // 受击
    private void Stun()
    {
        outControlTime--;
        if (outControlTime <= 0)
        {
            anim.SetTrigger("EndStun");
        }
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

    public void GetHit(int damage)
    {
        if (state == PlayerState.Stun || state == PlayerState.Death || curInvincibleTime > 0)
        {
            return;
        }
        hp = Mathf.Clamp(hp - damage, 0, maxHp);
        // 受击特效
        Instantiate(prefabHurtFlash, transform.position, Quaternion.identity);
        UIManager.Instance.FadeHurtImage();
        // 冻结时间
        GameManager.Instance.FreezeTime(0.02f);

        // 更新血条UI
        UIManager.Instance.LostHpBall(hp);

        if (hp == 0)
        {
            anim.SetTrigger("Death");
            GameManager.Instance.QuitFight();
            return;
        }

        // 设置动画
        anim.SetTrigger("GetHit");

        // 受伤时向反方向弹飞
        velocity.x = faceLeft ? 3 : -3;
        velocity.y = 0.5f;

        // 角色进入硬直
        outControlTime = hitOutOfControlTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("BOSS"))
        {
            GetHit(1);
        }
    }

    #region 核心状态机，由动画状态机驱动

    private delegate void FuncStateEnter(int n);

    private delegate void FuncStateUpdate(int n);

    private delegate void FuncStateExit(int n);

    private Dictionary<PlayerState, FuncStateEnter> dictStateEnter;
    private Dictionary<PlayerState, FuncStateUpdate> dictStateUpdate;
    private Dictionary<PlayerState, FuncStateExit> dictStateExit;

    // 进入动画状态时执行的方法
    public void OnAnimStateEnter(PlayerState s, int n)
    {
        if (dictStateEnter == null)
        {
            // 初次使用时初始化字典，其余两个字典同理
            dictStateEnter = new Dictionary<PlayerState, FuncStateEnter>
            {
                {PlayerState.Move, MoveEnter},
                {PlayerState.Dash, DashEnter},
                {PlayerState.Jump, JumpEnter},
                {PlayerState.Air, AirEnter},
                {PlayerState.Heal, HealEnter},
                {PlayerState.Stun, StunEnter},
                {PlayerState.Attack, AttackEnter},
                {PlayerState.Skill, SkillEnter},
                {PlayerState.Death, DeathEnter},
            };
        }

        // 调用对应状态的方法
        if (dictStateEnter.ContainsKey(s) && dictStateEnter[s] != null)
        {
            dictStateEnter[s](n);
        }
    }

    // 动画状态过程中执行的方法
    public void OnAnimStateUpdate(PlayerState s, int n)
    {
        if (dictStateUpdate == null)
        {
            dictStateUpdate = new Dictionary<PlayerState, FuncStateUpdate>
            {
                {PlayerState.Move, MoveUpdate},
                {PlayerState.Jump, JumpUpdate},
                {PlayerState.Air, AirUpdate},
                {PlayerState.Dash, DashUpdate},
                {PlayerState.Attack, AttackUpdate},
                {PlayerState.Stun, StunUpdate},
            };
        }

        if (dictStateUpdate.ContainsKey(s) && dictStateUpdate[s] != null)
        {
            dictStateUpdate[s](n);
        }
    }

    // 动画状态退出时执行的方法
    public void OnAnimStateExit(PlayerState s, int n)
    {
        if (dictStateExit == null)
        {
            dictStateExit = new Dictionary<PlayerState, FuncStateExit>
            {
                {PlayerState.Move, MoveExit},
                {PlayerState.Jump, JumpExit},
                {PlayerState.Attack, AttackExit},
                {PlayerState.Air, AirExit},
            };
        }

        if (dictStateExit.ContainsKey(s) && dictStateExit[s] != null)
        {
            dictStateExit[s](n);
        }
    }

    // ---------- Enter ----------
    public void MoveEnter(int n)
    {
        velocity.y = 0;
        state = PlayerState.Move;
    }

    public void DashEnter(int n)
    {
        velocity.x = (Forward * dashSpeed).x;
        velocity.y = 0;
        dashTime = 0;
        state = PlayerState.Dash;
    }

    public void JumpEnter(int n)
    {
        state = PlayerState.Jump;
        if (n == 0)
        {
            isJump = true;
            endJump = false;
            // 跳跃
            onGround = false;
            jumpTime = 0;
        }
    }

    public void AirEnter(int n)
    {
        isJump = false;
        state = PlayerState.Air;
    }

    public void HealEnter(int n)
    {
        state = PlayerState.Heal;
    }

    public void StunEnter(int n)
    {
        state = PlayerState.Stun;
        velocity.x = faceLeft ? 3 : -3;
        velocity.y = 3;
    }

    public void AttackEnter(int n)
    {
        state = PlayerState.Attack;
    }

    public void SkillEnter(int n)
    {
        state = PlayerState.Skill;
    }

    public void DeathEnter(int n)
    {
        state = PlayerState.Death;
        velocity = Vector3.zero;
    }

    // ---------- Update ----------
    public void MoveUpdate(int n)
    {
        // 检测是否在地面
        CheckGround();
        if (!onGround)
        {
            // 掉下平台
            anim.SetTrigger("Fall");
        }
        if (controller.startJump && onGround)
        {
            // 地面上才能跳跃（暂不支持二段跳）
            anim.SetTrigger("Jump");
        }
        else if (controller.dash)
        {
            anim.SetTrigger("Dash");
        }
        else if (controller.normalSlash)
        {
            anim.SetTrigger("NormalSlash");
        }
        else if (controller.upSlash)
        {
            anim.SetTrigger("UpSlash");
        }
        else if (controller.downSlash)
        {
            anim.SetTrigger("DownSlash");
        }
        else if (controller.vengefulSpirit)
        {
            anim.SetTrigger("VengefulSpirit");
        }
        else if (controller.howlingWraiths)
        {
            anim.SetTrigger("HowlingWraiths");
        }
    }

    public void JumpUpdate(int n)
    {
        // 检查是否落地
        CheckGround();
        if (onGround && jumpTime > jumpMinTime)
        {
            anim.SetTrigger("Land");
        }
        if (controller.endJump)
        {
            // 记录松开跳跃键
            endJump = true;
        }
        else if (controller.dash)
        {
            anim.SetTrigger("Dash");
        }
        else if (controller.normalSlash)
        {
            anim.SetTrigger("NormalSlash");
        }
    }

    public void AirUpdate(int n)
    {
        // 检查是否落地
        CheckGround();
        if (onGround)
        {
            anim.SetTrigger("Land");
        }
        if (controller.dash)
        {
            anim.SetTrigger("Dash");
        }
        else if (controller.normalSlash)
        {
            anim.SetTrigger("NormalSlash");
        }
    }

    public void DashUpdate(int n)
    {
    }

    public void AttackUpdate(int n)
    {
        // 检测是否在地面
        CheckGround();
        if (controller.normalSlash)
        {
            anim.SetTrigger("NormalSlash");
        }
        if (!onGround)
        {
            anim.SetTrigger("Fall");
        }
    }

    public void StunUpdate(int n)
    {
    }

    // ---------- Exit ----------
    public void MoveExit(int n)
    {
    }

    public void DashExit(int n)
    {
        velocity.x = controller.h;
    }

    public void JumpExit(int n)
    {
        anim.ResetTrigger("NormalSlash");
    }

    public void AirExit(int n)
    {
    }

    public void HealExit(int n)
    {
    }

    public void StunExit(int n)
    {
    }

    public void AttackExit(int n)
    {
        if (n == 2)
        {
            anim.ResetTrigger("NormalSlash");
        }
        else if (n == 3)
        {
            anim.ResetTrigger("DownSlash");
        }
        else if (n == 4)
        {
            anim.ResetTrigger("UpSlash");
        }
    }

    public void SkillExit(int n)
    {
    }

    #endregion 核心状态机，由动画状态机驱动
}