using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter2D : MonoBehaviour
{
    // ��ɫ���ܵ�����
    public float gravity = 10f;
    // ��ɫ���ƶ��ٶ�
    public float moveSpeed = 6f;
    // ��ɫ����Ծ��
    public float jumpForce = 8f;
    // ����ٶ�
    public float maxSpeed = 10f;
    // ��ɫǰ����ײ������ߵĳ���
    public float forwardLength = 0.1f;
    // ��ɫ�ײ���ײ������ߵĳ���
    public float bottonLength = 0.1f;
    // ��ɫ������ײ������ߵĳ���
    public float topLength = 0.1f;
    // ������Ծ�������Чʱ��
    public float jumpholdmaxtime = 0.5f;

    // ��ɫ������
    private PlayerController controller;
    // ����״̬��
    private Animator anim;
    // ��ɫ�Ƿ�����
    private bool faceLeft = true;
    // ��ɫ���ٶ�
    private Vector3 velocity;
    // ��ɫǰ����ײ������ߵ����
    private Vector3 FwdRaycastStartPos { get { return transform.position + new Vector3(-0.25f, 0, 0); } }
    // ��ɫ�ײ���ײ������ߵ����
    private Vector3 BottonRaycastStartPos { get { return transform.position + new Vector3(0, -0.6f, 0); } }
    // ��ɫ������ײ������ߵ����
    private Vector3 TopRaycastStartPos { get { return transform.position + new Vector3(0, 0.25f, 0); } }
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
    // ��ɫ��һ����Ծ�����ڵ�ʱ��
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
    void Start()
    {
        controller = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // ִ���ƶ�
        Move(controller.h);
        // ����
        Attack();
    }

    // �����ɫ�ƶ����
    private void Move(float h)
    {
        // ����Ƿ��ڵ���
        CheckGround();
        // ���ƽ�ɫ��ת
        Flip(h);
        // ������Ծ
        Jump();
        // ˢ�½�ɫ�ٶ�
        UpdateVelocity(h);
        // �����ٶ������ƶ���ɫ
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
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

    // ������Ծ
    private void Jump()
    {
        jumpTime += Time.deltaTime;
        // ��������
        if (onGround && controller.startJump)
        {
            onGround = false;
            jumpTime = 0;
            // �ڵ����ϲ�����Ծ
            velocity.y = jumpForce;
            // ���¶���
            anim.SetBool("IsJump", true);
        }
        if (velocity.y > 0)
        {
            // ���������г���������Ծ��ʱ�Ӵ���Ծ�߶ȣ����ܳ������ʱ�����ƣ����ɿ�����ʱ���ϵ��ٶ���0
            velocity.y = controller.endJump || jumpTime > jumpholdmaxtime ? 0 : jumpForce;
        }
    }

    // ��������ˢ�½�ɫ�ٶ�
    private void UpdateVelocity(float h)
    {
        if (onGround)
        {
            // �ڵ��棬y���ٶ���Ϊ0
            velocity.y = 0;
            anim.SetBool("IsJump", false);
        }
        else
        {
            // �ڿ��У�ģ������Ӱ��
            velocity.y -= gravity * Time.deltaTime;
        }
        // ��������
        velocity.x = h * moveSpeed;
        // ���¶���
        anim.SetBool("IsGround", onGround);
        anim.SetFloat("Speed", Mathf.Abs(controller.h));
    }

    // ���������
    private void Attack()
    {
        if (controller.normalSlash)
        {
            anim.SetTrigger("Slash");
        }
    }
}
