using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float _damage = 10f;

    public void Init(float value)
    {
        // 데미지 초기화
        _damage = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            // 몬스터와 충돌 시, 몬스터 피격 후 bullet 제거
            MonsterController monster = collision.GetComponent<MonsterController>();
            if (monster == null)
                return;

            monster.TakeDamage(_damage);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            // 벽 혹은 땅과 충돌 시, 바로 bullet 제거 
            Destroy(gameObject);
        }
    }
}
