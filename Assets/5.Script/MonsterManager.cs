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
        // 풀 초기화 및 몬스터 생성 시작
        _monsterPool.Clear();
        InitPool(_poolSize);
        StartCoroutine(SpawnMonsterRoutine());
    }

    private void InitPool(int size)
    {
        for (int i = 0; i < size; i++)
        {
            // 프리팹에서 랜덤으로 객체 생성
            int index = Random.Range(0, _monsterPrefab.Count);
            GameObject obj = Instantiate(_monsterPrefab[index]);
            obj.SetActive(false);

            // 풀에 담기
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
            // 풀 크기만큼 추가 확장
            InitPool(_poolSize); 
        }

        // 풀에서 객체 가져오기
        GameObject monster = _monsterPool.Dequeue();

        // 객체 위치 초기화
        int layerZ = Random.Range(_minLayerZ, _maxLayerZ + 1);
        float offset = 0.25f * layerZ;
        Vector3 basePos = _spawnPoint.position;
        Vector3 adjustedPos = basePos + new Vector3(0, offset, offset);

        // 레이어 설정
        monster.transform.position = adjustedPos;
        monster.layer = LayerMask.NameToLayer($"MonsterZ{layerZ}");

        // MonsterController 초기화
        MonsterController controller = monster.GetComponent<MonsterController>();
        monster.SetActive(true);
        controller.Init(layerZ);
    }

    public void ReturnToPool(GameObject monster)
    {
        // 풀에 객체 집어넣기
        monster.SetActive(false);
        _monsterPool.Enqueue(monster);
    }

    public Transform GetSpawnPoint()
    {
        return _spawnPoint;
    }
}

