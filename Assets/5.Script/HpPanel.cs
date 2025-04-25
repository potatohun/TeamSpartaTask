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
        // 초기화
        _slider.value = 1f;
        gameObject.SetActive(false);
    }

    public void UpdateValue(float value)
    {
        // UI off 상태이면, UI on
        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        // 슬라이더 value 업데이트
        _slider.value = value;
    }
}
