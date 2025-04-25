using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float _damage = 10f;

    public void Init(float value)
    {
        // ������ �ʱ�ȭ
        _damage = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            // ���Ϳ� �浹 ��, ���� �ǰ� �� bullet ����
            MonsterController monster = collision.GetComponent<MonsterController>();
            if (monster == null)
                return;

            monster.TakeDamage(_damage);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            // �� Ȥ�� ���� �浹 ��, �ٷ� bullet ���� 
            Destroy(gameObject);
        }
    }
}
