using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Scripts
{
    public class EnemySpawnService : IEnemySpawnService, ITickable, IDisposable
    {
        private readonly Camera _mainCamera;
        private readonly PlayerView _playerView;
        private readonly IPlayerModel _playerModel;
        private readonly EnemyPresenter.Factory _enemyFactory;
        private readonly LevelConfiguration _levelConfig;
        private readonly Transform _enemyParent;

        private readonly List<EnemyPresenter> _activeEnemies = new();
        private readonly Subject<EnemyPresenter> _onEnemySpawned = new();
        private readonly Subject<EnemyPresenter> _onEnemyDespawned = new();
        private readonly ReactiveProperty<int> _activeEnemyCount = new(0);
        private readonly CompositeDisposable _disposer = new();

        private WaveData _currentWaveData;
        private bool _isSpawning;
        private float _spawnInterval = 2f;
        private int _enemiesPerSpawn = 1;
        private IDisposable _spawnTimer;

        public IObservable<EnemyPresenter> OnEnemySpawned => _onEnemySpawned;
        public IObservable<EnemyPresenter> OnEnemyDespawned => _onEnemyDespawned;
        public IReadOnlyReactiveProperty<int> ActiveEnemyCount => _activeEnemyCount;

        public EnemySpawnService(
            Camera mainCamera,
            PlayerView playerView,
            IPlayerModel playerModel,
            EnemyPresenter.Factory enemyFactory,
            LevelConfiguration levelConfig)
        {
            _mainCamera = mainCamera;
            _playerView = playerView;
            _playerModel = playerModel;
            _enemyFactory = enemyFactory;
            _levelConfig = levelConfig;

            // Create parent for enemy organization
            _enemyParent = new GameObject("Enemies").transform;
        }

        public void SetCurrentWave(WaveData waveData)
        {
            _currentWaveData = waveData;
            if (_currentWaveData != null)
            {
                SetSpawnParameters(_currentWaveData.SpawnInterval, _currentWaveData.EnemiesPerSpawn);
            }
        }

        public void SetSpawnParameters(float interval, int enemiesPerSpawn)
        {
            _spawnInterval = interval;
            _enemiesPerSpawn = enemiesPerSpawn;

            // Restart spawning if already active
            if (_isSpawning)
            {
                StopSpawning();
                StartSpawning();
            }
        }

        public void StartSpawning()
        {
            if (_isSpawning) return;

            _isSpawning = true;

            _spawnTimer?.Dispose();
            _spawnTimer = Observable.Interval(TimeSpan.FromSeconds(_spawnInterval))
                .Subscribe(_ => SpawnEnemies())
                .AddTo(_disposer);
        }

        public void StopSpawning()
        {
            _isSpawning = false;
            _spawnTimer?.Dispose();
        }

        private void SpawnEnemies()
        {
            if (_currentWaveData == null || _currentWaveData.EnemySpawns.Count == 0)
                return;

            for (int i = 0; i < _enemiesPerSpawn; i++)
            {
                SpawnRandomEnemy();
            }
        }

        private void SpawnRandomEnemy()
        {
            var enemyData = SelectRandomEnemyData();
            if (enemyData == null) return;

            var spawnPosition = GetRandomOffScreenPosition();
            SpawnEnemy(enemyData, spawnPosition);
        }

        private EnemyData SelectRandomEnemyData()
        {
            var enabledSpawns = _currentWaveData.EnemySpawns
                .Where(s => s.IsEnabled && s.EnemyData != null)
                .ToList();

            if (enabledSpawns.Count == 0)
                return null;

            float totalWeight = enabledSpawns.Sum(s => s.SpawnWeight);
            float randomValue = Random.Range(0f, totalWeight);

            float currentWeight = 0f;
            foreach (var spawn in enabledSpawns)
            {
                currentWeight += spawn.SpawnWeight;
                if (randomValue <= currentWeight)
                {
                    return spawn.EnemyData;
                }
            }

            return enabledSpawns[0].EnemyData;
        }

        private void SpawnEnemy(EnemyData enemyData, Vector2 position)
        {
            // Instantiate enemy prefab
            var enemyView = Object.Instantiate(enemyData.EnemyPrefab, position, Quaternion.identity, _enemyParent);

            if (enemyView == null)
            {
                Debug.LogError($"Enemy prefab {enemyData.EnemyPrefab.name} is missing EnemyView component!");
                Object.Destroy(enemyView.gameObject);
                return;
            }

            // Initialize view with data
            enemyView.Initialize(enemyData);
            enemyView.SetPosition(position);

            // Create model
            var enemyModel = new EnemyModel(enemyData);

            // Create presenter using factory
            var enemyPresenter = _enemyFactory.Create(enemyModel, enemyView, _playerModel, _playerView);
            enemyPresenter.Initialize();

            // Track enemy
            _activeEnemies.Add(enemyPresenter);
            _activeEnemyCount.Value = _activeEnemies.Count;

            // Subscribe to death
            enemyModel.IsAlive
                .Where(isAlive => !isAlive)
                .Subscribe(_ => OnEnemyDied(enemyPresenter))
                .AddTo(_disposer);

            _onEnemySpawned.OnNext(enemyPresenter);
        }

        private void OnEnemyDied(EnemyPresenter enemy)
        {
            // Delay removal to allow death animation
            Observable.Timer(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => RemoveEnemy(enemy))
                .AddTo(_disposer);
        }

        private void RemoveEnemy(EnemyPresenter enemy)
        {
            if (_activeEnemies.Remove(enemy))
            {
                _activeEnemyCount.Value = _activeEnemies.Count;
                _onEnemyDespawned.OnNext(enemy);

                enemy.Dispose();
                if (enemy.View != null && enemy.View.gameObject != null)
                {
                    Object.Destroy(enemy.View.gameObject);
                }
            }
        }

        public void Tick()
        {
            if (!_isSpawning) return;

            // Update target positions and check for respawn
            foreach (var enemy in _activeEnemies.ToList())
            {
                if (!enemy.Model.IsAlive.Value)
                    continue;

                // Update target to player position
                enemy.Model.SetTargetPosition(_playerView.Position);

                // Check if enemy is too far from player (respawn logic)
                float distance = Vector2.Distance(enemy.View.Position, _playerView.Position);
                if (distance > _levelConfig.DespawnDistance)
                {
                    // Respawn closer to player
                    var newPosition = GetRandomOffScreenPosition();
                    enemy.View.SetPosition(newPosition);
                }
            }
        }

        private Vector2 GetRandomOffScreenPosition()
        {
            if (_mainCamera == null)
            {
                return _playerView.Position + Random.insideUnitCircle.normalized * 10f;
            }

            // Get camera bounds
            float cameraHeight = _mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * _mainCamera.aspect;

            Vector2 playerPos = _playerView.Position;
            float spawnDistance = _levelConfig.OffScreenSpawnDistance;

            // Choose random side: 0=top, 1=right, 2=bottom, 3=left
            int side = Random.Range(0, 4);
            Vector2 spawnPos = playerPos;

            switch (side)
            {
                case 0: // Top
                    spawnPos.y += cameraHeight / 2f + spawnDistance;
                    spawnPos.x += Random.Range(-cameraWidth / 2f, cameraWidth / 2f);
                    break;
                case 1: // Right
                    spawnPos.x += cameraWidth / 2f + spawnDistance;
                    spawnPos.y += Random.Range(-cameraHeight / 2f, cameraHeight / 2f);
                    break;
                case 2: // Bottom
                    spawnPos.y -= cameraHeight / 2f + spawnDistance;
                    spawnPos.x += Random.Range(-cameraWidth / 2f, cameraWidth / 2f);
                    break;
                case 3: // Left
                    spawnPos.x -= cameraWidth / 2f + spawnDistance;
                    spawnPos.y += Random.Range(-cameraHeight / 2f, cameraHeight / 2f);
                    break;
            }

            return spawnPos;
        }

        public void DespawnAll()
        {
            foreach (var enemy in _activeEnemies.ToList())
            {
                RemoveEnemy(enemy);
            }

            _activeEnemies.Clear();
            _activeEnemyCount.Value = 0;
        }

        public IReadOnlyList<EnemyPresenter> GetActiveEnemies()
        {
            return _activeEnemies.AsReadOnly();
        }

        public void Dispose()
        {
            StopSpawning();
            DespawnAll();
            _disposer?.Dispose();
            _onEnemySpawned?.Dispose();
            _onEnemyDespawned?.Dispose();

            if (_enemyParent != null)
            {
                Object.Destroy(_enemyParent.gameObject);
            }
        }
    }
}

