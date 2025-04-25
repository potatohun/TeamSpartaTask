using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoxManager : MonoBehaviour
{
    public static BoxManager instance;

    [Header("Box List")]
    [SerializeField] private List<BoxController> _boxList;

    [Header("Player")]
    [SerializeField] private PlayerController _playerController;

    [Header("Offset Pos")]
    [SerializeField] private float _boxPosY = 0f;
    [SerializeField] private float _playerPosY = 0f;
    [SerializeField] private float _boxSpacing = 1f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            return;
    }

    private void Start()
    {
        // 박스 리스트 초기화
        _boxList.Clear();
        foreach (Transform child in transform)
        {
            BoxController box = child.GetComponent<BoxController>();
            if (box != null)
            {
                _boxList.Add(box);
            }
        }

        if (_boxList.Count > 1)
        {
            _boxPosY = _boxList[0].transform.position.y;
            if (_boxList.Count > 1)
            {
                _boxSpacing = _boxList[1].transform.position.y - _boxList[0].transform.position.y;
            }
        }

        _playerPosY = _playerController.transform.position.y;
    }
    public void OnBoxBroken(BoxController boxToRemove)
    {
        int index = _boxList.IndexOf(boxToRemove);
        if (index == -1) 
            return;

        _boxList.RemoveAt(index);

        // 플레이어 내리기
        if (_playerController != null)
        {
            _playerPosY = _playerPosY - _boxSpacing;
            _playerController.transform.DOMoveY(_playerPosY, 0.3f).SetEase(Ease.InSine);
        }

        // 박스 내리기
        for (int i = index; i < _boxList.Count; i++)
        {
            float targetPos = _boxPosY + i * _boxSpacing;
            _boxList[i].transform.DOMoveY(targetPos, 0.3f).SetEase(Ease.InSine);
        }
    }
}
