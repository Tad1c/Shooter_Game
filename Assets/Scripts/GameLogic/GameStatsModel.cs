using UniRx;

namespace Scripts {
	public class GameStatsModel : IGameStatsModel {
		private readonly ReactiveProperty<int> _totalEnemiesKilled = new(0);

		public IReadOnlyReactiveProperty<int> TotalEnemiesKilled => _totalEnemiesKilled;

		public void IncrementKills() {
			_totalEnemiesKilled.Value++;
		}

		public void ResetStats() {
			_totalEnemiesKilled.Value = 0;
		}
	}
}

