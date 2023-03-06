using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Move,           // �ƶ�
    Dash,           // ���
    Jump,           // ��Ծ
    Air,            // ���еĽ���׶Σ�������Ծ����½�������ƽ̨���½���
    Heal,           // �ۼ����ۼ�����ʱ�ظ�Ѫ��
    Stun,           // �ܻ�Ӳֱ
    Attack,         // ����
    Skill,          // �ͷż���
    Death,          // ����
}

public class PlayerCharacter2D : MonoBehaviour
{
    // �������ֵ
    public int maxHp = 5;

    // ��ɫ���ƶ��ٶ�
    public float moveSpeed = 8.3f;

    // ��ɫ���ܻ�Ӳֱ������֡��
    public int hitOutOfControlTime = 15;

    // ��ɫ�ܻ�����޵�ʱ�䣨����֡��
    public int invincibleTime = 30;

    // ��ɫ�ĳ���ٶ�
    public float dashSpeed = 16f;

    // ��ɫ�ĳ��ʱ��
    public float dashTime = 0.15f;

    // ��ɫ����ʱ���ٶ�
    public float jumpYSpeed = 15.7f;

    // ��ɫ��Ծ������ʱ���ܵ��������ٶ�
    public float accelerationY = 47.5f;

    // ��������ٶ�
    public float maxYSpeed = 20.9f;

    // ��ɫǰ����ײ������ߵĳ���
    public float forwardLength = 0.1f;

    // ��ɫ�ײ���ײ������ߵĳ���
    public float bottonLength = 0.1f;

    // ��ɫ������ײ������ߵĳ���
    public float topLength = 0.1f;

    // ��������ʱ��
    public float maxSpeedJumpTime = 0.2f;

    // ������Ծ�������Чʱ��
    public float jumpMaxTime = 0.52f;

    // ��Ծ�����ʱ��
    public float jumpMinTime = 0.08f;

    // �ܻ���Ч��ͼ
    public Transform prefabHurtFlash;

    // ��ǰѪ��
    private int hp;

    // ��ɫ��ǰ״̬
    private PlayerState state;

    // ��ɫ������
    private PlayerController controller;

    // ����״̬��
    private Animator anim;

    // ��ɫ�Ƿ�����
    public bool faceLeft = false;

    // ��ɫ���ٶ�
    private Vector3 velocity;

    // �Ƿ��ڰ�ס��Ծ������״̬
    private bool isJump = false;

    // �洢�Ƿ��ɿ���Ծ��
    private bool endJump = false;

    // �ܻ�ʱ��Ӳֱʱ�䣨����֡��
    private int outControlTime = 0;

    // �ܻ�����޵�ʱ�䣨����֡��
    private int curInvincibleTime = 0;

    // ��ɫǰ����ײ������ߵ����
    private Vector3 FwdRaycastStartPos
    { get { return transform.position + new Vector3(-0.25f, 0, 0); } }

    // ��ɫ�ײ���ײ������ߵ����
    private Vector3 BottonRaycastStartPos
    { get { return transform.position + new Vector3(0, -1.2f, 0); } }

    // ��ɫ������ײ������ߵ����
    private Vector3 TopRaycastStartPos
    { get { return transform.position + new Vector3(0, 0.4f, 0); } }

    // ��ȡ��ɫǰ����������
    private Vector3 Forward
    {
        get
        {
            Vector3 forward = transform.right;
            forward.x = faceLeft ? forward.x * -1 : forward.x;
            return forward;
        }
    }

    // ��ɫ�Ƿ��ڵ���
    private bool onGround;

    // ��ʼ��Ծ�����ڵ�ʱ��
    private float jumpTime;

    private void OnDrawGizmos()
    {
        // ǰ������
        Debug.DrawRay(FwdRaycastStartPos, Forward * forwardLength, Color.green);
        // �ײ�����
        Debug.DrawRay(BottonRaycastStartPos, Vector2.down * bottonLength, Color.red);
        // ��������
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
        // ���º��������С
        anim.SetFloat("Speed", Mathf.Abs(controller.h));
        // �����ٶ������ƶ���ɫ
        velocity.y = Mathf.Clamp(velocity.y, -1 * maxYSpeed, jumpYSpeed);
        transform.position += velocity * Time.fixedDeltaTime;
    }

    // �����ɫ�ƶ����
    private void Move()
    {
        // ���ƽ�ɫ��ת
        Flip(controller.h);
        // ��������
        velocity.x = controller.h * moveSpeed;
    }

    // ���
    private void Dash()
    {
    }

    // ��Ծ�������׶�
    private void Jump()
    {
        // ���ƽ�ɫ��ת
        Flip(controller.h);
        // ��������
        velocity.x = controller.h * moveSpeed;
        // �ɿ���Ծ���ٶȹ��㣨��������0.08s��
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
                // ��Ծʱǰһ��ʱ�䲻����
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

    // ����
    private void Fall()
    {
        // ���ƽ�ɫ��ת
        Flip(controller.h);
        // ��������
        velocity.x = controller.h * moveSpeed;
        velocity.y -= accelerationY * Time.fixedDeltaTime;
    }

    // ����
    private void Attack()
    {
        // ���ƽ�ɫ��ת
        Flip(controller.h);
        // ��������
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

    // �ܻ�
    private void Stun()
    {
        outControlTime--;
        if (outControlTime <= 0)
        {
            anim.SetTrigger("EndStun");
        }
    }

    // �жϽ�ɫ�Ƿ��ڵ�����
    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(BottonRaycastStartPos, Vector2.down, bottonLength, LayerMask.GetMask("Ground"));
        onGround = hit ? true : false;
    }

    // ���ݺ������뷭ת��ɫ
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
        // �ܻ���Ч
        Instantiate(prefabHurtFlash, transform.position, Quaternion.identity);
        UIManager.Instance.FadeHurtImage();
        // ����ʱ��
        GameManager.Instance.FreezeTime(0.02f);

        // ����Ѫ��UI
        UIManager.Instance.LostHpBall(hp);

        if (hp == 0)
        {
            anim.SetTrigger("Death");
            GameManager.Instance.QuitFight();
            return;
        }

        // ���ö���
        anim.SetTrigger("GetHit");

        // ����ʱ�򷴷��򵯷�
        velocity.x = faceLeft ? 3 : -3;
        velocity.y = 0.5f;

        // ��ɫ����Ӳֱ
        outControlTime = hitOutOfControlTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("BOSS"))
        {
            GetHit(1);
        }
    }

    #region ����״̬�����ɶ���״̬������

    private delegate void FuncStateEnter(int n);

    private delegate void FuncStateUpdate(int n);

    private delegate void FuncStateExit(int n);

    private Dictionary<PlayerState, FuncStateEnter> dictStateEnter;
    private Dictionary<PlayerState, FuncStateUpdate> dictStateUpdate;
    private Dictionary<PlayerState, FuncStateExit> dictStateExit;

    // ���붯��״̬ʱִ�еķ���
    public void OnAnimStateEnter(PlayerState s, int n)
    {
        if (dictStateEnter == null)
        {
            // ����ʹ��ʱ��ʼ���ֵ䣬���������ֵ�ͬ��
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

        // ���ö�Ӧ״̬�ķ���
        if (dictStateEnter.ContainsKey(s) && dictStateEnter[s] != null)
        {
            dictStateEnter[s](n);
        }
    }

    // ����״̬������ִ�еķ���
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

    // ����״̬�˳�ʱִ�еķ���
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
            // ��Ծ
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
        // ����Ƿ��ڵ���
        CheckGround();
        if (!onGround)
        {
            // ����ƽ̨
            anim.SetTrigger("Fall");
        }
        if (controller.startJump && onGround)
        {
            // �����ϲ�����Ծ���ݲ�֧�ֶ�������
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
        // ����Ƿ����
        CheckGround();
        if (onGround && jumpTime > jumpMinTime)
        {
            anim.SetTrigger("Land");
        }
        if (controller.endJump)
        {
            // ��¼�ɿ���Ծ��
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
        // ����Ƿ����
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
        // ����Ƿ��ڵ���
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

    #endregion ����״̬�����ɶ���״̬������
}