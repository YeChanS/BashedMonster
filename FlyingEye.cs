using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEye : MonoBehaviour
{
    [SerializeField] Transform _attackBoxLeftPos;
    [SerializeField] Transform _attackBoxRightPos;
    [SerializeField] Vector2 _attackBoxSize;

    [SerializeField] GameObject _preBullet;
    [SerializeField] Transform _shootLeftPos;
    [SerializeField] Transform _shootRightPos;

    [SerializeField] Transform _damageHUDPos;
    [SerializeField] GameObject _prefDamageHUD;

    Rigidbody2D _rigd;
    public int _nextMove;
    Animator _aniCtrl;
    SpriteRenderer _back;
    CapsuleCollider2D _col2D;

    public bool _isBattle = false;
    bool _isAttack = false;
    bool _isInvoke = false;
    bool _isDead = false;
    public int _maxHP = 20;
    public int _currentHP = 0;
    public int _damage = 3;



    static FlyingEye _uniqueInstance;
    public static FlyingEye _instance
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
        Invoke("RandomNextMove", 2);
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (_isDead)
            return;

        if (Vector2.Distance(transform.position, PlayerControl._instance.transform.position) < 10f)
        {
            _isBattle = true;
            float x = transform.position.x - PlayerControl._instance.transform.position.x;
            if (x < 0)
                _back.flipX = false;
            else
                _back.flipX = true;
        }
        else
        {
            _isBattle = false;
            _isAttack = false;
        }


        if (_isBattle)
        {
            AttackPhase();
        }
        else 
        {
            if (!_isInvoke)
            {
                CancelInvoke();
                Invoke("RandomNextMove", 2);
                _isInvoke = true;
            }

            if (_nextMove > 0)
            {
                _back.flipX = false;
                transform.Translate(new Vector3(1 * 0.01f, 0, 0));
            }
            else
            {
                _back.flipX = true;
                transform.Translate(new Vector3(-1 * 0.01f, 0, 0));
            }
            

            Vector2 frontVec = new Vector2(_rigd.position.x + _nextMove * 0.5f, _rigd.position.y);
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector2.down, 1, LayerMask.GetMask("Ground"));
            RaycastHit2D rayHitS = Physics2D.Raycast(frontVec, Vector2.down, 1, LayerMask.GetMask("FootHold"));
            RaycastHit2D rayHit2 = Physics2D.Raycast(frontVec, Vector2.left, 1, LayerMask.GetMask("Ground"));
            RaycastHit2D rayHit3 = Physics2D.Raycast(frontVec, Vector2.right, 1, LayerMask.GetMask("Ground"));
            if (rayHit.collider == null && rayHitS.collider == null || rayHit2.collider != null || rayHit3.collider != null)
            {
                _nextMove *= -1;
                CancelInvoke();
                Invoke("RandomNextMove", 3);
            }
        }
    }

    // 애니메이션 이벤트.
    void Fire()
    {
        if (_back.flipX)
        {
            Instantiate(_preBullet, _shootLeftPos.position, Quaternion.identity);
        }
        else
        {
            Instantiate(_preBullet, _shootRightPos.position, Quaternion.identity);
        }
    }

    public int AttackDamage()
    {
        return _damage; 
    }

    void AttackPhase()
    {
        if (_isInvoke)
        {
            CancelInvoke();
            _isInvoke = false;
            Invoke("Attack", 1);
        }
    }

    // 상대방과의 거리에따라 공격모드 변경.
    void Attack()
    {
        if (Vector2.Distance(transform.position, PlayerControl._instance.transform.position) < 5f)
        {
            _damage = 3;
            _aniCtrl.SetTrigger("Attack");
            Invoke("Attack", 2);
        }
        else
        {
            _damage = 8;
            _aniCtrl.SetTrigger("Shoot");
            Invoke("Attack", 3);
        }
        if (_back.flipX)
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_attackBoxLeftPos.position, _attackBoxSize, 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Player")
                    collider.transform.GetComponent<PlayerControl>().TakeDamage(_damage, transform.position);
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
    // 데미지 연산.
    public void TakeDamage(int damage)
    {
        if (_isDead)
            return;

        if (_currentHP > 0)
            _currentHP -= damage;
        else
            _currentHP = 0;

        _aniCtrl.SetTrigger("Hit");

        GameObject dmgHUD = Instantiate(_prefDamageHUD, _damageHUDPos.position, Quaternion.identity);
        dmgHUD.GetComponent<DamageHUD>()._damage = damage;
        if(_back.flipX)
            dmgHUD.GetComponent<DamageHUD>()._dirRight = false;
        else
            dmgHUD.GetComponent<DamageHUD>()._dirRight = true;

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

    // 다음이동 주사위.
    void RandomNextMove()
    {
        _nextMove = Random.Range(-1, 2);
        Invoke("RandomNextMove", 2);
    }

    // 사망 애니메이션
    IEnumerator Destroy() 
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
