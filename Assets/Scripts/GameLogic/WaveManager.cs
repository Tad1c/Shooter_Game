using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts {
	public class WaveManager : IWaveManager, IInitializable, ITickable, IDisposable {
		private readonly LevelConfiguration _levelConfig;
		private readonly IEnemySpawnService _spawnService;
		private readonly IGameStatsModel _gameStatsModel;

		private readonly ReactiveProperty<int> _currentWaveIndex = new(-1);
		private readonly ReactiveProperty<int> _enemiesKilledInWave = new(0);
		private readonly ReactiveProperty<float> _waveTimer = new(0f);
		private readonly ReactiveProperty<bool> _isWaveActive = new(false);

		private readonly Subject<WaveData> _onWaveStarted = new();
		private readonly Subject<WaveData> _onWaveCompleted = new();
		private readonly Subject<Unit> _onAllWavesCompleted = new();

		private readonly CompositeDisposable _disposer = new();

		private WaveData _currentWave;
		private bool _isLevelActive;

		public IReadOnlyReactiveProperty<int> CurrentWaveIndex => _currentWaveIndex;
		public IReadOnlyReactiveProperty<int> EnemiesKilledInWave => _enemiesKilledInWave;
		public IReadOnlyReactiveProperty<float> WaveTimer => _waveTimer;
		public IReadOnlyReactiveProperty<bool> IsWaveActive => _isWaveActive;

		public IObservable<WaveData> OnWaveStarted => _onWaveStarted;
		public IObservable<WaveData> OnWaveCompleted => _onWaveCompleted;
		public IObservable<Unit> OnAllWavesCompleted => _onAllWavesCompleted;

		public WaveManager(LevelConfiguration levelConfig, IEnemySpawnService spawnService, IGameStatsModel gameStatsModel) {
			_levelConfig = levelConfig;
			_spawnService = spawnService;
			_gameStatsModel = gameStatsModel;
		}

		public void Initialize() {
			_spawnService.OnEnemyDespawned
				.Where(_ => _isWaveActive.Value)
				.Subscribe(enemy => {
					if (!enemy.Model.IsAlive.Value) {
						_enemiesKilledInWave.Value++;
						_gameStatsModel.IncrementKills();
					}
				})
				.AddTo(_disposer);
		}

		public void StartLevel() {
			if (_isLevelActive) return;

			_isLevelActive = true;
			_currentWaveIndex.Value = -1;

			NextWave();
		}

		public void StopLevel() {
			_isLevelActive = false;
			_isWaveActive.Value = false;
			_spawnService.StopSpawning();
			_spawnService.DespawnAll();
		}

		public async void NextWave() {
			if (!_isLevelActive) {
				return;
			}

			if (_currentWave != null) {
				_isWaveActive.Value = false;
				_spawnService.StopSpawning();
				_onWaveCompleted.OnNext(_currentWave);

				await UniTask.Delay(TimeSpan.FromSeconds(_levelConfig.DelayBetweenWaves));
			}

			_currentWaveIndex.Value++;

			if (_currentWaveIndex.Value >= _levelConfig.TotalWaves) {
				CompleteAllWaves();
				return;
			}

			StartWave(_currentWaveIndex.Value);
		}

		private void StartWave(int waveIndex) {
			_currentWave = _levelConfig.GetWave(waveIndex);

			if (_currentWave == null) {
				Debug.LogError($"Wave {waveIndex} not found in level configuration!");
				return;
			}

			_enemiesKilledInWave.Value = 0;
			_waveTimer.Value = 0f;
			_isWaveActive.Value = true;

			var spawnService = _spawnService as EnemySpawnService;
			spawnService?.SetCurrentWave(_currentWave);

			_spawnService.SetSpawnParameters(_currentWave.SpawnInterval, _currentWave.EnemiesPerSpawn);
			_spawnService.StartSpawning();

			_onWaveStarted.OnNext(_currentWave);

			Debug.Log($"Wave {waveIndex + 1} started: {_currentWave.WaveName}");
		}

		public void Tick() {
			if (!_isWaveActive.Value || _currentWave == null)
				return;

			_waveTimer.Value += Time.deltaTime;

			CheckWaveCompletion();
		}

		private void CheckWaveCompletion() {
			bool shouldComplete = false;

			switch (_currentWave.CompletionType) {
				case WaveCompletionType.KillCount:
					if (_enemiesKilledInWave.Value >= _currentWave.RequiredKills) {
						shouldComplete = true;
					}
					break;

				case WaveCompletionType.TimeBased:
					if (_waveTimer.Value >= _currentWave.WaveDuration) {
						shouldComplete = true;
					}
					break;

				case WaveCompletionType.AllEnemiesDead:
					if (_enemiesKilledInWave.Value > 0 && _spawnService.ActiveEnemyCount.Value == 0) {
						shouldComplete = true;
					}
					break;

				case WaveCompletionType.Manual:
					// Manually triggered via NextWave()
					break;
			}

			if (shouldComplete) {
				NextWave();
			}
		}

		private void CompleteAllWaves() {
			_isLevelActive = false;
			_isWaveActive.Value = false;
			_spawnService.StopSpawning();

			_onAllWavesCompleted.OnNext(Unit.Default);

			Debug.Log("All waves completed!");
		}
		public WaveData GetCurrentWave() {
			return _currentWave;
		}

		public void Dispose() {
			StopLevel();
			_disposer?.Dispose();
			_onWaveStarted?.Dispose();
			_onWaveCompleted?.Dispose();
			_onAllWavesCompleted?.Dispose();
		}
	}
}
