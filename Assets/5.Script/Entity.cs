using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] protected float _maxHp = 100f;
    [SerializeField] protected float _hp;

    protected HpPanel _hpPanel;
    protected List<SpriteRenderer> _spriteRenderers = new List<SpriteRenderer>();

    protected virtual void Awake()
    {
        _hpPanel = GetComponentInChildren<HpPanel>();
        _spriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
    }

    protected virtual void Start()
    {
        // 체력 및 UI 초기화
        _hp = _maxHp;
        _hpPanel.Init();
    }

    public virtual void TakeDamage(float value)
    {
        // 피격
        _hp -= value;
        if (_hp <= 0f)
        {
            Die();
        }

        // 연출
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.color = Color.black * 0.5f;
            spriteRenderer.DOColor(Color.white, 0.25f).SetEase(Ease.OutSine);
        }

        // HP UI 업데이트
        _hpPanel.UpdateValue(_hp / _maxHp);
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + "처치!");
    }
}
