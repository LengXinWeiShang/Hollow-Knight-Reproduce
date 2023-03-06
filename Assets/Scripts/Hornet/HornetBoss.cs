using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HornetState
{
    Decide,             // ����״̬����������ҵ����λ�õ���Ϣ�����ж�
    Anticipation,       // ����ǰҡ
    Recovery,           // ������ҡ
    Run,                // �ƶ�
    Jump,               // ��Ծ�����⣬��Ծʱ��ʱ��������ҵľ��룬�����Ƿ���У�
    Evade,              // ����������Զ�����
    GDash,              // ���泯���ͻ�̣��������
    ADash,              // ���г����ͻ�̣���ʼͻ��ʱ�������λ��
    Throw,              // �ӳ���
    SphereAttack,       // ˿�߹���������Ϳ���һ�������ǲ��ƶ���
}

public class HornetBoss : MonoBehaviour
{
    public int maxHp = 50;
    public float checkGroundLength = 0.1f;      // �ײ����߼�ⳤ��
    public Material hitMaterial;                // �ܻ�ʱ�Ĳ��ʣ�ȫ�ף�
    public float runSpeed = 16f;                // �ƶ��ٶ�
    public float evadeSpeed = 32f;              // �����ٶ�
    public float[] jumpYSpeed = { 40f, 50f };   // ��Ծ�����ٶ�
    public float jumpXSpeed = 20f;              // ��Ծ�����ٶ�
    public float dashSpeed = 35f;               // ����Ϳ���ͻ���ٶ�
    public int decideCoolingTime = 20;          // AI������ȴʱ�䣨֡����
    public Transform leftWall;                  // BOSS����߽�
    public Transform rightWall;                 // BOSS����߽�
    public Needle prefabNeedle;                 // ��Ԥ����
    public SphereThread prefabSphere;           // ˿����Ԥ����
    public ActionEffect prefabADashEffect;      // ����ͻ����Ч
    public ActionEffect prefabGDashEffect;      // ����ͻ����Ч
    public ActionEffect prefabThrowEffect;      // ������Ч
    public ActionEffect prefabFlashEffect;      // ������Ч
    public ActionEffect prefabSplatEffect;      // �ܻ�Һ��ɽ���Ч
    public AudioClip startClip;                 // ��������
    public List<AudioClip> dashOrNeedleClips;   // ͻ�̺ͷ�������
    public List<AudioClip> threadAttackClips;   // ˿�߹�������
    public List<AudioClip> jumpClips;           // ��Ծ����
    public List<AudioClip> evadeClips;          // ��������
    public List<AudioClip> stunClips;           // Ӳֱ����
    public AudioClip deathClip;                 // ս������

    private PlayerCharacter2D player;
    private Animator anim;
    private Rigidbody2D rigid;
    private AudioSource audioSource;
    private int hp;
    private bool faceLeft;
    private HornetState state;
    private float accelerationY = 87.5f;    // ��Ծʱ������ٶ�
    private int curDecideCoolingTime = 0;   // ��ǰ�ľ�����ȴʱ��

    // ��ɫ�ײ���ײ������ߵ����
    private Vector3 BottonRaycastStartPos
    { get { return transform.position + new Vector3(0, -1.8f, 0); } }

    private Vector3 playerPos;              // ��ҵ�λ�ã�����ʱ����
    private float playerDistance;           // ����ҵľ��룬����BOSS�ж���AI����
    private bool isPlayerLeft;              // ����Ƿ��ڴ�Ʒ�����
    private static float leftWallPosX;      // ������߽�ĺ�����
    private static float rightWallPosX;     // �����ұ߽�ĺ�����
    private float leftWallDistance;         // �뷿����߽�ľ��룬����BOSS�ж���AI����
    private float rightWallDistance;        // �뷿���ұ߽�ľ��룬����BOSS�ж���AI����
    private float moveTargetX;              // �ڵ������ƶ���Ŀ�꣨�����꣩
    private float curJumpYSpeed;            // ��������������ٶ�
    private float curJumpXSpeed;            // ����ĺ��������ٶ�
    private bool jumpChange;                // ��ǰ��Ծ�Ƿ�����
    private Needle needle;                  // Ͷ����ȥ����
    private SpriteRenderer sprite;
    private Material defaultMaterial;       // Ĭ�ϲ���

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.Find("Player").GetComponent<PlayerCharacter2D>();
        hp = maxHp;
        faceLeft = true;
        leftWallPosX = leftWall.position.x;
        rightWallPosX = rightWall.position.x;
        playerDistance = (transform.position - player.transform.position).magnitude;
        isPlayerLeft = true;
        leftWallDistance = transform.position.x - leftWallPosX;
        rightWallDistance = rightWallPosX - transform.position.x;
        sprite = GetComponent<SpriteRenderer>();
        defaultMaterial = sprite.material;
    }

    private void OnDrawGizmos()
    {
        // �ײ�����
        Debug.DrawRay(BottonRaycastStartPos, Vector2.down * checkGroundLength, Color.red);
    }

    private void Start()
    {
        audioSource.clip = startClip;
        audioSource.Play();
    }

    private void FixedUpdate()
    {
        // ������Ҫ�ƶ���״̬
        switch (state)
        {
            case HornetState.Run:
                Run();
                break;

            case HornetState.Evade:
                Evade();
                break;

            case HornetState.Jump:
                Jump();
                break;

            case HornetState.GDash:
                GDash();
                break;

            case HornetState.ADash:
                ADash();
                break;
        }
    }

    private void Run()
    {
        Vector3 newPos = Vector3.MoveTowards(transform.position, new Vector3(moveTargetX, transform.position.y, transform.position.z), runSpeed * Time.fixedDeltaTime);
        transform.position = newPos;
    }

    private void Evade()
    {
        Vector3 newPos = Vector3.MoveTowards(transform.position, new Vector3(moveTargetX, transform.position.y, transform.position.z), evadeSpeed * Time.fixedDeltaTime);
        transform.position = newPos;
    }

    private void Jump()
    {
        Vector3 move = new Vector3((faceLeft ? -1 : 1) * curJumpXSpeed * Time.fixedDeltaTime, curJumpYSpeed * Time.fixedDeltaTime, 0);
        transform.position += move;
        curJumpYSpeed -= accelerationY * Time.fixedDeltaTime;
    }

    private void GDash()
    {
        Vector3 newPos = Vector3.MoveTowards(transform.position, new Vector3(moveTargetX, transform.position.y, transform.position.z), dashSpeed * Time.fixedDeltaTime);
        transform.position = newPos;
    }

    private void ADash()
    {
        transform.position += (faceLeft ? -1 : 1) * (transform.right * dashSpeed * Time.fixedDeltaTime);
    }

    public void GetHit(int damage)
    {
        hp = Mathf.Clamp(hp - damage, 0, maxHp);
        // ��Ч
        Instantiate(prefabSplatEffect, transform.position, Quaternion.identity);
        // �ܻ�����
        sprite.material = hitMaterial;
        // һ��ʱ����л���Ĭ�ϲ���
        StartCoroutine(ResetMaterial(0.1f));

        if (hp == 0)
        {
            anim.SetTrigger("Death");
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (var c in colliders)
            {
                c.enabled = false;
            }
            rigid.isKinematic = true;
            audioSource.clip = deathClip;
            audioSource.Play();
            GameManager.Instance.QuitFight();
        }
    }

    // �жϽ�ɫ�Ƿ��ڵ�����
    private bool CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(BottonRaycastStartPos, Vector2.down, checkGroundLength, LayerMask.GetMask("Ground"));
        return hit ? true : false;
    }

    private IEnumerator ResetMaterial(float delay)
    {
        yield return new WaitForSeconds(delay);
        sprite.material = defaultMaterial;
    }

    private void Flip(float targetX)
    {
        if (targetX > transform.position.x)
        {
            faceLeft = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            faceLeft = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    #region ����״̬��

    private delegate void FuncStateEnter(int n);

    private delegate void FuncStateUpdate(int n);

    private delegate void FuncStateExit(int n);

    private Dictionary<HornetState, FuncStateEnter> dictStateEnter;
    private Dictionary<HornetState, FuncStateUpdate> dictStateUpdate;
    private Dictionary<HornetState, FuncStateExit> dictStateExit;

    // ���붯��״̬ʱִ�еķ���
    public void OnAnimStateEnter(HornetState s, int n)
    {
        if (dictStateEnter == null)
        {
            // ����ʹ��ʱ��ʼ���ֵ䣬���������ֵ�ͬ��
            dictStateEnter = new Dictionary<HornetState, FuncStateEnter>
            {
                {HornetState.Decide, DecideEnter},
                {HornetState.Anticipation, AnticipationEnter},
                {HornetState.Recovery, RecoveryEnter},
                {HornetState.Run, RunEnter},
                {HornetState.Jump, JumpEnter},
                {HornetState.Evade, EvadeEnter},
                {HornetState.GDash, GDashEnter},
                {HornetState.ADash, ADashEnter},
                {HornetState.Throw, ThrowEnter},
                {HornetState.SphereAttack, SphereAttackEnter},
            };
        }

        // ���ö�Ӧ״̬�ķ���
        if (dictStateEnter.ContainsKey(s) && dictStateEnter[s] != null)
        {
            dictStateEnter[s](n);
        }
    }

    // ����״̬������ִ�еķ���
    public void OnAnimStateUpdate(HornetState s, int n)
    {
        if (dictStateUpdate == null)
        {
            dictStateUpdate = new Dictionary<HornetState, FuncStateUpdate>
            {
                {HornetState.Decide, DecideUpdate},
                {HornetState.Anticipation, AnticipationUpdate},
                {HornetState.Recovery, RecoveryUpdate},
                {HornetState.Run, RunUpdate},
                {HornetState.Jump, JumpUpdate},
                {HornetState.Evade, EvadeUpdate},
                {HornetState.GDash, GDashUpdate},
                {HornetState.ADash, ADashUpdate},
                {HornetState.Throw, ThrowUpdate},
                {HornetState.SphereAttack, SphereAttackUpdate},
            };
        }

        if (dictStateUpdate.ContainsKey(s) && dictStateUpdate[s] != null)
        {
            dictStateUpdate[s](n);
        }
    }

    // ����״̬�˳�ʱִ�еķ���
    public void OnAnimStateExit(HornetState s, int n)
    {
        if (dictStateExit == null)
        {
            dictStateExit = new Dictionary<HornetState, FuncStateExit>
            {
                {HornetState.Decide, DecideExit},
                {HornetState.Anticipation, AnticipationExit},
                {HornetState.Recovery, RecoveryExit},
                {HornetState.Run, RunExit},
                {HornetState.Jump, JumpExit},
                {HornetState.Evade, EvadeExit},
                {HornetState.GDash, GDashExit},
                {HornetState.ADash, ADashExit},
                {HornetState.Throw, ThrowExit},
                {HornetState.SphereAttack, SphereAttackExit},
            };
        }

        if (dictStateExit.ContainsKey(s) && dictStateExit[s] != null)
        {
            dictStateExit[s](n);
        }
    }

    // ---------- Enter ----------

    public void DecideEnter(int n)
    {
        state = HornetState.Decide;
        jumpChange = false;
        // ��ȡ��Һͷ���߽������Ϣ������AI�ж�����
        playerPos = player.transform.position;
        playerDistance = (transform.position - playerPos).magnitude;
        isPlayerLeft = transform.position.x - playerPos.x > 0;
        leftWallDistance = transform.position.x - leftWallPosX;
        rightWallDistance = rightWallPosX - transform.position.x;
        // �������
        Flip(playerPos.x);
    }

    public void AnticipationEnter(int n)
    {
        state = HornetState.Anticipation;
        switch (n)
        {
            case 0:
                // ��Ծ
                {
                    curJumpYSpeed = jumpYSpeed[Random.Range(0, 2)];
                    curJumpXSpeed = jumpXSpeed;
                    audioSource.clip = jumpClips[Random.Range(0, 3)];
                }
                break;

            case 1:
                // ����
                {
                    audioSource.clip = evadeClips[Random.Range(0, 2)];
                }
                break;

            case 2:
                // ����ͻ��
                {
                    audioSource.clip = dashOrNeedleClips[Random.Range(0, 2)];
                }
                break;

            case 3:
                // ����ͻ��
                {
                    // �������
                    Flip(playerPos.x);
                    audioSource.clip = dashOrNeedleClips[Random.Range(0, 2)];
                }
                break;

            case 4:
                // ����
                {
                    audioSource.clip = dashOrNeedleClips[Random.Range(0, 2)];
                }
                break;

            case 5:
                // ˿�߹���
                {
                    audioSource.clip = threadAttackClips[Random.Range(0, 2)];
                }
                break;
        }
    }

    public void RecoveryEnter(int n)
    {
        state = HornetState.Recovery;
    }

    public void RunEnter(int n)
    {
        state = HornetState.Run;
        // ����Ŀ��㷭ת����ͼ
        Flip(moveTargetX);
    }

    public void JumpEnter(int n)
    {
        state = HornetState.Jump;
        if (n == 0)
        {
            audioSource.Play();
        }
        if (!jumpChange && n == 1 && playerDistance > 8f)
        {
            jumpChange = true;
            // ������ҽ�Զʱһ�������ͷ�ͻ��
            if (Random.Range(0f, 1f) < 1f)
            {
                anim.SetTrigger("ADash");
            }
        }
    }

    public void EvadeEnter(int n)
    {
        state = HornetState.Evade;
        if (n == 0)
        {
            audioSource.Play();
        }
    }

    public void GDashEnter(int n)
    {
        state = HornetState.GDash;
        if (n == 0)
        {
            audioSource.Play();
            ActionEffect ef = Instantiate(prefabGDashEffect, transform.position, Quaternion.identity);
            if (!faceLeft)
            {
                ef.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    public void ADashEnter(int n)
    {
        state = HornetState.ADash;
        if (n == 0)
        {
            audioSource.Play();
            ActionEffect ef = Instantiate(prefabADashEffect, transform.position, Quaternion.identity);
            if (!faceLeft)
            {
                ef.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        // ָ������ڵ����ϵ�ͶӰλ��
        Vector3 v = new Vector3(playerPos.x, -5.5f, playerPos.z) - transform.position;
        v.z = 0;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, faceLeft ? v * -1 : v);
        transform.rotation = rotation;
    }

    public void ThrowEnter(int n)
    {
        state = HornetState.Throw;
        if (n == 0)
        {
            audioSource.Play();
            ActionEffect ef = Instantiate(prefabThrowEffect, transform.position, Quaternion.identity);
            if (!faceLeft)
            {
                ef.transform.localScale = new Vector3(-1, 1, 1);
            }
            // ���������
            needle = Instantiate(prefabNeedle, new Vector3(transform.position.x, transform.position.y - 0.4f, transform.position.z), Quaternion.identity);
            if (!faceLeft)
            {
                // �������ҷ��еķ���
                needle.transform.right = Vector3.left;
                needle.moveLeft = false;
            }
        }
    }

    public void SphereAttackEnter(int n)
    {
        state = HornetState.SphereAttack;
        if (n == 0)
        {
            audioSource.Play();
            ActionEffect ef = Instantiate(prefabFlashEffect, transform.position, Quaternion.identity);
            if (!faceLeft)
            {
                ef.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        // �ٶ����㣨�����Ǵӿ��н���ģ�
        curJumpXSpeed = 0;
        curJumpYSpeed = 0;
        // ����˿�߶���
        SphereThread sphere = Instantiate(prefabSphere, transform.position, Quaternion.identity);
    }

    // ---------- Update ----------

    public void DecideUpdate(int n)
    {
        if (curDecideCoolingTime < decideCoolingTime)
        {
            curDecideCoolingTime++;
            return;
        }
        Debug.Log("�������ࣺ" + isPlayerLeft + "�����룺" + playerDistance);
        // �����ж�
        if (playerDistance < 8)
        {
            if (Random.Range(0f, 1f) < 0.2f)
            {
                // ������ҽϽ�ʱ30%�����ͷ�˿�߹���
                anim.SetTrigger("SphereAttack");
                return;
            }
            if ((isPlayerLeft && rightWallDistance < 8) || (!isPlayerLeft && leftWallDistance < 8))
            {
                // ����̫����ǽ��������ͻ��
                int rand = Random.Range(0, 2);
                if (rand == 0)
                {
                    // ��Ծ
                    anim.SetTrigger("Jump");
                }
                else
                {
                    // ���õ���ͻ��Ŀ���(��ҵ�ǰλ����ǰ��һ��)
                    moveTargetX = Mathf.Clamp(isPlayerLeft ? playerPos.x - 3f : playerPos.x + 3f, leftWallPosX, rightWallPosX);
                    // ����ͻ��
                    anim.SetTrigger("GDash");
                }
            }
            else
            {
                // �������ǽ��һ�ξ��룬תͷ�ƶ������
                moveTargetX = isPlayerLeft ? Mathf.Clamp(playerPos.x + 12f, leftWallPosX + 1f, rightWallPosX - 1f) : Mathf.Clamp(playerPos.x - 12f, leftWallPosX + 1f, rightWallPosX - 1f);
                int rand = Random.Range(0, 2);
                if (rand == 0)
                {
                    anim.SetTrigger("Run");
                }
                else
                {
                    anim.SetTrigger("Evade");
                }
            }
        }
        else
        {
            // ������ҽ�Զ��ͻ�̻���Ͷ������Ծ����
            int rand = Random.Range(0, 3);
            if (rand == 0)
            {
                anim.SetTrigger("Throw");
            }
            else if (rand == 1)
            {
                // ���õ���ͻ��Ŀ���(��ҵ�ǰλ����ǰ��һ��)
                moveTargetX = Mathf.Clamp(isPlayerLeft ? playerPos.x - 3f : playerPos.x + 3f, leftWallPosX, rightWallPosX);
                anim.SetTrigger("GDash");
            }
            else
            {
                anim.SetTrigger("Jump");
            }
        }
        curDecideCoolingTime = 0;
    }

    public void AnticipationUpdate(int n)
    {
    }

    public void RecoveryUpdate(int n)
    {
    }

    public void RunUpdate(int n)
    {
        // �ƶ���Ŀ�����˳�
        if (Mathf.Abs(moveTargetX - transform.position.x) < 0.1f)
        {
            anim.SetTrigger("EndRun");
        }
    }

    public void JumpUpdate(int n)
    {
        if (CheckGround())
        {
            anim.SetTrigger("Land");
        }
        // ��ȡ���λ�������Ϣ������AI���߱���
        playerPos = player.transform.position;
        playerDistance = (transform.position - playerPos).magnitude;
        if (!jumpChange && playerDistance < 5f && Mathf.Abs(playerPos.x - transform.position.x) < 3)
        {
            jumpChange = true;
            // ������ҽϽ�ʱһ�������ͷ�˿�߹���
            if (Random.Range(0f, 1f) < 0.1f)
            {
                anim.SetTrigger("SphereAttack");
            }
        }
    }

    public void EvadeUpdate(int n)
    {
        // �ƶ���Ŀ�����˳�
        if (Mathf.Abs(moveTargetX - transform.position.x) < 0.1f)
        {
            anim.SetTrigger("EndEvade");
        }
    }

    public void GDashUpdate(int n)
    {
        // ͻ�̵�Ŀ�����˳�
        if (Mathf.Abs(moveTargetX - transform.position.x) < 0.1f)
        {
            anim.SetTrigger("EndGDash");
        }
    }

    public void ADashUpdate(int n)
    {
        // ͻ�̵��������ֹ
        if (transform.position.y < -4.5f)
        {
            anim.SetTrigger("EndADash");
        }
    }

    public void ThrowUpdate(int n)
    {
        // �ж����Ƿ����ջ�
        if (Mathf.Abs(needle.transform.position.x - transform.position.x) < 0.5f && needle.speed < 0)
        {
            needle.Destroy();
            needle = null;
            anim.SetTrigger("EndThrow");
        }
    }

    public void SphereAttackUpdate(int n)
    {
    }

    // ---------- Exit ----------

    public void DecideExit(int n)
    {
    }

    public void AnticipationExit(int n)
    {
    }

    public void RecoveryExit(int n)
    {
    }

    public void RunExit(int n)
    {
    }

    public void JumpExit(int n)
    {
        anim.ResetTrigger("Land");
    }

    public void EvadeExit(int n)
    {
    }

    public void GDashExit(int n)
    {
    }

    public void ADashExit(int n)
    {
        // ��ת�ظ�
        transform.rotation = Quaternion.identity;
    }

    public void ThrowExit(int n)
    {
    }

    public void SphereAttackExit(int n)
    {
    }

    #endregion ����״̬��
}