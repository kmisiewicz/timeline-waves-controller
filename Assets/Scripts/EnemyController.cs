using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Chroma.Utility;

public enum EnemyType { RedCapsule = 0, YellowCube = 1 }

public enum PreviousSpawnsHandlingMode
{
    /// <summary>
    /// Wipe currently spawned and start anew
    /// </summary>
    DespawnEverything = 0,

    /// <summary>
    /// Old enemies are still counted and new will spawn after total sum decreases below new values
    /// </summary>
    IncludeInNewCount = 1,

    /// <summary>
    /// Same as DespawnEverything but leaves current enemies
    /// </summary>
    ForgetSpawnedEnemies = 2
}

public class EnemyController : MonoBehaviour
{
    [SerializeField, Min(0.5f)] float _SpawnRadius = 18f;

    [Header("References")]
    [SerializeField] PlayableDirector _PlayableDirector;
    [SerializeField] Transform _Player;
    [SerializeField] EnumNamedArray<GameObject> _EnemyPrefabs = new(typeof(EnemyType));

    [Header("Debug")]
    [SerializeField] int _SpawnedEnemiesCurrent;
    [SerializeField] EnumNamedArray<int> _Enemies = new(typeof(EnemyType));
    [Space]
    [SerializeField] int _SpawnedEnemiesKept;
    [SerializeField] EnumNamedArray<int> _EnemiesToKeep = new(typeof(EnemyType));
    [Space]
    [SerializeField, Min(0f)] float _SpawnInterval = 0.5f;
    [SerializeField, Min(0f)] float _SpawnTimer;

    Dictionary<EnemyType, List<GameObject>> _allSpawnedEnemies;
    Dictionary<EnemyType, int> _enemiesToKeepInternal;
    EnumNamedArray<int> _enemiesInternal;
    WaveData _lastWaveData;
    PreviousSpawnsHandlingMode _previousSpawnsHandlingMode;
    int _spawnedEnemiesCurrentInternal;
    float _spawnTimer;

    public WaveData LastWaveData => _lastWaveData;

    void Start()
    {
        _spawnedEnemiesCurrentInternal = 0;
        _previousSpawnsHandlingMode = PreviousSpawnsHandlingMode.DespawnEverything;
        _spawnTimer = _SpawnInterval;

        _allSpawnedEnemies = new Dictionary<EnemyType, List<GameObject>>();
        foreach (var value in Enum.GetValues(typeof(EnemyType)))
            _allSpawnedEnemies[(EnemyType)value] = new List<GameObject>();

        _enemiesInternal = new EnumNamedArray<int>(typeof(EnemyType));
        _enemiesToKeepInternal = new Dictionary<EnemyType, int>
        {
            { EnemyType.RedCapsule, 0 },
            { EnemyType.YellowCube, 0 }
        };
    }

    void Update()
    {
        if (_enemiesToKeepInternal != null)
            ProcessSpawning();

        RefreshDebugValues();
    }

    public void AssignWaveData(WaveData waveData)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        _lastWaveData = waveData;
        _enemiesToKeepInternal.Clear();
        _enemiesToKeepInternal = waveData.EnemyCounts;
        _previousSpawnsHandlingMode = waveData.PreviousSpawnsHandlingMode;
        if (_previousSpawnsHandlingMode == PreviousSpawnsHandlingMode.DespawnEverything)
        {
            foreach (var kvp in _allSpawnedEnemies)
            {
                foreach (var go in kvp.Value)
                    Destroy(go);
                kvp.Value.Clear();
            }
            foreach (var value in Enum.GetValues(typeof(EnemyType)))
                _enemiesInternal[(EnemyType)value] = 0;
        }

        _SpawnInterval = waveData.SpawnInterval;
        _spawnTimer = Mathf.Min(_spawnTimer, _SpawnInterval);
    }

    void ProcessSpawning()
    {
        if (_spawnTimer > 0)
        {
            _spawnTimer -= Time.deltaTime;
            return;
        }

        if (_previousSpawnsHandlingMode == PreviousSpawnsHandlingMode.IncludeInNewCount
            && _spawnedEnemiesCurrentInternal >= _enemiesToKeepInternal.Sum(x => x.Value))
            return;

        foreach (var kvp in _enemiesToKeepInternal) 
        {
            if (_enemiesInternal[kvp.Key] < kvp.Value)
            {
                SpawnEnemy(kvp.Key);
                break;
            }
        }

        _spawnTimer = _lastWaveData?.SpawnInterval ?? float.MaxValue;
    }

    void SpawnEnemy(EnemyType enemyType)
    {
        Vector3 spawnPosition = UnityEngine.Random.insideUnitSphere;
        spawnPosition.y = 0;
        spawnPosition = _Player.position + spawnPosition.normalized * _SpawnRadius;
        var go = Instantiate(_EnemyPrefabs[enemyType], spawnPosition, Quaternion.identity, transform);
        go.GetComponent<Enemy>().Init(_Player, () => OnEnemyKilled(go, enemyType));
        _allSpawnedEnemies[enemyType].Add(go);
        _enemiesInternal[enemyType]++;
        _spawnedEnemiesCurrentInternal++;
    }

    void OnEnemyKilled(GameObject enemyGO, EnemyType enemyType)
    {
        _enemiesInternal[enemyType]--;
        _allSpawnedEnemies[enemyType].Remove(enemyGO);
        _spawnedEnemiesCurrentInternal--;
    }

    public void StartLevelTimeline()
    {
        _PlayableDirector.Play();

#if UNITY_EDITOR
        UnityEditor.EditorWindow.GetWindow<UnityEditor.Timeline.TimelineEditorWindow>();
        UnityEditor.Selection.SetActiveObjectWithContext(_PlayableDirector, this);
#endif
    }

    void RefreshDebugValues()
    {
        _SpawnedEnemiesKept = _enemiesToKeepInternal.Sum(x => x.Value);
        for (int i = 0; i < _Enemies.Values.Length; i++)
            _Enemies.Values[i] = _enemiesInternal.Values[i];
        for (int i = 0; i < _EnemiesToKeep.Values.Length; i++)
            _EnemiesToKeep.Values[i] = 0;
        foreach (var kvp in _enemiesToKeepInternal)
            _EnemiesToKeep[kvp.Key] = kvp.Value;
        _SpawnedEnemiesCurrent = _spawnedEnemiesCurrentInternal;
        _SpawnInterval = _lastWaveData?.SpawnInterval ?? float.MaxValue;
    }
}

[Serializable]
public class WaveData
{
    [SerializeField] List<EnemyType> _EnemyTypes = new();
    [SerializeField] List<int> _EnemyCounts = new();
    [SerializeField] PreviousSpawnsHandlingMode _PreviousSpawnsHandlingMode;
    [SerializeField, Min(0)] float _SpawnInterval = 0.5f;

    public Dictionary<EnemyType, int> EnemyCounts
    {
        get
        {
            Dictionary<EnemyType, int> dic = new();
            for (int i = 0; i < _EnemyTypes.Count; i++)
                dic.Add(_EnemyTypes[i], _EnemyCounts[i]);
            return dic;
        }
    }

    public PreviousSpawnsHandlingMode PreviousSpawnsHandlingMode
        => _PreviousSpawnsHandlingMode;

    public float SpawnInterval => _SpawnInterval;
}
