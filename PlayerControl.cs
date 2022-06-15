using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    //임시
    [SerializeField] GameObject _wnd;
    public int _maxHP = 50;
    public int _currentHP = 0;

    [SerializeField] Transform _damageHUDPos;
    [SerializeField] GameObject _prefDamageHUD;

    PublicDefine.eActionState _currentActState;
    PublicDefine.eNPCType _currentNPC;
    //사망
    bool _isDead = false;
    [SerializeField] GameObject _preResultWindow;

    ResultWindow _resutWindow;


    public float _moveSpeed;
    float _nowSpeed;
    //점프
    public float _jumpPow;
    bool _isJump = false;
    public float checkRadius;
    public int _jumpCount;
    int _playerLayer, _footHoldLayer;

    //임시
    int step = 0;
    int _creatureLayer;

    //롤
    bool _isGround = false;
    bool _isRun = false;
    bool _isRoll = false;
    public float _rollPow;
    //공격
    [SerializeField] Transform _frontPos;
    [SerializeField] Transform _backPos;
    [SerializeField] Transform[] _attackBoxRightPos;
    [SerializeField] Transform[] _attackBoxLeftPos;
    [SerializeField] Vector2[] _attackBoxSize;
    [SerializeField] Transform _jumpAttackBoxRightPos;
    [SerializeField] Transform _jumpAttackBoxLeftPos;
    [SerializeField] Vector2 _jumpAttackBoxSize;
    int _comboStep = 0;
    bool _comboPossible = false;
    bool _isAttack = false;
    bool _jumpAttackPossible = false;
    int _damage = 2;
    //상호작용
    bool _isStageTrigger = false;
    bool _isNPCTrigger = false;


    VirtualStickControl _vitualStick;
    Rigidbody2D _rigBody;
    CapsuleCollider2D _collider2D;
    Animator _aniCtrl;
    SpriteRenderer _back;

    static PlayerControl _uniqueInstance;
    public static PlayerControl _instance
    {
        get { return _uniqueInstance; }
    }
    void Awake()
    {
        _uniqueInstance = this;
        _vitualStick = GameObject.Find("VirtualStickBG").GetComponent<VirtualStickControl>();
        _back = transform.GetComponent<SpriteRenderer>();
        _aniCtrl =transform.GetComponent<Animator>();
        _rigBody = transform.GetComponent<Rigidbody2D>();
        _collider2D = transform.GetComponent<CapsuleCollider2D>();
        _currentHP = _maxHP;
        _nowSpeed = _moveSpeed;
        _playerLayer = LayerMask.NameToLayer("Player");
        _footHoldLayer = LayerMask.NameToLayer("FootHold");
        _creatureLayer = LayerMask.NameToLayer("Creature");
    }
    void Start()
    {
        PlatformEffector2D playerEffector = GetComponent<PlatformEffector2D>();
        playerEffector.useColliderMask = false;
        Physics2D.IgnoreLayerCollision(_playerLayer, _creatureLayer, true);
    }

    void Update()
    {
        if (_isDead)
            return;

#if UNITY_EDITOR
        float mx;
        float my;
        if (_vitualStick._horizontalValue != 0)
        {
            mx = _vitualStick._horizontalValue;
        }
        else
            mx = Input.GetAxis("Horizontal");

        if (_vitualStick._verticalValue != 0)
        {
            my = _vitualStick._verticalValue;
        }
        else
            my = Input.GetAxis("Vertical");

        if (_isJump)
            _nowSpeed = _moveSpeed * 0.8f;
        else
            _nowSpeed = _moveSpeed;

        if (mx < 0) 
            _back.flipX = true;
        else if (mx > 0)
            _back.flipX = false;
        if (mx != 0)
        {
            _isRun = true;
            if (!_isAttack)
            {
                ChangeAniToAction(PublicDefine.eActionState.RUN);
                if (mx < 0)
                    transform.position += new Vector3(-_nowSpeed, 0, 0) * Time.deltaTime;
                else
                    transform.position += new Vector3(_nowSpeed, 0, 0) * Time.deltaTime;
            }
        }
        else
            ChangeAniToAction(PublicDefine.eActionState.IDLE);

        if (!_isJump)
        {
            if (my < -0.25f)
                _isRoll = true;
            else
                _isRoll = false;
            UIButtons._instance.ChangeJumpText(_isRoll);
        }

        // 점프 & 롤
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_isAttack)
                return;
            if (!_isRoll)
            {
                if (step == 0 || step == 2)
                {
                    _rigBody.velocity = Vector2.up * _jumpPow;
                    ChangeAniToAction(PublicDefine.eActionState.JUMP);
                    step++;
                }

            }
            if (_isRoll && _isGround)
            {
                ChangeAniToAction(PublicDefine.eActionState.ROLL);
                if (_back.flipX == true)
                    _rigBody.velocity = -Vector2.right * _rollPow;
                if (_back.flipX == false)
                    _rigBody.velocity = Vector2.right * _rollPow;
            }

        }

        if(_rigBody.velocity.y > 0)
            Physics2D.IgnoreLayerCollision(_playerLayer, _footHoldLayer, true);
        else
            Physics2D.IgnoreLayerCollision(_playerLayer, _footHoldLayer, false);


        switch (step)
        {
            case 0: // 바닥상태 (점프가능)
                _isGround = true;
                if (_rigBody.velocity.y < 0) step = 2;
                break;
            case 1: // 1단점프 후 올라가고 있는 상태 (점프 불가능)
                _isGround = false;
                if (_rigBody.velocity.y < 10f) step = 2;
                break;
            case 2: // 1단 점프 후  떨어지고 있는 상태(step 1 ->2) & 착지상태에서 떨어지고 있을 때 (step 0 ->2) (점프가능)
            case 3: // 2단 점프 후 올라가고 있는 상태 (점프 불가능)
                _isGround = false;
                _aniCtrl.SetBool("IsJump", true);
                _jumpAttackPossible = true;
                RaycastHit2D rayHit = Physics2D.Raycast(_rigBody.position, Vector2.down, 1, LayerMask.GetMask("Ground"));
                RaycastHit2D rayHit1 = Physics2D.Raycast(_rigBody.position, Vector2.down, 1, LayerMask.GetMask("FootHold"));
                if (rayHit.collider != null || rayHit1.collider != null) // 걸리는게 있으면.
                    if (rayHit.distance < 0.5f)
                    {
                        step = 0;
                        _isJump = false;
                        _aniCtrl.SetBool("IsJump", false);
                    }
                break;
        }

        // 어택
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (_isRoll)
                return;

            if (_isJump && _jumpAttackPossible) // 점프어택
            {
                ChangeAniToAction(PublicDefine.eActionState.JUMPATTACK);
                _aniCtrl.Play("Player_JumpAtt");
            }

            if (!_isJump)
            {
                ChangeAniToAction(PublicDefine.eActionState.ATTACK);
                if (_comboStep == 0)
                {
                    _isAttack = true;
                    _aniCtrl.Play("Player_Att1");
                    MoveAttack();
                }
                else// 2~3번째 공격만 들어온다.
                {
                    if (_comboPossible) // 콤보 가능한 상태라면?
                    {
                        _isAttack = true;
                        Combo();
                        _comboPossible = false; // 콤보 불가능상태.
                    }
                }

            }

        }
        
        // 임시
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ChangeAniToAction(PublicDefine.eActionState.DEAD);
        }

        // 임시
        if (Input.GetKeyDown(KeyCode.B)) // 나중에 조이스틱 y축에따라 _isRoll 바꿔주면 가능할듯?
        {
            if (!_isRoll)
                _isRoll = true;
            else
                _isRoll = false;
        }

        // 상호작용 키
        if (Input.GetKeyDown(KeyCode.T))
        {
            if(_isStageTrigger == true)
            {
                IngameManager._instance.NextStage();
            }
            if(_isNPCTrigger == true)
            {
                // 엔피씨 종류를 확인해서 개별로 작용.
                NPCInteraction(_currentNPC);
            }
        }

#else
        float mx = _vitualStick._horizontalValue;
        float my = _vitualStick._verticalValue;
        //mx = Input.GetAxis("Horizontal");
        if (mx < 0) 
            _back.flipX = true;
        else if (mx > 0)
            _back.flipX = false;
        if (mx != 0)
        {
            _isRun = true;
            if (!_isAttack)
            {
                ChangeAniToAction(PublicDefine.eActionState.RUN);
                if (mx < 0)
                    transform.position += new Vector3(-_moveSpeed, 0, 0) * Time.deltaTime;
                else
                    transform.position += new Vector3(_moveSpeed, 0, 0) * Time.deltaTime;
            }
        }
        else
            ChangeAniToAction(PublicDefine.eActionState.IDLE);

        if (!_isJump)
        {
            if (my < -0.25f)
                _isRoll = true;
            else
                _isRoll = false;
            UIButtons._instance.ChangeJumpText(_isRoll);
        }

        if(_rigBody.velocity.y > 0)
            Physics2D.IgnoreLayerCollision(_playerLayer, _footHoldLayer, true);
        else
            Physics2D.IgnoreLayerCollision(_playerLayer, _footHoldLayer, false);


        switch (step)
        {
            case 0:
                _isGround = true;
                if (_rigBody.velocity.y < 0) step = 2;
                break;
            case 1:
                _isGround = false;
                if (_rigBody.velocity.y < 10f) step = 2;
                break;
            case 2:
            case 3:
                _isGround = false;
                _aniCtrl.SetBool("IsJump", true);
                _jumpAttackPossible = true;
                RaycastHit2D rayHit = Physics2D.Raycast(_rigBody.position, Vector2.down, 1, LayerMask.GetMask("Ground"));
                RaycastHit2D rayHit1 = Physics2D.Raycast(_rigBody.position, Vector2.down, 1, LayerMask.GetMask("FootHold"));
                if (rayHit.collider != null || rayHit1.collider != null)
                    if (rayHit.distance < 0.5f)
                    {
                        step = 0;
                        _isJump = false;
                        _aniCtrl.SetBool("IsJump", false);
                    }
                break;
        }
#endif

    }
    #region 모바일 인게임 UI버튼
    public void Jump()
    {
        if (_isDead)
            return;
        if (_isAttack)
            return;
        if (!_isRoll)
        {
            if (step == 0 || step == 2)
            {
                _rigBody.velocity = Vector2.up * _jumpPow;
                ChangeAniToAction(PublicDefine.eActionState.JUMP);
                step++;
            }
        }
        if (_isRoll && _isGround)
        {
            ChangeAniToAction(PublicDefine.eActionState.ROLL);
            if (_back.flipX == true)
                _rigBody.velocity = -Vector2.right * _rollPow;
            if (_back.flipX == false)
                _rigBody.velocity = Vector2.right * _rollPow;
        }
    }
    public void Attack()
    {
        if (_isDead)
            return;
        if (_isRoll)
            return;

        if (_isJump && _jumpAttackPossible) // 점프어택
        {
            ChangeAniToAction(PublicDefine.eActionState.JUMPATTACK);
            _aniCtrl.Play("Player_JumpAtt");
        }
        if (!_isJump)
        {
            ChangeAniToAction(PublicDefine.eActionState.ATTACK);
            if (_comboStep == 0)
            {
                _isAttack = true;
                _aniCtrl.Play("Player_Att1");
                MoveAttack();
            }
            else // 2~3번째 공격만 들어온다.
            {
                if (_comboPossible) // 콤보 가능한 상태라면?
                {
                    _isAttack = true;
                    Combo();
                    _comboPossible = false; // 콤보 불가능상태.
                }
            }
        }
    }
    #endregion
    #region 점프 & 롤
    public void JumpAttackImpossible()
    {
        _jumpAttackPossible = false;
    }

    public void EndRoll()
    {
        _isRoll = false;
        _isJump = false;
        _aniCtrl.SetBool("IsRoll", false);
    }
    #endregion
    #region 공격
    public void ComboPossible()
    {
        _comboPossible = true;
        _comboStep++;
    }

    // 콤보어택
    void Combo()
    {
        if (_comboStep == 1)
        {
            _damage = 3;
            _aniCtrl.Play("Player_Att2");
            MoveAttack();
        }
        if (_comboStep == 2)
        {
            _damage = 5;
            _aniCtrl.Play("Player_Att3");
            MoveAttack();
        }
    }

    public void ComboReset()
    {
        _damage = 2;
        _comboPossible = false;
        _comboStep = 0;
        _isAttack = false;
    }

    void MoveAttack()
    {
        if (_isRun == true)
        {
            if (_back.flipX == true)
                transform.position = Vector2.MoveTowards(transform.position, _backPos.position, 0.5f);
            if (_back.flipX == false)
                transform.position = Vector2.MoveTowards(transform.position, _frontPos.position, 0.5f);
        }
    }

    void JumpAttackEvent()
    {
        if (_back.flipX)
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_jumpAttackBoxLeftPos.position, _jumpAttackBoxSize, 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Creature_Skeleton")
                    collider.transform.GetComponent<Skeleton>().TakeDamage(Damage());
                if (collider.tag == "Creature_FlyingEye")
                    collider.transform.GetComponent<FlyingEye>().TakeDamage(Damage());
            }
            return;
        }
        else
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_jumpAttackBoxRightPos.position, _jumpAttackBoxSize, 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Creature_Skeleton")
                    collider.transform.GetComponent<Skeleton>().TakeDamage(Damage());
                if (collider.tag == "Creature_FlyingEye")
                    collider.transform.GetComponent<FlyingEye>().TakeDamage(Damage());
            }
            return;
        }
    }
    void AttackEvent()
    {
        if (_back.flipX)
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_attackBoxLeftPos[_comboStep].position, _attackBoxSize[_comboStep], 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Creature_Skeleton")
                    collider.transform.GetComponent<Skeleton>().TakeDamage(Damage());
                if (collider.tag == "Creature_FlyingEye")
                    collider.transform.GetComponent<FlyingEye>().TakeDamage(Damage());
            }
        }
        else
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_attackBoxRightPos[_comboStep].position, _attackBoxSize[_comboStep], 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Creature_Skeleton")
                    collider.transform.GetComponent<Skeleton>().TakeDamage(Damage());
                if (collider.tag == "Creature_FlyingEye")
                    collider.transform.GetComponent<FlyingEye>().TakeDamage(Damage());
            }
        }
    }

    #endregion

    public void TakeDamage(int damage, Vector2 CreturePos)
    {
        if (_isDead)
            return;

        _currentHP -= damage;

        GameObject dmgHUD = Instantiate(_prefDamageHUD, _damageHUDPos.position, Quaternion.identity);
        dmgHUD.GetComponent<DamageHUD>()._damage = damage;
        if(transform.position.x - CreturePos.x < 0)
            dmgHUD.GetComponent<DamageHUD>()._dirRight = true;
        else
            dmgHUD.GetComponent<DamageHUD>()._dirRight = false;

        if (_currentHP <= 0)
        {
            _isDead = true;
            _aniCtrl.SetBool("IsDead", true);
            if (_resutWindow == null)
            {
                string text = "DEFEAT";
                GameObject go = Instantiate(_preResultWindow, _preResultWindow.transform.position, Quaternion.identity);
                go.GetComponent<ResultWindow>().SetResultText(text, Color.gray);
                _resutWindow = go.GetComponent<ResultWindow>();
            }
            else
                _resutWindow.gameObject.SetActive(true);
        }
    }
    public void Interaction()
    {
        if (_isStageTrigger == true)
        {
            IngameManager._instance.NextStage();
            return;
        }
        if (_isNPCTrigger == true)
        {
            NPCInteraction(_currentNPC);
            return;
        }
    }


    void ChangeAniToAction(PublicDefine.eActionState state)
    {
        switch (state)
        {
            case PublicDefine.eActionState.IDLE:
                _aniCtrl.SetBool("IsRun", false);
                _isRun=false;
                _currentActState = state;
                break;
            case PublicDefine.eActionState.RUN:
                _aniCtrl.SetBool("IsRun", true);
                _currentActState = state;
                break;
            case PublicDefine.eActionState.JUMP:
                _aniCtrl.SetBool("IsJump", true);
                _isJump = true;
                _currentActState = state;
                break;
            case PublicDefine.eActionState.ROLL:
                _aniCtrl.SetBool("IsRoll", true);
                _currentActState = state;
                break;
            case PublicDefine.eActionState.ATTACK:
                _isAttack = true;
                _currentActState = state;
                break;
            case PublicDefine.eActionState.JUMPATTACK:
                _currentActState = state;
                break;
            case PublicDefine.eActionState.DEAD:
                _isDead = true;
                _aniCtrl.SetBool("IsDead", true);
                _currentActState = state;
                break;

        }

    }

    void NPCInteraction(PublicDefine.eNPCType currentNPC)
    {
        switch (currentNPC)
        {
            case PublicDefine.eNPCType.Awakening:
                break;
            case PublicDefine.eNPCType.Item:
                break;
            case PublicDefine.eNPCType.Job:
                break;
            default:
                break;
        }
    }


    public int Damage()
    {
        return _damage;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Gate")
        {
            _isStageTrigger = true;
            _isNPCTrigger = false;
            UIButtons._instance.ChangeInteractionBT();
        }

        if (other.tag == "NPC")
        {
            _isStageTrigger = false;
            _isNPCTrigger = true;
            UIButtons._instance.ChangeInteractionBT();
            if (other.name.Contains("NPC_Awakening"))
            {
                _currentNPC = PublicDefine.eNPCType.Awakening;
                return;
            }
            if (other.name.Contains("NPC_Item"))
            {
                _currentNPC = PublicDefine.eNPCType.Item;
                return;
            }
            if (other.name.Contains("NPC_Job"))
            {
                _currentNPC = PublicDefine.eNPCType.Job;
                return;
            }
        }
        
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 상호작용 버튼을 공격 버튼으로.
        if (other.tag == "Gate")
        {
            _isStageTrigger = false;
            UIButtons._instance.ChangeAttackBT();
        }
        if (other.tag == "NPC")
        {
            _currentNPC = PublicDefine.eNPCType.None;
            UIButtons._instance.ChangeAttackBT();
        }
    }
}
