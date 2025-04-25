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
            // ��� �� ��Ÿ�� ����
            Shoot();
            _lastShootTime = Time.time;
        }
    }

    private void AimGunAtMouse()
    {
        // ���콺 �������� �� �ܴ���
        _mousePos = Input.mousePosition;
        _mousePos.z = Mathf.Abs(Camera.main.transform.position.z);

        // ��ũ�� ���� ��ǥ�� ���� ��ǥ�� ��ȯ
        _mousePos = Camera.main.ScreenToWorldPoint(_mousePos);

        // ���� �� ���� ��� 
        Vector2 direction = _mousePos - _gunModel.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // ��������Ʈ �̹����� 30�� ���� ���ϰ� �����Ƿ� ����
        _gunModel.rotation = Quaternion.Euler(0f, 0f, angle - 30f);
    }

    public void Shoot()
    {
        Vector2 baseDirection = _mousePos - _firePoint.position;

        for (int i = 0; i < _bulletsPerShot; i++)
        {
            // ���� ���� ����
            float angleOffset = Random.Range(-_spreadAngle / 2f, _spreadAngle / 2f);

            // ���� ������ �߽����� ���� ȸ��
            Quaternion spreadRotation = Quaternion.AngleAxis(angleOffset, Vector3.forward);
            Vector2 finalDirection = spreadRotation * baseDirection.normalized;

            // �Ѿ� ����
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
        Debug.Log("�÷��̾� ���. ���� ����");

        // ��������
        if (_finalWall == null)
            return;

        _finalWall.enabled = false;

        transform.DOScale(Vector3.zero, 1f).OnComplete(() => Time.timeScale = 0);
    }
}