using UniRx;

namespace Scripts {
	public interface IGameStatsModel {
		IReadOnlyReactiveProperty<int> TotalEnemiesKilled { get; }
		void IncrementKills();
		void ResetStats();
	}
}

