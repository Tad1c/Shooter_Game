using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "Game/Wave Data", order = 1)]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Info")]
        [SerializeField] private string _waveName = "New Wave";
        [SerializeField, TextArea(2, 4)] private string _description;

        [Header("Spawn Configuration")]
        [SerializeField] private List<EnemySpawnInfo> _enemySpawns = new();
        [SerializeField, Range(0.1f, 10f)] private float _spawnInterval = 2f;
        [SerializeField, Range(1, 10)] private int _enemiesPerSpawn = 1;

        [Header("Wave Completion")]
        [SerializeField] private WaveCompletionType _completionType = WaveCompletionType.KillCount;
        [SerializeField] private int _requiredKills = 20;
        [SerializeField] private float _waveDuration = 30f;

        public string WaveName => _waveName;
        public string Description => _description;
        public List<EnemySpawnInfo> EnemySpawns => _enemySpawns;
        public float SpawnInterval => _spawnInterval;
        public int EnemiesPerSpawn => _enemiesPerSpawn;
        public WaveCompletionType CompletionType => _completionType;
        public int RequiredKills => _requiredKills;
        public float WaveDuration => _waveDuration;

        public int TotalEnemyTypes => _enemySpawns.Count;

        public float GetTotalSpawnWeight()
        {
            float total = 0;
            foreach (var spawn in _enemySpawns)
            {
                total += spawn.SpawnWeight;
            }
            return total;
        }
    }

    [Serializable]
    public class EnemySpawnInfo
    {
        [SerializeField] private EnemyData _enemyData;
        [SerializeField, Range(0f, 100f)] private float _spawnWeight = 50f;
        [SerializeField] private bool _isEnabled = true;

        public EnemyData EnemyData => _enemyData;
        public float SpawnWeight => _spawnWeight;
        public bool IsEnabled => _isEnabled;
    }

    public enum WaveCompletionType
    {
        KillCount,      // Complete after X enemies killed
        TimeBased,      // Complete after X seconds
        AllEnemiesDead, // Complete when no enemies remain
        Manual          // Manually triggered
    }
}

