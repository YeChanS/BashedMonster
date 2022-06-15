using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    Rigidbody2D _rigd;
    public int _nextMove;
    Animator _aniCtrl;
    SpriteRenderer _back;
    CapsuleCollider2D _col2D;

    [SerializeField] Transform _attackBoxLeftPos;
    [SerializeField] Transform _attackBoxRightPos;
    [SerializeField] Vector2 _attackBoxSize;

    [SerializeField] Transform _damageHUDPos;
    [SerializeField] GameObject _prefDamageHUD;

    public bool _isBattle = false;
    bool _isAttack = false;
    bool _isInvoke = false;
    bool _isDead = false;
    bool _isDefense = false;
    int _phaseNum = 0;
    public int _maxHP = 20;
    public int _currentHP = 0;
    public int _damage = 0;

    static Skeleton _uniqueInstance;
    public static Skeleton _instance
    {
        get { return _uniqueInstance; }
    }
    void Awake()
    {
        _uniqueInstance = this;
        _rigd = GetComponent<Rigidbody2D>();
        _aniCtrl = GetComponent<Animator>();
        _back = GetComponent<SpriteRenderer>();
        _col2D = GetComponent<CapsuleCollider2D>();
        _currentHP = _maxHP;
        Invoke("RandomNextMove", 3);
    }
    void Start()
    {
        _phaseNum = Random.Range(0, 2); // 0이면 방어모드 1이면 공격모드.
    }

    void Update()
    {
        if (_isDead)
            return;

        if (Vector2.Distance(transform.position, PlayerControl._instance.transform.position) < 10f)
        {
            _isBattle = true;
            _aniCtrl.SetBool("IsRun", false);
            float x = transform.position.x - PlayerControl._instance.transform.position.x;
            if (x < 0)
                _back.flipX = false;
            else
                _back.flipX = true;
        }
        else
        {
            _isBattle = false;
            _isDefense = false;
            _isAttack = false;
            _aniCtrl.SetBool("IsDefense", false);
        }


        if (_isBattle)
        {
            BattlePhase();
        }
        else
        {
            if (!_isInvoke)
            {
                CancelInvoke();
                Invoke("RandomNextMove", 3);
                _isInvoke = true;
            }
            if (_nextMove == 0)
            {
                _aniCtrl.SetBool("IsRun", false);
                return;
            }
            else
            {
                if (_nextMove > 0)
                {
                    _back.flipX = false;
                    _aniCtrl.SetBool("IsRun", true);
                    transform.Translate(new Vector3(1 * 0.01f, 0, 0));
                }
                else
                {
                    _back.flipX = true;
                    _aniCtrl.SetBool("IsRun", true);
                    transform.Translate(new Vector3(-1 * 0.01f, 0, 0));
                }
            }
        
            Vector2 frontVec = new Vector2(_rigd.position.x + _nextMove * 0.5f, _rigd.position.y);
            Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector2.down, 1, LayerMask.GetMask("Ground"));
            RaycastHit2D rayHitS = Physics2D.Raycast(frontVec, Vector2.down, 1, LayerMask.GetMask("FootHold"));
            RaycastHit2D rayHit2 = Physics2D.Raycast(frontVec, Vector2.left, 1, LayerMask.GetMask("Ground"));
            RaycastHit2D rayHit3 = Physics2D.Raycast(frontVec, Vector2.right, 1, LayerMask.GetMask("Ground"));
            if(rayHit.collider == null && rayHitS .collider == null || rayHit2.collider != null || rayHit3.collider != null)
            {
                _nextMove *= -1;
                CancelInvoke();
                Invoke("RandomNextMove", 3);
            }
        }
        
    }

    void BattlePhase()
    {
        if (_isInvoke)
        {
            CancelInvoke();
            _isInvoke = false;
        }

        if (_phaseNum == 0) // 방어모드
        {
            _isDefense = true;
            _aniCtrl.SetBool("IsDefense", true);
            if (!_isAttack)
            {
                Invoke("Attack", 3);
                _isAttack = true;
            }

        }
        else // 공격모드.
        {
            if (!_isAttack)
            {
                Invoke("Attack", 2);
                _isAttack = true;
            }
        }
    }


    // 공격
    void Attack() 
    {
        if (_isDefense)
        {
            _damage = 5;
            if (Vector2.Distance(transform.position, PlayerControl._instance.transform.position) < 5f)
            {
                _aniCtrl.SetTrigger("DToA");
            }
            Invoke("Attack", 3);
        }
        else
        {
            _damage = 3;
            if (Vector2.Distance(transform.position, PlayerControl._instance.transform.position) < 5f)
            {
                _aniCtrl.SetTrigger("Attack");
            }
            Invoke("Attack", 2);
        }


        //방향에따른 OverlapBox 변경
        if (_back.flipX)
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_attackBoxLeftPos.position, _attackBoxSize, 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Player")
                    collider.transform.GetComponent<PlayerControl>().TakeDamage(_damage,transform.position);
            }
        }
        else
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_attackBoxRightPos.position, _attackBoxSize, 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Player")
                    collider.transform.GetComponent<PlayerControl>().TakeDamage(_damage, transform.position);
            }
        }
    }
    public void AttackColiderReset() 
    {
        _damage = 3;
    }

    public int AttackDamage()
    {
        return _damage; 
    }

    // 데미지연산
    public void TakeDamage(int damage)
    {
        if (_isDead)
            return;

        if (_currentHP > 0)
        {
            if (_isDefense)
            {
                _currentHP -= damage / 2;
                GameObject dmgHUD = Instantiate(_prefDamageHUD, _damageHUDPos.position, Quaternion.identity);
                dmgHUD.GetComponent<DamageHUD>()._damage = damage / 2;
                if (_back.flipX)
                    dmgHUD.GetComponent<DamageHUD>()._dirRight = false;
                else
                    dmgHUD.GetComponent<DamageHUD>()._dirRight = true;
            }
            else
            {
                _currentHP -= damage;
                _aniCtrl.SetTrigger("Hit");
                GameObject dmgHUD = Instantiate(_prefDamageHUD, _damageHUDPos.position, Quaternion.identity);
                dmgHUD.GetComponent<DamageHUD>()._damage = damage;
                if (_back.flipX)
                    dmgHUD.GetComponent<DamageHUD>()._dirRight = false;
                else
                    dmgHUD.GetComponent<DamageHUD>()._dirRight = true;
            }

        }
        else
            _currentHP = 0;
        
        // Dead
        if (_currentHP <= 0)  
        {
            CancelInvoke();
            _isDead = true;
            _aniCtrl.SetBool("IsDead", true);

            _col2D.isTrigger = true;
            _rigd.constraints = RigidbodyConstraints2D.FreezePositionY;
            transform.parent.parent.parent.GetComponent<StageControl>().DeadCreature();
        }
        float x = transform.position.x - PlayerControl._instance.transform.position.x;
        if (x < 0)
            _back.flipX = false;
        else
            _back.flipX = true;
    }
    // Dead 애니메이션 이벤트
    IEnumerator Destroy() 
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // NextMove 주사위
    void RandomNextMove()
    {
        _nextMove = Random.Range(-1, 2);
        Invoke("RandomNextMove", 3);
    }
}
