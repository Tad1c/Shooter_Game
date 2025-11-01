using System;
using UniRx;

namespace Scripts
{
    public interface IWaveManager
    {
        IReadOnlyReactiveProperty<int> CurrentWaveIndex { get; }
        IReadOnlyReactiveProperty<int> EnemiesKilledInWave { get; }
        IReadOnlyReactiveProperty<float> WaveTimer { get; }
        IReadOnlyReactiveProperty<bool> IsWaveActive { get; }

        IObservable<WaveData> OnWaveStarted { get; }
        IObservable<WaveData> OnWaveCompleted { get; }
        IObservable<Unit> OnAllWavesCompleted { get; }

        void StartLevel();
        void StopLevel();
        void NextWave();
        WaveData GetCurrentWave();
    }
}

