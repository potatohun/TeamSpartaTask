using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HpPanel : MonoBehaviour
{
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponentInChildren<Slider>();
    }

    public void Init()
    {
        // �ʱ�ȭ
        _slider.value = 1f;
        gameObject.SetActive(false);
    }

    public void UpdateValue(float value)
    {
        // UI off �����̸�, UI on
        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        // �����̴� value ������Ʈ
        _slider.value = value;
    }
}
