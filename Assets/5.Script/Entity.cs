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
        // ü�� �� UI �ʱ�ȭ
        _hp = _maxHp;
        _hpPanel.Init();
    }

    public virtual void TakeDamage(float value)
    {
        // �ǰ�
        _hp -= value;
        if (_hp <= 0f)
        {
            Die();
        }

        // ����
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.color = Color.black * 0.5f;
            spriteRenderer.DOColor(Color.white, 0.25f).SetEase(Ease.OutSine);
        }

        // HP UI ������Ʈ
        _hpPanel.UpdateValue(_hp / _maxHp);
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + "óġ!");
    }
}
