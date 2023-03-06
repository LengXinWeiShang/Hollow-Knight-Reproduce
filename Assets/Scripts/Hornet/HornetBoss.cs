using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HornetState
{
    Decide,             // 决策状态，根据与玩家的相对位置等信息决策行动
    Anticipation,       // 动作前摇
    Recovery,           // 动作后摇
    Run,                // 移动
    Jump,               // 跳跃（特殊，跳跃时随时更新与玩家的距离，决策是否变招）
    Evade,              // 后跳，快速远离玩家
    GDash,              // 地面朝玩家突刺，距离随机
    ADash,              // 空中朝玩家突刺，开始突刺时锁定玩家位置
    Throw,              // 扔出针
    SphereAttack,       // 丝线攻击（地面和空中一样，均是不移动）
}

public class HornetBoss : MonoBehaviour
{
    public int maxHp = 50;
    public float checkGroundLength = 0.1f;      // 底部射线检测长度
    public Material hitMaterial;                // 受击时的材质（全白）
    public float runSpeed = 16f;                // 移动速度
    public float evadeSpeed = 32f;              // 后跳速度
    public float[] jumpYSpeed = { 40f, 50f };   // 跳跃纵向速度
    public float jumpXSpeed = 20f;              // 跳跃横向速度
    public float dashSpeed = 35f;               // 地面和空中突刺速度
    public int decideCoolingTime = 20;          // AI决策冷却时间（帧数）
    public Transform leftWall;                  // BOSS房左边界
    public Transform rightWall;                 // BOSS房左边界
    public Needle prefabNeedle;                 // 针预制体
    public SphereThread prefabSphere;           // 丝线球预制体
    public ActionEffect prefabADashEffect;      // 空中突刺特效
    public ActionEffect prefabGDashEffect;      // 地面突刺特效
    public ActionEffect prefabThrowEffect;      // 飞针特效
    public ActionEffect prefabFlashEffect;      // 闪光特效
    public ActionEffect prefabSplatEffect;      // 受击液体飞溅特效
    public AudioClip startClip;                 // 开场语音
    public List<AudioClip> dashOrNeedleClips;   // 突刺和飞针语音
    public List<AudioClip> threadAttackClips;   // 丝线攻击语音
    public List<AudioClip> jumpClips;           // 跳跃语音
    public List<AudioClip> evadeClips;          // 闪避语音
    public List<AudioClip> stunClips;           // 硬直语音
    public AudioClip deathClip;                 // 战败语音

    private PlayerCharacter2D player;
    private Animator anim;
    private Rigidbody2D rigid;
    private AudioSource audioSource;
    private int hp;
    private bool faceLeft;
    private HornetState state;
    private float accelerationY = 87.5f;    // 跳跃时纵向加速度
    private int curDecideCoolingTime = 0;   // 当前的决策冷却时间

    // 角色底部碰撞检测射线的起点
    private Vector3 BottonRaycastStartPos
    { get { return transform.position + new Vector3(0, -1.8f, 0); } }

    private Vector3 playerPos;              // 玩家的位置，决策时更新
    private float playerDistance;           // 与玩家的距离，用于BOSS行动的AI决策
    private bool isPlayerLeft;              // 玩家是否在大黄蜂的左侧
    private static float leftWallPosX;      // 房间左边界的横坐标
    private static float rightWallPosX;     // 房间右边界的横坐标
    private float leftWallDistance;         // 与房间左边界的距离，用于BOSS行动的AI决策
    private float rightWallDistance;        // 与房间右边界的距离，用于BOSS行动的AI决策
    private float moveTargetX;              // 在地面上移动的目标（横坐标）
    private float curJumpYSpeed;            // 随机的纵向起跳速度
    private float curJumpXSpeed;            // 随机的横向起跳速度
    private bool jumpChange;                // 当前跳跃是否变过招
    private Needle needle;                  // 投掷出去的针
    private SpriteRenderer sprite;
    private Material defaultMaterial;       // 默认材质

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
        // 底部射线
        Debug.DrawRay(BottonRaycastStartPos, Vector2.down * checkGroundLength, Color.red);
    }

    private void Start()
    {
        audioSource.clip = startClip;
        audioSource.Play();
    }

    private void FixedUpdate()
    {
        // 处理需要移动的状态
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
        // 特效
        Instantiate(prefabSplatEffect, transform.position, Quaternion.identity);
        // 受击材质
        sprite.material = hitMaterial;
        // 一段时间后切换回默认材质
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

    // 判断角色是否在地面上
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

    #region 核心状态机

    private delegate void FuncStateEnter(int n);

    private delegate void FuncStateUpdate(int n);

    private delegate void FuncStateExit(int n);

    private Dictionary<HornetState, FuncStateEnter> dictStateEnter;
    private Dictionary<HornetState, FuncStateUpdate> dictStateUpdate;
    private Dictionary<HornetState, FuncStateExit> dictStateExit;

    // 进入动画状态时执行的方法
    public void OnAnimStateEnter(HornetState s, int n)
    {
        if (dictStateEnter == null)
        {
            // 初次使用时初始化字典，其余两个字典同理
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

        // 调用对应状态的方法
        if (dictStateEnter.ContainsKey(s) && dictStateEnter[s] != null)
        {
            dictStateEnter[s](n);
        }
    }

    // 动画状态过程中执行的方法
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

    // 动画状态退出时执行的方法
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
        // 获取玩家和房间边界相关信息，用于AI行动决策
        playerPos = player.transform.position;
        playerDistance = (transform.position - playerPos).magnitude;
        isPlayerLeft = transform.position.x - playerPos.x > 0;
        leftWallDistance = transform.position.x - leftWallPosX;
        rightWallDistance = rightWallPosX - transform.position.x;
        // 面向玩家
        Flip(playerPos.x);
    }

    public void AnticipationEnter(int n)
    {
        state = HornetState.Anticipation;
        switch (n)
        {
            case 0:
                // 跳跃
                {
                    curJumpYSpeed = jumpYSpeed[Random.Range(0, 2)];
                    curJumpXSpeed = jumpXSpeed;
                    audioSource.clip = jumpClips[Random.Range(0, 3)];
                }
                break;

            case 1:
                // 后跳
                {
                    audioSource.clip = evadeClips[Random.Range(0, 2)];
                }
                break;

            case 2:
                // 地面突刺
                {
                    audioSource.clip = dashOrNeedleClips[Random.Range(0, 2)];
                }
                break;

            case 3:
                // 空中突刺
                {
                    // 面向玩家
                    Flip(playerPos.x);
                    audioSource.clip = dashOrNeedleClips[Random.Range(0, 2)];
                }
                break;

            case 4:
                // 飞针
                {
                    audioSource.clip = dashOrNeedleClips[Random.Range(0, 2)];
                }
                break;

            case 5:
                // 丝线攻击
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
        // 根据目标点翻转精灵图
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
            // 距离玩家较远时一定几率释放突刺
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
        // 指向玩家在地面上的投影位置
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
            // 创建针对象
            needle = Instantiate(prefabNeedle, new Vector3(transform.position.x, transform.position.y - 0.4f, transform.position.z), Quaternion.identity);
            if (!faceLeft)
            {
                // 设置左右飞行的方向
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
        // 速度置零（可能是从空中进入的）
        curJumpXSpeed = 0;
        curJumpYSpeed = 0;
        // 创建丝线对象
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
        Debug.Log("玩家在左侧：" + isPlayerLeft + "，距离：" + playerDistance);
        // 决策行动
        if (playerDistance < 8)
        {
            if (Random.Range(0f, 1f) < 0.2f)
            {
                // 距离玩家较近时30%几率释放丝线攻击
                anim.SetTrigger("SphereAttack");
                return;
            }
            if ((isPlayerLeft && rightWallDistance < 8) || (!isPlayerLeft && leftWallDistance < 8))
            {
                // 背后太靠近墙，跳出或突刺
                int rand = Random.Range(0, 2);
                if (rand == 0)
                {
                    // 跳跃
                    anim.SetTrigger("Jump");
                }
                else
                {
                    // 设置地面突刺目标点(玩家当前位置再前进一点)
                    moveTargetX = Mathf.Clamp(isPlayerLeft ? playerPos.x - 3f : playerPos.x + 3f, leftWallPosX, rightWallPosX);
                    // 地面突刺
                    anim.SetTrigger("GDash");
                }
            }
            else
            {
                // 背后距离墙有一段距离，转头移动或后跳
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
            // 距离玩家较远，突刺或者投掷或跳跃靠近
            int rand = Random.Range(0, 3);
            if (rand == 0)
            {
                anim.SetTrigger("Throw");
            }
            else if (rand == 1)
            {
                // 设置地面突刺目标点(玩家当前位置再前进一点)
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
        // 移动至目标点后退出
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
        // 获取玩家位置相关信息，用于AI决策变招
        playerPos = player.transform.position;
        playerDistance = (transform.position - playerPos).magnitude;
        if (!jumpChange && playerDistance < 5f && Mathf.Abs(playerPos.x - transform.position.x) < 3)
        {
            jumpChange = true;
            // 距离玩家较近时一定几率释放丝线攻击
            if (Random.Range(0f, 1f) < 0.1f)
            {
                anim.SetTrigger("SphereAttack");
            }
        }
    }

    public void EvadeUpdate(int n)
    {
        // 移动至目标点后退出
        if (Mathf.Abs(moveTargetX - transform.position.x) < 0.1f)
        {
            anim.SetTrigger("EndEvade");
        }
    }

    public void GDashUpdate(int n)
    {
        // 突刺到目标点后退出
        if (Mathf.Abs(moveTargetX - transform.position.x) < 0.1f)
        {
            anim.SetTrigger("EndGDash");
        }
    }

    public void ADashUpdate(int n)
    {
        // 突刺到地面后终止
        if (transform.position.y < -4.5f)
        {
            anim.SetTrigger("EndADash");
        }
    }

    public void ThrowUpdate(int n)
    {
        // 判断针是否已收回
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
        // 旋转回复
        transform.rotation = Quaternion.identity;
    }

    public void ThrowExit(int n)
    {
    }

    public void SphereAttackExit(int n)
    {
    }

    #endregion 核心状态机
}