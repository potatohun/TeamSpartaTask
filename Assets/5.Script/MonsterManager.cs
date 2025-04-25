using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager instance;

    [Header("Monster Spawn Option")]
    [SerializeField] private List<GameObject> _monsterPrefab;
    [SerializeField] private float _spawnDelay = 2f;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private int _minLayerZ = 0;
    [SerializeField] private int _maxLayerZ = 3;

    [Header("Monster Pool Option")]
    [SerializeField] private int _poolSize = 10;
    private Queue<GameObject> _monsterPool = new Queue<GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            return;
    }
    private void Start()
    {
        // Ǯ �ʱ�ȭ �� ���� ���� ����
        _monsterPool.Clear();
        InitPool(_poolSize);
        StartCoroutine(SpawnMonsterRoutine());
    }

    private void InitPool(int size)
    {
        for (int i = 0; i < size; i++)
        {
            // �����տ��� �������� ��ü ����
            int index = Random.Range(0, _monsterPrefab.Count);
            GameObject obj = Instantiate(_monsterPrefab[index]);
            obj.SetActive(false);

            // Ǯ�� ���
            _monsterPool.Enqueue(obj);
        }
    }

    private IEnumerator SpawnMonsterRoutine()
    {
        while (true)
        {
            SpawnMonster();
            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    private void SpawnMonster()
    {
        if (_monsterPool.Count == 0)
        {
            // Ǯ ũ�⸸ŭ �߰� Ȯ��
            InitPool(_poolSize); 
        }

        // Ǯ���� ��ü ��������
        GameObject monster = _monsterPool.Dequeue();

        // ��ü ��ġ �ʱ�ȭ
        int layerZ = Random.Range(_minLayerZ, _maxLayerZ + 1);
        float offset = 0.25f * layerZ;
        Vector3 basePos = _spawnPoint.position;
        Vector3 adjustedPos = basePos + new Vector3(0, offset, offset);

        // ���̾� ����
        monster.transform.position = adjustedPos;
        monster.layer = LayerMask.NameToLayer($"MonsterZ{layerZ}");

        // MonsterController �ʱ�ȭ
        MonsterController controller = monster.GetComponent<MonsterController>();
        monster.SetActive(true);
        controller.Init(layerZ);
    }

    public void ReturnToPool(GameObject monster)
    {
        // Ǯ�� ��ü ����ֱ�
        monster.SetActive(false);
        _monsterPool.Enqueue(monster);
    }

    public Transform GetSpawnPoint()
    {
        return _spawnPoint;
    }
}

