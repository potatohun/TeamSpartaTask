using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class MonsterController : Entity
{
    [SerializeField] private float _power = 5f;
    
    private enum State { Idle, Move, Attack, Jump, Die }
    [Header("State")]
    [SerializeField] private State _currentState = State.Idle;

    [Header("Option")]
    [SerializeField] [Range(2, 4)] private float _moveSpeed = 2f;
    [SerializeField] private Vector2 _jumpPower;
    [SerializeField] private float _jumpCooldown = 1f;

    [Header("Ray")]
    [SerializeField]  private LayerMask _rayLayerMask;
    [SerializeField] private float _rayDistanceFront = 0.5f;
    [SerializeField] private float _rayDistanceUp = 1f;

    [Header("Attack Target")]
    [SerializeField] private Entity _targetEntity;

    private List<int> _baseSortingOrders = new List<int>();

    private Animator _animator;
    private Rigidbody2D _rigid;

    private int _layerZ = 0;
    private bool _isJumping = false;
    private float _lastJumpTime = -10f;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody2D>();

        foreach (var renderer in _spriteRenderers)
        {
            // �� ������ sorting order �ʱⰪ ����
            _baseSortingOrders.Add(renderer.sortingOrder); 
        }
    }

    protected override void Start()
    {
        base.Start();

        _moveSpeed = Random.Range(2f, 4f);
    }

    public void Init(int layerZ)
    {
        // ���� �ʱ�ȭ
        _layerZ = layerZ;
        _hp = _maxHp;
        _isJumping = false;
        _lastJumpTime = -10f;
        _targetEntity = null;
        ChangeState(State.Idle);

        // ��ġ �� ���̾� �ʱ�ȭ
        float offset = 0.25f * layerZ;
        transform.position = MonsterManager.instance.GetSpawnPoint().position + new Vector3(0, offset, offset);
        gameObject.layer = LayerMask.NameToLayer($"MonsterZ{layerZ}");
        _rayLayerMask = LayerMask.GetMask($"MonsterZ{_layerZ}", "Box", "Player");

        // �� ������ sorting order �ʱ�ȭ
        for (int i = 0; i < _spriteRenderers.Count; i++)
        {
            _spriteRenderers[i].sortingOrder = _baseSortingOrders[i] + 100 / (_layerZ + 1);
        }

        // �ǰ� ���� �ʱ�ȭ
        foreach (var sprite in _spriteRenderers)
            sprite.color = Color.white;

        // �ִϸ��̼� ���� �ʱ�ȭ
        _animator.SetBool("IsAttacking", false);
        _animator.SetBool("IsDead", false);
        _animator.SetBool("IsIdle", true);

        // Hp UI �ʱ�ȭ
        _hpPanel.Init();
    }

    private void Update()
    {
        if (_currentState == State.Die ) 
            return;

        DetectFront();
        DoAction();

        if (_currentState == State.Idle)
            return;
    }

    private void DetectFront()
    {
        // ���� ����

        // ���� ��ġ �� ���� ����
        Vector2 origin = transform.position + Vector3.up * 0.75f;
        Vector2 dir = Vector2.left;

        // ����ĳ��Ʈ
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, _rayDistanceFront, _rayLayerMask);

        // �ڱ� �ڽŸ� ���� => �տ� �ƹ��͵� ����
        if (hits.Length == 1)
        {
            ChangeState(State.Move);
            return;
        }

        // �տ� ��ֹ� ����
        foreach (var hit in hits)
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj == gameObject) 
                continue;

            if (hit.collider.CompareTag("Box") || hit.collider.CompareTag("Player"))
            {
                // Box, Player -> ����
                _targetEntity = hit.collider.GetComponent<Entity>();
                if(_targetEntity != null)
                {
                    ChangeState(State.Attack);
                    return;
                }
            }

            if (hit.collider.CompareTag("Monster"))
            {
                // ���� ���̾��� Monster -> �پ�ѱ�
                MonsterController other = hit.collider.GetComponentInParent<MonsterController>();
                if (other == null)
                    continue;

                if(other._layerZ != this._layerZ)
                    continue;

                // ���� ó��
                if (CanJump())
                    ChangeState(State.Jump);
                else
                    ChangeState(State.Idle);

                return;
            }
        }
    }

    private void DoAction()
    {
        switch (_currentState)
        {
            case State.Idle: 
                // �ƹ� �ൿ�� ����
                break;
            case State.Move: 
                HandleMove(); 
                break;
            case State.Attack: 
                HandleAttack(); 
                break;
            case State.Jump: 
                HandleJump(); 
                break;
        }
    }

    private void HandleMove()
    {
        transform.position += Vector3.left * _moveSpeed * Time.deltaTime;
    }

    private void HandleAttack()
    {
        _animator.SetBool("IsAttacking", true);
    }

    private void OnAttack()
    {
        // Attack �ִϸ��̼� Ŭ���� OnAttack �̺�Ʈ ���
        if (_targetEntity != null)
            _targetEntity.TakeDamage(_power);

        _animator.SetBool("IsAttacking", false);
        ChangeState(State.Idle);
    }

    private void HandleJump()
    {
        if (_isJumping) 
            return;

        _isJumping = true;
        _lastJumpTime = Time.time;
        _rigid.AddForce(_jumpPower, ForceMode2D.Impulse);
        Invoke("FinishJump", 0.6f);
    }

    private void FinishJump()
    {
        _isJumping = false;
        ChangeState(State.Idle);
    }

    private bool CanJump()
    {
        if (Time.time < _lastJumpTime + _jumpCooldown)
            return false;

        Vector2 origin = transform.position + Vector3.up;
        Vector2 dir = Vector2.up;

        // �Ӹ� �� ����
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, _rayDistanceUp, _rayLayerMask);
        if (hits.Length > 1)
            return false;
        else
            return true;
    }

    private void ChangeState(State newState)
    {
        if (_currentState == newState) 
            return;

        _currentState = newState;
    }

    protected override void Die()
    {
        base.Die();

        ChangeState(State.Die);
        _animator.SetBool("IsDead", true);

        // ������Ʈ Ǯ�� ����
        MonsterManager.instance.ReturnToPool(this.gameObject);
    }
}
