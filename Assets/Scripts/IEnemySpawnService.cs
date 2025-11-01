using System;
using System.Collections.Generic;
using UniRx;

namespace Scripts
{
    public interface IEnemySpawnService
    {
        IObservable<EnemyPresenter> OnEnemySpawned { get; }
        IObservable<EnemyPresenter> OnEnemyDespawned { get; }
        IReadOnlyReactiveProperty<int> ActiveEnemyCount { get; }

        void StartSpawning();
        void StopSpawning();
        void DespawnAll();
        void SetSpawnParameters(float interval, int enemiesPerSpawn);
        IReadOnlyList<EnemyPresenter> GetActiveEnemies();
    }
}

