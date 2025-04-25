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
            // 각 파츠별 sorting order 초기값 저장
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
        // 정보 초기화
        _layerZ = layerZ;
        _hp = _maxHp;
        _isJumping = false;
        _lastJumpTime = -10f;
        _targetEntity = null;
        ChangeState(State.Idle);

        // 위치 및 레이어 초기화
        float offset = 0.25f * layerZ;
        transform.position = MonsterManager.instance.GetSpawnPoint().position + new Vector3(0, offset, offset);
        gameObject.layer = LayerMask.NameToLayer($"MonsterZ{layerZ}");
        _rayLayerMask = LayerMask.GetMask($"MonsterZ{_layerZ}", "Box", "Player");

        // 각 파츠별 sorting order 초기화
        for (int i = 0; i < _spriteRenderers.Count; i++)
        {
            _spriteRenderers[i].sortingOrder = _baseSortingOrders[i] + 100 / (_layerZ + 1);
        }

        // 피격 연출 초기화
        foreach (var sprite in _spriteRenderers)
            sprite.color = Color.white;

        // 애니메이션 상태 초기화
        _animator.SetBool("IsAttacking", false);
        _animator.SetBool("IsDead", false);
        _animator.SetBool("IsIdle", true);

        // Hp UI 초기화
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
        // 전방 감지

        // 기준 위치 및 방향 설정
        Vector2 origin = transform.position + Vector3.up * 0.75f;
        Vector2 dir = Vector2.left;

        // 레이캐스트
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, _rayDistanceFront, _rayLayerMask);

        // 자기 자신만 존재 => 앞에 아무것도 없음
        if (hits.Length == 1)
        {
            ChangeState(State.Move);
            return;
        }

        // 앞에 장애물 존재
        foreach (var hit in hits)
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj == gameObject) 
                continue;

            if (hit.collider.CompareTag("Box") || hit.collider.CompareTag("Player"))
            {
                // Box, Player -> 공격
                _targetEntity = hit.collider.GetComponent<Entity>();
                if(_targetEntity != null)
                {
                    ChangeState(State.Attack);
                    return;
                }
            }

            if (hit.collider.CompareTag("Monster"))
            {
                // 같은 레이어의 Monster -> 뛰어넘기
                MonsterController other = hit.collider.GetComponentInParent<MonsterController>();
                if (other == null)
                    continue;

                if(other._layerZ != this._layerZ)
                    continue;

                // 점프 처리
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
                // 아무 행동도 안함
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
        // Attack 애니메이션 클립에 OnAttack 이벤트 등록
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

        // 머리 위 감지
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

        // 오브젝트 풀로 복귀
        MonsterManager.instance.ReturnToPool(this.gameObject);
    }
}
