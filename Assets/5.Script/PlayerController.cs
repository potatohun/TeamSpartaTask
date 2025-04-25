using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Entity
{
    [Header("Gun Option")]
    [SerializeField] private Transform _gunModel;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _shootCooldown = 0.25f;

    [Header("Bullet Option")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bulletDamage = 10f;
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField][Range(1, 10)] private int _bulletsPerShot = 3;
    [SerializeField][Range(0, 90)] private float _spreadAngle = 15f;

    [Header("Box Collider")]
    [SerializeField] private BoxCollider2D _finalWall;

    private Vector3 _mousePos;
    private float _lastShootTime = -999f;

    private void Update()
    {
        AimGunAtMouse();

        if (Input.GetMouseButtonDown(0) && Time.time >= _lastShootTime + _shootCooldown)
        {
            // 사격 및 쿨타임 설정
            Shoot();
            _lastShootTime = Time.time;
        }
    }

    private void AimGunAtMouse()
    {
        // 마우스 방향으로 총 겨누기
        _mousePos = Input.mousePosition;
        _mousePos.z = Mathf.Abs(Camera.main.transform.position.z);

        // 스크린 기준 좌표를 월드 좌표로 변환
        _mousePos = Camera.main.ScreenToWorldPoint(_mousePos);

        // 방향 및 각도 계산 
        Vector2 direction = _mousePos - _gunModel.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 스프라이트 이미지가 30도 위를 향하고 있으므로 보정
        _gunModel.rotation = Quaternion.Euler(0f, 0f, angle - 30f);
    }

    public void Shoot()
    {
        Vector2 baseDirection = _mousePos - _firePoint.position;

        for (int i = 0; i < _bulletsPerShot; i++)
        {
            // 퍼짐 각도 설정
            float angleOffset = Random.Range(-_spreadAngle / 2f, _spreadAngle / 2f);

            // 기준 방향을 중심으로 각도 회전
            Quaternion spreadRotation = Quaternion.AngleAxis(angleOffset, Vector3.forward);
            Vector2 finalDirection = spreadRotation * baseDirection.normalized;

            // 총알 생성
            GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);

            BulletController bc = bullet.GetComponent<BulletController>();
            bc.Init(_bulletDamage);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            float speed = Random.Range(_bulletSpeed, _bulletSpeed + 5);
            rb.velocity = finalDirection * speed;
        }
    }

    protected override void Die()
    {
        Debug.Log("플레이어 사망. 게임 오버");

        // 게임종료
        if (_finalWall == null)
            return;

        _finalWall.enabled = false;

        transform.DOScale(Vector3.zero, 1f).OnComplete(() => Time.timeScale = 0);
    }
}